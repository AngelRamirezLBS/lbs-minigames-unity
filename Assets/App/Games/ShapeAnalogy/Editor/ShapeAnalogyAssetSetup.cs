#if UNITY_EDITOR
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Lbs.MiniGames.Games.ShapeAnalogy.Editor
{
    public static class ShapeAnalogyAssetSetup
    {
        private const string Root = "Assets/App/Games/ShapeAnalogy";
        [MenuItem("LBS/Shape Analogy/Setup Assets")]
        public static void Run()
        {
            foreach (string file in Directory.GetFiles(Root, "*.png", SearchOption.AllDirectories))
            {
                string path = file.Replace('\\', '/');
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            foreach (string file in Directory.GetFiles(Root, "*.svg", SearchOption.AllDirectories))
                AssetDatabase.ImportAsset(file.Replace('\\', '/'), ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            MethodInfo rebuild = typeof(Lbs.MiniGames.Bootstrap.Editor.FirstVerticalSliceInstaller).GetMethod("CreateShapeAnalogyScene", BindingFlags.Static | BindingFlags.NonPublic);
            rebuild?.Invoke(null, null);
            Debug.Log("SHAPE_ANALOGY_ASSET_SETUP_SUMMARY pngs=" + Directory.GetFiles(Root, "*.png", SearchOption.AllDirectories).Length + " svgs=" + Directory.GetFiles(Root, "*.svg", SearchOption.AllDirectories).Length);
        }
    }
}
#endif
