using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Nianxie.Riff;
using UnityEngine;
using WebP;

namespace Nianxie.Craft
{
    public class PackContext:IPackContext
    {
        private List<TypedBinary> binaryList = new();
        private class TypedBinary
        {
            public string ext;
            public byte[] data;
        }

        private AssetUsageCenter usageCenter;
        public PackContext(AssetUsageCenter usageCenter)
        {
            this.usageCenter = usageCenter;
        }

        public async UniTask<byte[]> PackRoot(SlotBehaviour rootSlot)
        {
            // 1. 打包webp，同时为TextureUsage分配packRiffIndex
            var texUsages = usageCenter.textureUsageCollection.Where(usage => usage.sourceKind is PackableSourceKind).ToArray();
            RectanglePacker.PackFromVec2s(texUsages.Select(usage => usage.texRegion.size).ToArray(), out var packRectArr, out var atlasSize);
            if (atlasSize == Vector2Int.zero)
            {
                atlasSize = new Vector2Int(1, 1);
            }

            for (int i = 0; i < texUsages.Length; i++)
            {
                (texUsages[i].sourceKind as PackableSourceKind)!.packRiffIndex = i;
            }
            var webpData = await PackAtlasWebp(texUsages.Select(usage=>usage.texRegion).ToArray(), packRectArr, atlasSize);
            
            // 2. 计算craftRiffJson
            var rootJson = rootSlot.TypedPackToJson(this);
            var craftJson = new CraftRiffJson()
            {
                root = rootJson,
            };
            
            // 3. 计算manifestRiffJson
            var manifestRiffJson = new ManifestRiffJson()
            {
                regions=packRectArr.Select(r=>new ManifestRiffJson.RegionMeta()
                {
                    rect=r,
                }).ToArray(),
                // binaries TODO
                binaries=binaryList.Select(a=>new ManifestRiffJson.BinaryMeta()
                {
                    ext=a.ext,
                }).ToArray(),
            };
            var packBytes = RiffPackage.Pack(webpData, craftJson, manifestRiffJson, binaryList.Select(a=>a.data).ToList());
            return packBytes;
        }
        
        protected async UniTask<byte[]> PackAtlasWebp(TextureRegion[] texRegions, IntRectangle[] atlasPackRectArr, Vector2Int atlasSize)
        {
            RenderTexture tempRT = new RenderTexture(atlasSize.x, atlasSize.y, 0, RenderTextureFormat.ARGB32);
            RenderTexture previousRT = RenderTexture.active;
            RenderTexture.active = tempRT;
            // 清空白画布
            GL.Clear(true, true, Color.clear);
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, atlasSize.x, 0, atlasSize.y); // 注意，如果使用png保存这里需要改成GL.LoadPixelMatrix(0, atlasSize.x, atlasSize.y, 0); 但webp会上下颠倒，所以这里这么写。

            // 遍历所有纹理并绘制到目标上
            for (int i = 0; i < texRegions.Length; i++)
            {
                var texRegion = texRegions[i];
                var packRect = atlasPackRectArr[i];
                var spriteRect = texRegion.rect;
                var tex = texRegion.texture;
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

        public int PutSprite(Sprite sprite)
        {
            throw new NotImplementedException();
        }

        public int PutBinary(string ext, byte[] binary)
        {
            throw new NotImplementedException();
        }
    }
}