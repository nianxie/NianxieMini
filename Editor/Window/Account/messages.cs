using System;
using Nianxie.Framework;

namespace Nianxie.Editor
{
    [Serializable]
    public class DB_Mini: MiniCommonConfig
    {
        public const int STATUS_INIT = 0;
        public const int STATUS_UPLOADED = 1;
        public const int STATUS_VIDEO_USED= 2;
        public string miniId;
        public string accountId;
        public string videoId;
        public string createTime;
        //public string name;
        //public bool craftable;
        //public int majorVersion;
        //public int minorVersion;
        //public int patchVersion;
        public int readyStatus;
        public string readyToken;
        public string manifestUrl;
        public string androidUrl;
        public string iosUrl;
        public bool packageReady;
        public string packageReadyToken;
        public string packageUrl;
        public bool deleted;
    }

    [Serializable]
    public class MiniCreateRequest:MiniCommonConfig
    {
        public MiniCreateRequest(MiniCommonConfig commonConfig) : base(commonConfig)
        {
        }
    }

    [Serializable]
    public class MiniList
    {
        public int pageNum;
        public int pageSize;
        public DB_Mini[] itemList;
        public int totalPages;
    }
    
    [Serializable]
    public class MiniPaginationResponse
    {
        public int pageNum;
        public int pageSize;
        public DB_Mini[] itemList;
        public int totalPages;
    }
    
    [Serializable]
    public class MiniBeginUploadRequest
    {
        public string miniId;
        public int fileSize;
    }
    
    [Serializable]
    public class MiniBeginUploadResponse
    {
        public string readyTime;
        public string manifestFileKey;
        public string androidFileKey;
        public string iosFileKey;
        public string postSign;
    }
    
    [Serializable]
    public class MiniSyncConfigRequest:MiniCommonConfig
    {
        public string miniId;
        public MiniSyncConfigRequest(string miniId, MiniCommonConfig commonConfig) : base(commonConfig)
        {
            this.miniId = miniId;
        }
    }
    [Serializable]
    public class MiniEndUploadRequest:MiniCommonConfig
    {
        public string miniId;
        public MiniEndUploadRequest(string miniId, MiniCommonConfig commonConfig) : base(commonConfig)
        {
            this.miniId = miniId;
        }
    }
}