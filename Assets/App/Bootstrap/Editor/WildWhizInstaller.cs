using System.Collections.Generic;
using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Games.WildWhiz;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Lbs.MiniGames.Bootstrap.Editor
{
    public static class WildWhizInstaller
    {
        private const string CatalogFolder = "Assets/App/Catalog/Data";
        private const string WildWhizCategoryPath = CatalogFolder + "/WildWhizCategory.asset";
        private const string WildWhizGamePath = CatalogFolder + "/WildWhizGame.asset";
        private const string CatalogPath = CatalogFolder + "/MiniGameCatalog.asset";
        private const string WildWhizScenePath = "Assets/App/Games/WildWhiz/WildWhiz.unity";
        private const string WildWhizThumbnailPath = "Assets/App/Games/WildWhiz/Art/Thumbnails/WildWhizNatureAdventure.png";

        [MenuItem("Tools/LBS Mini Games/Install Wild Whiz")]
        public static void Install()
        {
            EnsureFolders();

            GameCategory wildWhizCategory = CreateOrLoad<GameCategory>(WildWhizCategoryPath);
            wildWhizCategory.Configure("wild-whiz", "Wild Whiz", "Sort animals by habitat, diet and movement in three gated levels.");

            GameDefinition wildWhizGame = CreateOrLoad<GameDefinition>(WildWhizGamePath);
            wildWhizGame.Configure(
                "wild-whiz.logic",
                "Wild Whiz",
                wildWhizCategory,
                "WildWhiz",
                "Sort animals by habitat, diet and movement in three gated levels.");
            Sprite thumb = AssetDatabase.LoadAssetAtPath<Sprite>(WildWhizThumbnailPath);
            if (thumb != null)
            {
                wildWhizGame.SetThumbnail(thumb);
            }

            GameCatalog catalog = CreateOrLoad<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                throw new System.InvalidOperationException("MiniGameCatalog could not be created or loaded.");
            }

            // Additive — do not overwrite Classification
            catalog.EnsureCategory(wildWhizCategory);
            catalog.EnsureGame(wildWhizGame);

            EditorUtility.SetDirty(wildWhizCategory);
            EditorUtility.SetDirty(wildWhizGame);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();

            EnsureBuildScene(WildWhizScenePath, true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void ConfigureImportedImages()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            string animalRoot = "Assets/App/Games/WildWhiz/Art/Animals/";
            string zoneRoot = "Assets/App/Games/WildWhiz/Art/Zones/";
            string[] animals = { "Bear", "Dolphin", "Eagle", "Elephant", "Fox", "Giraffe", "Lion", "Octopus", "Parrot", "Rabbit", "Shark", "Wolf" };
            string[] zones = { "Forest", "Ocean", "Herbivore", "Carnivore", "Fly", "Swim", "Walk" };
            foreach (string name in animals) ConfigureSprite(animalRoot + "WildWhiz_" + name + ".png");
            foreach (string name in zones) ConfigureSprite(zoneRoot + "WildWhiz_" + name + "_Zone.png");
            ConfigureSprite("Assets/App/Games/WildWhiz/Art/Thumbnails/WildWhizNatureAdventure.png");
            TextureImporter thumbnailImporter = AssetImporter.GetAtPath("Assets/App/Games/WildWhiz/Art/Thumbnails/WildWhizNatureAdventure.png") as TextureImporter;
            thumbnailImporter.alphaIsTransparency = false;
            thumbnailImporter.SaveAndReimport();
            ConfigureSprite("Assets/App/Shared/Art/Icon_Speaker.png");
            WildWhizLevelSet set = AssetDatabase.LoadAssetAtPath<WildWhizLevelSet>("Assets/App/Games/WildWhiz/Data/WildWhizLevelSet.asset");
            UnityEditor.SerializedObject so = new(set);
            UnityEditor.SerializedProperty levels = so.FindProperty("levels");
            for (int level = 0; level < levels.arraySize; level++)
            {
                UnityEditor.SerializedProperty items = levels.GetArrayElementAtIndex(level).FindPropertyRelative("items");
                for (int i = 0; i < items.arraySize; i++)
                {
                    string id = items.GetArrayElementAtIndex(i).FindPropertyRelative("tokenId").stringValue;
                    string name = char.ToUpperInvariant(id[0]) + id.Substring(1);
                    items.GetArrayElementAtIndex(i).FindPropertyRelative("sprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(animalRoot + "WildWhiz_" + name + ".png");
                }
                UnityEditor.SerializedProperty targets = levels.GetArrayElementAtIndex(level).FindPropertyRelative("targets");
                UnityEditor.SerializedProperty sprites = levels.GetArrayElementAtIndex(level).FindPropertyRelative("targetSprites");
                sprites.arraySize = targets.arraySize;
                for (int i = 0; i < targets.arraySize; i++)
                {
                    string id = targets.GetArrayElementAtIndex(i).stringValue;
                    string name = char.ToUpperInvariant(id[0]) + id.Substring(1);
                    sprites.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(zoneRoot + "WildWhiz_" + name + "_Zone.png");
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            Scene scene = EditorSceneManager.OpenScene(WildWhizScenePath, OpenSceneMode.Single);
            foreach (WildWhizGame game in UnityEngine.Object.FindObjectsByType<WildWhizGame>(FindObjectsSortMode.None))
            {
                SerializedObject gameSo = new(game);
                gameSo.FindProperty("speakerSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Shared/Art/Icon_Speaker.png");
                gameSo.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(game);
            }
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.Refresh();
        }

        private static void ConfigureSprite(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new System.InvalidOperationException("Missing imported image: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.isReadable = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        private static void EnsureBuildScene(string path, bool enabled)
        {
            List<EditorBuildSettingsScene> scenes = new(EditorBuildSettings.scenes);
            foreach (EditorBuildSettingsScene s in scenes)
            {
                if (string.Equals(s.path, path, System.StringComparison.Ordinal))
                {
                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(path, enabled));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static T CreateOrLoad<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolders()
        {
            string[] folders =
            {
                "Assets/App",
                "Assets/App/Bootstrap",
                "Assets/App/Catalog",
                CatalogFolder,
                "Assets/App/Games",
                "Assets/App/Games/WildWhiz",
                "Assets/App/Games/WildWhiz/Art",
                "Assets/App/Games/WildWhiz/Audio",
                "Assets/App/Games/WildWhiz/Data",
            };

            foreach (string folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    string parent = System.IO.Path.GetDirectoryName(folder)?.Replace('\\', '/');
                    string name = System.IO.Path.GetFileName(folder);
                    AssetDatabase.CreateFolder(parent, name);
                }
            }
        }
    }
}
