using System;
using System.Linq;
using Nianxie.Utils;
using UnityEngine;
using WebP;
using Nianxie.Craft;
using Nianxie.Riff;

namespace Nianxie.Preview
{
    public class SimplePackContext:AbstractPackContext
    {
        private byte[] PackAtlasWebp(IntRectangle[] atlasPackRectArr, Vector2Int atlasSize)
        {
            RenderTexture tempRT = new RenderTexture(atlasSize.x, atlasSize.y, 0, RenderTextureFormat.ARGB32);
            RenderTexture previousRT = RenderTexture.active;
            RenderTexture.active = tempRT;
            // 清空白画布
            GL.Clear(true, true, Color.clear);
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, atlasSize.x, 0, atlasSize.y); // 注意，如果使用png保存这里需要改成GL.LoadPixelMatrix(0, atlasSize.x, atlasSize.y, 0); 但webp会上下颠倒，所以这里这么写。

            // 遍历所有纹理并绘制到目标上
            for (int i = 0; i < atlasSpriteList.Count; i++)
            {
                var spriteInfo = atlasSpriteList[i];
                var packRect = atlasPackRectArr[i];
                var spriteRect = spriteInfo.sprite.rect;
                var tex = spriteInfo.sprite.texture;
                Graphics.DrawTexture(new Rect(packRect.x, atlasSize.y-packRect.y-packRect.height, packRect.width, packRect.height), tex, new Rect(1.0f*spriteRect.x/tex.width,1.0f*spriteRect.y/tex.height, 1.0f*spriteRect.width/tex.width,1.0f*spriteRect.height/tex.height),0,0,0,0);
                // 绘制纹理到指定位置和大小
                //var packRect = spriteInfo.atlasRect;
                //var tex = spriteInfo.sourceTex;
                //Graphics.DrawTexture(new Rect(packRect.x, atlasSize.y-packRect.y-packRect.height, packRect.width, packRect.height), tex, new Rect(1.0f*cropRect.x/tex.width,1.0f*cropRect.y/tex.height, 1.0f*cropRect.width/tex.width,1.0f*cropRect.height/tex.height),0,0,0,0);
            }
            GL.PopMatrix();
        
            // 将RenderTexture转换为Texture2D
            Texture2D resultTexture = new Texture2D(atlasSize.x, atlasSize.y, TextureFormat.ARGB32, false);
            resultTexture.ReadPixels(new Rect(0, 0, atlasSize.x, atlasSize.y), 0, 0);
            resultTexture.Apply();
        
            // 清理
            RenderTexture.active = previousRT;
            UnityEngine.Object.Destroy(tempRT);

            var webpData = resultTexture.EncodeToWebP(90, out var err);
            if (err != Error.Success)
            {
                throw new Exception($"encode to webp failed : {err}");
            }
            UnityEngine.Object.Destroy(resultTexture);
            return webpData;
        }

        public byte[] PackRoot(SlotBehaviour rootSlot)
        {
            var rootJson = (SlotBehavJson)rootSlot.PackToJson(this);
            RectanglePacker.PackFromVec2s(atlasSpriteList.Select(s => s.size).ToArray(), out var packRectArr, out var atlasSize);
            var webpData = PackAtlasWebp(packRectArr, atlasSize);
            var manifestJson = new ManifestJson()
            {
                sprites = atlasSpriteList.Select((s,i)=>new ManifestJson.SpriteMeta()
                {
                    rect=packRectArr[i],
                    pivot=s.pivot,
                    pixelsPerUnit=s.sprite.pixelsPerUnit,
                }).ToArray(),
                binaries = binaryList.Select(a=>new ManifestJson.BinaryMeta()
                {
                    ext=a.ext,
                }).ToArray(),
            };
            var craftJson = new CraftJson()
            {
                root = rootJson,
            };
            var packBytes = RiffFile.Pack(webpData, craftJson, manifestJson, binaryList.Select(a=>a.data).ToList());
            return packBytes;
        }
    }
}