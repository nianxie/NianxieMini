using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Net;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace Nianxie.Editor
{
    public static class AccountController
    {
        private const string MIME_BIN = "application/octet-stream";
        private const string MIME_JSON = "application/json";
        private const string MIME_JPEG = "image/jpeg";
        //private const string SERVER_URL = "http://127.0.0.1:5239";
        private const string SERVER_URL = "http://192.168.1.5:10080";

        private static string URL_SIGNIN => $"{SERVER_URL}/api/account/sign/UnitySignin";
        private static string URL_LIST => $"{SERVER_URL}/api/mini/List";
        private static string URL_DELETE => $"{SERVER_URL}/api/mini/Delete";
        private static string URL_BEGIN_UPLOAD => $"{SERVER_URL}/api/mini/BeginUpload";
        private static string URL_END_UPLOAD => $"{SERVER_URL}/api/mini/EndUpload";

        private static string token = "";
        public static bool signinRunning = false;
        public static readonly List<DB_Mini> dbMiniDatas = new();
        public static bool signed => !string.IsNullOrEmpty(token);
        
        static AccountController()
        {
            token = EditorPrefs.GetString(nameof(token));
        }

        public static async UniTask Signin(string accountName)
        {
            signinRunning = true;
            try{
                token = await Post<string>(URL_SIGNIN, $"{{\"accountName\":\"{accountName}\"}}");
                EditorPrefs.SetString(nameof(token), token);
            }finally
            {
                signinRunning = false;
            }
        }

        public static void Signout()
        {
            token = "";
            EditorPrefs.SetString(nameof(token), token);
        }

        public static async UniTask DeleteMini(string miniId)
        {
            await Post<string>($"{URL_DELETE}/{miniId}");
        }

        public static async UniTask<DB_Mini[]> List(int pageNum)
        {
            var pageSize = 10;
            var data = await Get($"{URL_LIST}?pageNum={pageNum}&pageSize={pageSize}");
            return JsonUtility.FromJson<MiniPaginationResponse>(data).itemList;
        }

        public static async UniTask UploadBundle(Texture2D thumbnailTex, MiniProjectConfig config, MiniEditorEnvPaths envPaths, Action<string, int, int> onFileProgress)
        {
            //var files = envPaths.finalBundleDict.Values.ToArray();
            //var maxFileSize = files.Select(e => new FileInfo(e).Length).Max();
            //if (maxFileSize > beginResp.sizeLimit)
            //{
                //Debug.LogError("文件过大, TODO, 使用recompress之后的尺寸");
                //return;
            //}
            Dictionary<BuildTarget, string> bundleDict = new();

            var beginResp = await Post<MiniBeginUploadResponse>($"{URL_BEGIN_UPLOAD}");
            foreach (var buildTarget in envPaths.finalBundleDict.Keys)
            {
                bundleDict[buildTarget] = await envPaths.ExecuteRename(beginResp.miniId, buildTarget);
            }

            var postSign = AliyunOssPostSign.HardDecode(beginResp.postSign);
            var key_file_type= new List<(string, string, string)>
            {
                (beginResp.iosFileKey, bundleDict[BuildTarget.iOS], MIME_BIN),
                (beginResp.androidFileKey, bundleDict[BuildTarget.Android], MIME_BIN),
                (beginResp.webglFileKey, bundleDict[BuildTarget.WebGL], MIME_BIN),
            };
            // var fileCount = key_file_type.Count;
            // TODO 如果有缩略图，则上传缩略图
            if (thumbnailTex == null)
            {
                //fileCount++;
                //Debug.Log("TODO, 用Texture Load一下以检查缩略图的合法性，并encode为jpg提交。");
                //key_file_type.Insert(0, (beginResp.thumbnailFileKey, thumbnailFilePath, MIME_JPEG));
            }
            for(int i=0;i<key_file_type.Count;i++)
            {
                var (key, file, type) = key_file_type[i];
                onFileProgress(file, i + 1, key_file_type.Count);
                var fileBytes = await File.ReadAllBytesAsync(file);
                var respMd5 = await postSign.PostFile(fileBytes, key, type);
                var fileMd5 = new LargeBytes(fileBytes).Md5Base64();
                if (fileMd5 == respMd5)
                {
                    Debug.Log($"文件 {file} 上传成功");
                }
                else
                {
                    throw new Exception("上传异常，md5不一致");
                }
            }

            await Post<string>($"{URL_END_UPLOAD}", JsonUtility.ToJson(new MiniEndUploadRequest
            {
                session = beginResp.session,
                name = config.name,
                craftable = config.craftable,
                thumbnailUploaded = false,
                miniVersion = config.miniVersion,
                unityVersion = config.unityVersion,
            }));
            onFileProgress("", key_file_type.Count, key_file_type.Count);
        }
        
        private static async UniTask<string> Get(string url)
        {
            Dictionary<string, string> headers = new();
            if (!string.IsNullOrEmpty(token))
            {
                headers["Authorization"] = $"Bearer {token}";
            }
            var request = new UnityWebRequest(url, "GET");
            foreach (var pair in headers)
            {
                request.SetRequestHeader(pair.Key, pair.Value as string);
            }
            request.downloadHandler = new DownloadHandlerBuffer();
            try
            {
                await request.SendWebRequest().ToUniTask();
            } catch(UnityWebRequestException e)
            {
                if (e.ResponseCode == (int)HttpStatusCode.Unauthorized)
                {
                    token = null;
                }
                throw;
            }
            return request.downloadHandler.text;
        }

        private static async UniTask<TResponse> Post<TResponse>(string url, string body="")
        {
            var request = new UnityWebRequest(url, "POST");
            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", $"Bearer {token}");
            }

            request.SetRequestHeader("Content-Type", MIME_JSON);
            request.downloadHandler = new DownloadHandlerBuffer();
            if (body != null)
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                request.uploadHandler.contentType = MIME_JSON;
            }
            try
            {
                await request.SendWebRequest().ToUniTask();
            } catch(UnityWebRequestException e)
            {
                if (e.ResponseCode == (int)HttpStatusCode.Unauthorized)
                {
                    token = null;
                }
                throw;
            }
            var retText = request.downloadHandler.text;
            if (retText is TResponse resp)
            {
                return resp;
            }
            else
            {
                return (TResponse)JsonUtility.FromJson(retText, typeof(TResponse));
            }
        }
    }
}
