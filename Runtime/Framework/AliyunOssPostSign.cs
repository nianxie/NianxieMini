using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace Nianxie.Framework
{
    public class AliyunOssPostSign
    {
        private static readonly byte[] dash = Encoding.UTF8.GetBytes("--");
        private static readonly byte[] line = Encoding.UTF8.GetBytes("\r\n");
        public string endpoint; 
        public string accessKeyId; 
        public string signature; 
        public string policy;

        public static AliyunOssPostSign HardDecode(string postSign)
        {
            var arr = postSign.Split(" ");
            return new AliyunOssPostSign()
            {
                endpoint = arr[0],
                accessKeyId = arr[1],
                signature = arr[2],
                policy = arr[3],
            };
        }

        private List<IMultipartFormSection> MakeFormSection(byte[] fileData, string fileKey, string mimeType)
        {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            // 添加文本字段
            formData.Add(new MultipartFormDataSection("x-oss-signature-version", "OSS2"));
            formData.Add(new MultipartFormDataSection("x-oss-signature", signature));
            formData.Add(new MultipartFormDataSection("x-oss-access-key-id", accessKeyId));
            formData.Add(new MultipartFormDataSection("policy", policy));
            formData.Add(new MultipartFormDataSection("key", fileKey));
            formData.Add(new MultipartFormFileSection("file", fileData, fileKey, mimeType));
            return formData;
        }

        /// <summary>
        /// unity的UnityWebRequest.Post用formSection list构建的form数据上传阿里云会出错，所以我们手动构建form数据
        /// </summary>
        private byte[] CreateFormWithFileAndBoundary(byte[] fileData, string fileKey, string mimeType, byte[] boundary)
        {
            var multipartFormSections = MakeFormSection(fileData, fileKey, mimeType);
            Func<IMultipartFormSection, byte[]> middle = (section) =>
            {
                string ret;
                string name = section.sectionName;
                string fileName = section.fileName;
                if (string.IsNullOrEmpty(fileName))
                {
                    ret = $"Content-Disposition: form-data; name=\"{name}\"\r\n";
                }
                else
                {
                    var contentType = string.IsNullOrEmpty(section.contentType)?"application/octet-stream":section.contentType;
                    ret = $"Content-Disposition: form-data; name=\"{name}\"; fileName=\"{fileName}\"\r\nContent-Type: {contentType}\r\n";
                }
                return Encoding.UTF8.GetBytes(ret);
            };
            int capacity = 0;
            foreach (IMultipartFormSection multipartFormSection in multipartFormSections)
                capacity += 64 + multipartFormSection.sectionData.Length;
            List<byte> byteList = new List<byte>(capacity);
            foreach (IMultipartFormSection section in multipartFormSections)
            {
                byteList.AddRange(dash);
                byteList.AddRange(boundary);
                byteList.AddRange(line);
                byteList.AddRange(middle(section));
                byteList.AddRange(line);
                byteList.AddRange(section.sectionData);
                byteList.AddRange(line);
            }
            byteList.AddRange((IEnumerable<byte>) dash);
            byteList.AddRange((IEnumerable<byte>) boundary);
            byteList.AddRange((IEnumerable<byte>) dash);
            byteList.AddRange((IEnumerable<byte>) line);
            return byteList.ToArray();
        }

        /// <summary>
        /// 使用PostSign上传一个文件
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="fileKey"></param>
        /// <param name="mimeType"></param>
        /// <returns>返回aliyun的response headers中的md5</returns>
        public async UniTask<string> PostFile(byte[] fileData, string fileKey, string mimeType) 
        {
            // 随机一个boundary并创建post form的请求
            var boundary = UnityWebRequest.GenerateBoundary();
            var request = new UnityWebRequest(endpoint, "POST");
            var contentType = $"multipart/form-data; boundary={Encoding.UTF8.GetString(boundary)}";
            request.SetRequestHeader("Content-Type", contentType);
            request.downloadHandler = new DownloadHandlerBuffer();
            var formBody = CreateFormWithFileAndBoundary(fileData, fileKey, mimeType, boundary);
            request.uploadHandler = new UploadHandlerRaw(formBody);
            request.uploadHandler.contentType = contentType;
            await request.SendWebRequest().ToUniTask();
            return request.GetResponseHeader("Content-MD5");
        }
    }
}

