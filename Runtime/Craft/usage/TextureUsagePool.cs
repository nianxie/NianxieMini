using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Nianxie.Framework;
using Nianxie.Riff;
using UnityEngine;
using WebP;
using XLua;

namespace Nianxie.Craft
{
    public class TextureUsagePool: UsagePool<TextureUsage>
    {
        public TextureUsagePool(CraftManager craftManager) 
        {
            releaseFn = craftManager.ShellRelease;
        }
        public TextureUsage AddByUpload(Texture2D uploadTex)
        {
            var sourceKind = new UploadSourceInfo(this);
            var texRegion = new TextureRegion(uploadTex, new IntRectangle(0, 0, uploadTex.width, uploadTex.height));
            var usage = new TextureUsage(sourceKind, texRegion);
            AddUsage(usage);
            return usage;
        }
        public void AddByBuiltin(Sprite sprite, string builtinPath)
        {
            var rect = new IntRectangle(Mathf.RoundToInt(sprite.textureRect.x), Mathf.RoundToInt(sprite.textureRect.y), Mathf.RoundToInt(sprite.textureRect.width), Mathf.RoundToInt(sprite.textureRect.height));
            var texture = sprite.texture;
            var sourceKind = new BuiltinSourceInfo(this, builtinPath);
            var texRegion = new TextureRegion(texture, rect);
            var usage = new TextureUsage(sourceKind, texRegion);
            AddUsage(usage);
        }
        public void AddByRiff(TextureRegion texRegion, int riffIndex)
        {
            var sourceKind = new RiffSourceInfo(this, riffIndex);
            var usage = new TextureUsage(sourceKind, texRegion);
            AddUsage(usage);
        }

        public async UniTask<(ManifestRiffJson.RegionMeta[], byte[])> PackRegionsAndWebp()
        {
            var texUsages = PreparePackableUsages();
            RectanglePacker.PackFromVec2s(texUsages.Select(usage => usage.texRegion.size).ToArray(), out var packRectArr, out var atlasSize);
            if (atlasSize == Vector2Int.zero)
            {
                atlasSize = new Vector2Int(1, 1);
            }
            
            var webpData = await PackAtlasWebp(texUsages.Select(usage=>usage.texRegion).ToArray(), packRectArr, atlasSize);
            var regionArr = packRectArr.Select(r => new ManifestRiffJson.RegionMeta()
            {
                rect = r,
            }).ToArray();
            return (regionArr, webpData);
        }
        
        private async UniTask<byte[]> PackAtlasWebp(TextureRegion[] texRegions, IntRectangle[] atlasPackRectArr, Vector2Int atlasSize)
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
    }
}