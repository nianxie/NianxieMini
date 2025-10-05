using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XLua;

namespace Nianxie.Editor
{
    public class TextureBackupSettings
    {
        public readonly TextureImporterPlatformSettings defaultSettings;
        public readonly TextureImporterPlatformSettings Standalone;
        public readonly TextureImporterPlatformSettings iOS;
        public readonly TextureImporterPlatformSettings Android;
        public readonly TextureImporterPlatformSettings WebGL;
        public TextureImporterPlatformSettings[] byPlatformSettings => new[]
        {
            Standalone, iOS, Android, WebGL
        };

        public TextureBackupSettings(TextureImporter importer)
        {
            defaultSettings = importer.GetDefaultPlatformTextureSettings();
            Standalone = importer.GetPlatformTextureSettings(nameof(Standalone));
            iOS = importer.GetPlatformTextureSettings(nameof(iOS));
            Android = importer.GetPlatformTextureSettings(nameof(Android));
            WebGL = importer.GetPlatformTextureSettings(nameof(WebGL));
        }
    }

    public class CollectNotScript:AbstractCollectAsset
    {
        public bool isExplicit { get; private set; }
        public bool canWebP => backup != null;
        private TextureBackupSettings backup;
        public void OverrideSettingsWithRGBA32()
        {
            Debug.LogError("TODO here");
            // TODO 后续改成ASTC
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (backup.defaultSettings.format!=TextureImporterFormat.RGBA32 || backup.defaultSettings.maxTextureSize > 1024)
            {
                var format = backup.defaultSettings.format;
                var maxSize = backup.defaultSettings.maxTextureSize;
                backup.defaultSettings.format = TextureImporterFormat.RGBA32;
                backup.defaultSettings.maxTextureSize = 1024;
                importer!.SetPlatformTextureSettings(backup.defaultSettings);
                backup.defaultSettings.format = format;
                backup.defaultSettings.maxTextureSize = maxSize;
            }

            foreach (var platformSettings in backup.byPlatformSettings)
            {
                if (platformSettings.overridden)
                {
                    platformSettings.overridden = false;
                    importer!.SetPlatformTextureSettings(platformSettings);
                    platformSettings.overridden = true;
                }
            }
            importer!.SaveAndReimport();
        }

        public void RollbackSettings()
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (backup.defaultSettings.format!=TextureImporterFormat.RGBA32)
            {
                importer!.SetPlatformTextureSettings(backup.defaultSettings);
            }

            foreach (var platformSettings in backup.byPlatformSettings)
            {
                if (platformSettings.overridden)
                {
                    importer!.SetPlatformTextureSettings(platformSettings);
                }
            }
            importer!.SaveAndReimport();
        }

        private static CollectNotScript Create(bool isExplicit, string assetPath)
        {
            TextureBackupSettings backup = null;
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                backup = new TextureBackupSettings(importer);
            }
            return new CollectNotScript
            {
                path=assetPath,
                guid=AssetDatabase.AssetPathToGUID(assetPath),
                isExplicit=isExplicit,
                backup=backup,
            };
        }
        public static Dictionary<string, CollectNotScript> Collect(EditorReflectEnv reflectEnv)
        {
            // 1. collect explicit
            HashSet<string> explicitPathSet = new();
            var guids = AssetDatabase.FindAssets("t:Prefab", new [] {reflectEnv.envPaths.luafabPathPrefix});
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                explicitPathSet.Add(assetPath);
            }
            foreach (var assetPath in reflectEnv.CollectReferenceAssetPaths())
            {
                explicitPathSet.Add(assetPath);
            }

            // 2. collect dependencies
            HashSet<string> implicitPathSet = new();
            foreach (var assetPath in explicitPathSet)
            {
                foreach (var dependPath in AssetDatabase.GetDependencies(assetPath, true))
                {
                    if (!explicitPathSet.Contains(dependPath))
                    {
                        implicitPathSet.Add(dependPath);
                    }
                }
            }
            implicitPathSet = implicitPathSet.Where(Filter).ToHashSet();
            
            // 3. build info
            var collectDict = new Dictionary<string, CollectNotScript>();
            foreach (var assetPath in explicitPathSet)
            {
                collectDict[assetPath] = Create(true, assetPath);
            }
            foreach (var assetPath in implicitPathSet)
            {
                if (!collectDict.ContainsKey(assetPath))
                {
                    collectDict[assetPath] = Create(false, assetPath);
                }
            }
            return collectDict;
        }
        private static readonly HashSet<string> IgnoreFileExtensions = new HashSet<string>() { "", ".so", ".dll", ".cs", ".js", ".boo", ".meta", ".cginc", ".hlsl" };
        private static bool Filter(string assetPath)
        {
            if (!assetPath.StartsWith("Assets/") && !assetPath.StartsWith("Packages/"))
            {
                Debug.LogWarning($"Invalid asset path : {assetPath}");
                return false;
            }

            // 忽略文件夹
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return false;
            }
            
            // 忽略Gizmos
            if (assetPath.Contains("/Gizmos/") || assetPath.Contains("/Editor"))
            {
                return false;
            }
            
            // 忽略Gizmos
            if (assetPath.Contains("/Editor Resources/"))
            {
                Debug.LogWarning($"Invalid asset path : {assetPath} in /Editor Resources/");
                return false;
            }

            return !IgnoreFileExtensions.Contains(Path.GetExtension(assetPath));
        }
    }
}