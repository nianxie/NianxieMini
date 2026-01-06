using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nianxie.Riff
{
    public class RiffPackage:ScriptableObject
    {
        public CustomJson customJson { get; private set; }
        public Sprite[] sprites { get; private set; }
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
            riffPackage.customJson = JsonCodec.Load<CustomJson>(riffContainer.CustomChunk.GetUtf8String());
            var manifestJson = JsonCodec.Load<ManifestJson>(riffContainer.ManifestChunk.GetUtf8String());
            riffPackage.sprites = new Sprite[manifestJson.sprites.Length];
            for (int i = 0; i < riffPackage.sprites.Length; i++)
            {
                var meta = manifestJson.sprites[i];
                riffPackage.sprites[i] = Sprite.Create(texture, meta.rect.ToUnityRect(), new Vector2(meta.pivot.x, meta.pivot.y), meta.pixelsPerUnit);
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
            Debug.Log("Riff Bundle on Destroy");
            if (sprites != null)
            {
                for (int i = 0; i < sprites.Length; i++)
                {
                    Destroy(sprites[i]);
                }
                sprites = null;
            }

            if (binaries != null)
            {
                for (int i = 0; i < binaries.Length; i++)
                {
                    Destroy(binaries[i]);
                }
                binaries = null;
            }
        }
    }
}