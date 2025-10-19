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
        //private const string SERVER_URL = "http://127.0.0.1:5239";
        private const string SERVER_URL = "http://39.107.44.97:10080";

        private static string URL_SIGNIN => $"{SERVER_URL}/api/account/sign/UnitySignin";
        private static string URL_LIST => $"{SERVER_URL}/api/mini/List";
        private static string URL_CREATE => $"{SERVER_URL}/api/mini/Create";
        private static string URL_DELETE => $"{SERVER_URL}/api/mini/Delete";
        private static string URL_BEGIN_UPLOAD => $"{SERVER_URL}/api/mini/BeginUpload";
        private static string URL_END_UPLOAD => $"{SERVER_URL}/api/mini/EndUpload";
        private static string URL_SYNC_CONFIG => $"{SERVER_URL}/api/mini/SyncConfig";

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
            var reqData = JsonUtility.ToJson(new MiniCreateRequest(commonConfig));
            var dbMini = await Post<DB_Mini>(URL_CREATE, reqData);
            await RefreshList();
            return dbMini;
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

        public static async UniTask UploadBundle(string miniId, MiniEditorEnvPaths envPaths, Action<string, int, int> onFileProgress)
        {
            var files = new []
            {
                envPaths.finalManifest, envPaths.finalBundleDict[BuildTarget.iOS], envPaths.finalBundleDict[BuildTarget.Android]
            };
            var maxFileSize = files.Select(e => new FileInfo(e).Length).Max();

            var beginResp = await Post<MiniBeginUploadResponse>($"{URL_BEGIN_UPLOAD}", JsonUtility.ToJson(new MiniBeginUploadRequest()
            {
                miniId=miniId,
                fileSize=(int)maxFileSize,
            }));
            var postSign = AliyunOssPostSign.HardDecode(beginResp.postSign);
            var key_file_type= new []
            {
                (beginResp.manifestFileKey, envPaths.finalManifest, MIME_JSON),
                (beginResp.iosFileKey, envPaths.finalBundleDict[BuildTarget.iOS], MIME_BIN),
                (beginResp.androidFileKey, envPaths.finalBundleDict[BuildTarget.Android], MIME_BIN),
            };
            //foreach (var (key, file, type) in key_file_Type)
            for(int i=0;i<key_file_type.Length;i++)
            {
                var (key, file, type) = key_file_type[i];
                onFileProgress(key, i + 1, key_file_type.Length);
                var fileBytes = await File.ReadAllBytesAsync(file);
                var respMd5 = await postSign.PostFile(fileBytes, key, type);
                using (MD5 md5 = MD5.Create())
                {
                    // 计算字节数组的哈希值
                    var fileMd5 = Convert.ToBase64String(md5.ComputeHash(fileBytes));
                    if (fileMd5 == respMd5)
                    {
                        Debug.Log($"文件 {file} 上传成功");
                    }
                    else
                    {
                        throw new Exception("上传异常，md5不一致");
                    }
                }
            }

            await Post<string>($"{URL_END_UPLOAD}", JsonUtility.ToJson(new MiniEndUploadRequest(miniId, envPaths.config)));
            onFileProgress("", key_file_type.Length, key_file_type.Length);
            await RefreshList();
        }
        
        public static async UniTask SyncConfig(string miniId, MiniEditorEnvPaths envPaths)
        {
            await Post<string>($"{URL_SYNC_CONFIG}", JsonUtility.ToJson(new MiniSyncConfigRequest(miniId, envPaths.config)));
            await RefreshList();
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
    }
}
