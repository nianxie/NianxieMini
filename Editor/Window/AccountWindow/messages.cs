using System;
using System.Collections.Generic;
using Nianxie.Framework;

namespace Nianxie.Editor
{
    [Serializable]
    public class DB_Mini
    {
        public string miniId;
        public string accountId;
        public string storyId;
        public string createTime;
        public string name;
        public bool craftable;
        public string miniVersion;
        public string unityVersion;
        public string thumbnailUrl;
        public string androidUrl;
        public string iosUrl;
        public string webglUrl;
        public bool used;
        public bool packageReady;
        public string packageReadyTime;
        public string packageUrl;
    }

    [Serializable]
    public class MiniPaginationResponse
    {
        public int pageNum;
        public int pageSize;
        public DB_Mini[] itemList;
    }
    
    [Serializable]
    public class MiniBeginUploadResponse
    {
        public string session;
        public string miniId;
        public int sizeLimit;
        public string thumbnailFileKey;
        public string androidFileKey;
        public string iosFileKey;
        public string webglFileKey;
        public string postSign;
    }

    [Serializable]
    public class MiniEndUploadRequest
    {
        public string session;
        public string name;
        public bool craftable;
        public bool thumbnailUploaded;
        public string miniVersion;
        public string unityVersion;
    }
}