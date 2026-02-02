using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nianxie.Riff
{
    public class RiffPackage:ScriptableObject
    {
        public CustomRiffJson custom { get; private set; }
        public Sprite[] sprites { get; private set; }
        public TextureRegion[] texRegions { get; private set; }
        public Object[] binaries { get; private set; }

        /// <summary>
        /// RiffPackage的创建入口，TODO 加入Audio支持并做成异步的。 
        /// </summary>
        /// <param name="riffBytes"></param>
        /// <param name="texture"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async UniTask<RiffPackage> Create(byte[] riffBytes, Texture2D texture)
        {
            var riffPackage = CreateInstance<RiffPackage>();
            var riffContainer = RiffFile.Load(riffBytes);
            riffPackage.custom = riffContainer.CustomChunk.GetAsJson<CustomRiffJson>();
            var manifestJson = riffContainer.ManifestChunk.GetAsJson<ManifestRiffJson>();
            riffPackage.texRegions = new TextureRegion[manifestJson.regions.Length];
            for (int i = 0; i < riffPackage.texRegions.Length; i++)
            {
                riffPackage.texRegions[i] = new TextureRegion(texture, manifestJson.regions[i].rect);
            }

            riffPackage.binaries = new Object[manifestJson.binaries.Length];
            for (int i = 0; i < riffPackage.binaries.Length; i++)
            {
                var meta = manifestJson.binaries[i];
                var chunk = riffContainer.BinaryChunks[i];
                throw new Exception("chunk binary to object TODO");
            }
            return riffPackage;
        }

        private void OnDestroy()
        {
            if (binaries != null)
            {
                for (int i = 0; i < binaries.Length; i++)
                {
                    Destroy(binaries[i]);
                }
                binaries = null;
            }
        }
        public static byte[] Pack(byte[] webpData, CustomRiffJson customRiffJson, ManifestRiffJson manifestRiffJson, List<byte[]> binaries)
        {
            if (binaries != null)
            {
                UnityEngine.Assertions.Assert.IsTrue(manifestRiffJson.binaries.Length==binaries.Count, "binaries count not match when riff pack");
            }
            else
            {
                UnityEngine.Assertions.Assert.IsTrue(manifestRiffJson.binaries.Length==0, "binaries count not match when riff pack");
            }

            var riffFile = RiffFile.Load(webpData);
            riffFile.CustomChunk.SetAsJson(customRiffJson);
            riffFile.ManifestChunk.SetAsJson(manifestRiffJson);
            riffFile.BinaryChunks.Clear();
            for(int i=0;i<manifestRiffJson.binaries.Length;i++)
            {
                riffFile.BinaryChunks.Add(new RiffChunk(RiffFile.NX_BINARY_UINT, binaries[i]));
            }
            return riffFile.Dump();
        }
    }
}