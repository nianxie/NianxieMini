using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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

        public static async UniTask<DB_Mini> CreateMini(MiniCommonConfig commonConfig)
        {
            throw new NotImplementedException("not implement");
        }
        
        public static async UniTask DeleteMini(string miniId)
        {
            await Post<string>($"{URL_DELETE}/{miniId}");
            await RefreshList();
        }
        
        private static async UniTask<MiniPaginationResponse> GetPagination(int pageNum, int pageSize)
        {
            var data = await Get($"{URL_LIST}?pageNum={pageNum}&pageSize={pageSize}");
            return JsonUtility.FromJson<MiniPaginationResponse>(data);
        }

        public static async UniTask RefreshList()
        {
            var pagination = await GetPagination(1, 10);
            dbMiniDatas.Clear();
            for (int i = 0; i < pagination.itemList.Length; i++)
            {
                var item = pagination.itemList[i];
                dbMiniDatas.Add(item);
            }
        }

        public static async UniTask UploadBundle(string thumbnailFilePath, MiniEditorEnvPaths envPaths, Action<string, int, int> onFileProgress)
        {
            var files = new []
            {
                envPaths.finalManifest, envPaths.finalBundleDict[BuildTarget.iOS], envPaths.finalBundleDict[BuildTarget.Android]
            };
            var maxFileSize = files.Select(e => new FileInfo(e).Length).Max();

            var beginResp = await Post<MiniBeginUploadResponse>($"{URL_BEGIN_UPLOAD}");
            if (maxFileSize > beginResp.sizeLimit)
            {
                Debug.LogError("文件过大, TODO, 使用recompress之后的尺寸");
                return;
            }

            var postSign = AliyunOssPostSign.HardDecode(beginResp.postSign);
            var key_file_type= new List<(string, string, string)>
            {
                (beginResp.iosFileKey, envPaths.finalBundleDict[BuildTarget.iOS], MIME_BIN),
                (beginResp.androidFileKey, envPaths.finalBundleDict[BuildTarget.Android], MIME_BIN),
                (beginResp.webglFileKey, envPaths.finalBundleDict[BuildTarget.Android], MIME_BIN),
            };
            // var fileCount = key_file_type.Count;
            // TODO 如果有缩略图，则上传缩略图
            if (!string.IsNullOrEmpty(thumbnailFilePath))
            {
                //fileCount++;
                //Debug.Log("TODO, 用Texture Load一下以检查缩略图的合法性，并encode为jpg提交。");
                //key_file_type.Insert(0, (beginResp.thumbnailFileKey, thumbnailFilePath, MIME_JPEG));
            }
            for(int i=0;i<key_file_type.Count;i++)
            {
                var (key, file, type) = key_file_type[i];
                onFileProgress(key, i + 1, key_file_type.Count);
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
                name = envPaths.config.name,
                craftable = envPaths.config.craftable,
                thumbnailUploaded = false,
                miniVersion = NianxieConst.MINI_VERSION,
                unityVersion = Application.unityVersion,
            }));
            onFileProgress("", key_file_type.Count, key_file_type.Count);
            await RefreshList();
        }
        
        public static async UniTask SyncConfigs(DB_Mini syncMini)
        {
            throw new NotImplementedException("ignore this api");
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
        private static void ReplaceFolderMeta(string folderPath, string oldGuid, string newGuid)
        {
            var folderMeta = $"{folderPath}.meta";
            if (oldGuid.Length==32 && Directory.Exists(folderPath))
            {
                var newMeta = File.ReadAllText(folderMeta).Replace($"guid: {oldGuid}", $"guid: {newGuid}");
                File.WriteAllText(folderMeta, newMeta);
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError($"{folderPath} is not a valid project");
            }
        }

        public static void LinkFolder(DB_Mini dbMini, string folder)
        {
            var miniId = dbMini.miniId;
            var folderPath = $"{NianxieConst.MiniPrefixPath}/{folder}";
            var conflictPath = AssetDatabase.GUIDToAssetPath(miniId);
            if (!string.IsNullOrEmpty(conflictPath) && conflictPath != folderPath)
            {
                File.Delete($"{conflictPath}.meta");
            }
            var oldGuid = AssetDatabase.AssetPathToGUID(folderPath);
            if (oldGuid != miniId)
            {
                ReplaceFolderMeta(folderPath, oldGuid, miniId);
            }
        }
        public static void UnlinkFolder(DB_Mini dbMini)
        {
            var miniId = dbMini.miniId;
            var folderPath = AssetDatabase.GUIDToAssetPath(miniId);
            var folder = Path.GetFileName(folderPath);
            if (folderPath == $"{NianxieConst.MiniPrefixPath}/{folder}")
            {
                ReplaceFolderMeta(folderPath, miniId, Guid.NewGuid().ToString("N"));
            }
        }
        public static bool TryMapLinkedFolder(DB_Mini dbMini, out string folderPath, out string folderName)
        {
            folderPath = AssetDatabase.GUIDToAssetPath(dbMini.miniId);
            var folder = Path.GetFileName(folderPath??"");
            if (folderPath == $"{NianxieConst.MiniPrefixPath}/{folder}" && Directory.Exists(folderPath))
            {
                folderName = folder;
                return true;
            }
            else
            {
                folderName = null;
                return false;
            }
        }
    }
}
