using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using Nianxie.Utils;
using UnityEngine;

namespace Nianxie.Craft
{
    public class PngPackContext:AbstractPackContext
    {
        public (LargeBytes, byte[]) DumpJsonPng()
        {
            var altasSize = craftJson.atlasSize;
            RenderTexture tempRT = new RenderTexture(altasSize.x, altasSize.y, 0, RenderTextureFormat.ARGB32);
            RenderTexture previousRT = RenderTexture.active;
            RenderTexture.active = tempRT;
            // 清空白画布
            GL.Clear(true, true, Color.clear);
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, altasSize.x, altasSize.y, 0);

            // 遍历所有纹理并绘制到目标上
            for (int i = 0; i < spriteList.Count; i++)
            {
                var spriteInfo = spriteList[i];
            
                // 绘制纹理到指定位置和大小
                var packRect = spriteInfo.atlasRect;
                var cropRect = spriteInfo.cropRect;
                var tex = spriteInfo.sourceTex;
                Graphics.DrawTexture(new Rect(packRect.x, altasSize.y-packRect.y-packRect.height, packRect.width, packRect.height), tex, new Rect(1.0f*cropRect.x/tex.width,1.0f*cropRect.y/tex.height, 1.0f*cropRect.width/tex.width,1.0f*cropRect.height/tex.height),0,0,0,0);
            }
            GL.PopMatrix();
        
            // 将RenderTexture转换为Texture2D
            Texture2D resultTexture = new Texture2D(altasSize.x, altasSize.y, TextureFormat.ARGB32, false);
            resultTexture.ReadPixels(new Rect(0, 0, altasSize.x, altasSize.y), 0, 0);
            resultTexture.Apply();
        
            // 清理
            RenderTexture.active = previousRT;
            UnityEngine.Object.Destroy(tempRT);

            var pngData = resultTexture.EncodeToPNG();
            UnityEngine.Object.Destroy(resultTexture);
            var jsonBytes = craftJson.ToLargeBytes();
            return (jsonBytes, pngData);
        }
    }
}