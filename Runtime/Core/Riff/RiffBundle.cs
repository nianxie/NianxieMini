using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nianxie.Riff
{
    public class RiffBundle:ScriptableObject
    {
        public string custom { get; private set; }
        public Sprite[] sprites { get; private set; }
        public Object[] binaries { get; private set; }

        public static RiffBundle Create(byte[] riffBytes, Texture2D texture)
        {
            var riffBundle = CreateInstance<RiffBundle>();
            var riffContainer = RiffContainer.Load(riffBytes);
            riffBundle.custom = riffContainer.CustomChunk.GetUtf8String();
            var manifestJson = ManifestJson.Load(riffContainer.ManifestChunk.GetUtf8String());
            riffBundle.sprites = new Sprite[manifestJson.sprites.Length];
            for (int i = 0; i < riffBundle.sprites.Length; i++)
            {
                var meta = manifestJson.sprites[i];
                riffBundle.sprites[i] = Sprite.Create(texture, meta.rect.ToUnityRect(), new Vector2(meta.pivot.x, meta.pivot.y), meta.pixelsPerUnit);
            }

            riffBundle.binaries = new Object[manifestJson.binaries.Length];
            for (int i = 0; i < riffBundle.binaries.Length; i++)
            {
                var meta = manifestJson.binaries[i];
                var chunk = riffContainer.BinaryChunks[i];
                throw new Exception("chunk binary to object TODO");
            }
            return riffBundle;
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