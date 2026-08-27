using System.Collections.Generic;
using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Games.Classification;
using Lbs.MiniGames.Lobby;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lbs.MiniGames.Bootstrap.Editor
{
    public static class FirstVerticalSliceInstaller
    {
        private const string CatalogFolder = "Assets/App/Catalog/Data";
        private const string CategoryPath = CatalogFolder + "/AnimalsCategory.asset";
        private const string DefinitionPath = CatalogFolder + "/ClassificationGame.asset";
        private const string CatalogPath = CatalogFolder + "/MiniGameCatalog.asset";
        private const string VolteRegularPath = "Assets/App/Theme/Fonts/Volte-Regular.otf";
        private const string BrandLogoPath = "Assets/App/Theme/Brand/LbsPlusLogo.png";
        private const string WolfieHubPath = "Assets/App/Theme/Brand/WolfieHub.png";
        private const string ClassificationThumbnailPath = "Assets/App/Games/Classification/ClassificationThumbnail.png";

        [MenuItem("Tools/LBS Mini Games/Install First Vertical Slice")]
        public static void Install()
        {
            EnsureFolders();
            ConfigureOrientation();

            GameCategory animals = CreateOrLoad<GameCategory>(CategoryPath);
            animals.Configure("animals", "Animals", "Learn how animals belong to different groups.");

            GameDefinition classification = CreateOrLoad<GameDefinition>(DefinitionPath);
            classification.Configure(
                "classification.animals",
                "Animal Classification",
                animals,
                "Classification",
                "Drag the dolphin to the correct group.");
            classification.SetThumbnail(AssetDatabase.LoadAssetAtPath<Sprite>(ClassificationThumbnailPath));

            GameCatalog catalog = CreateOrLoad<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                throw new System.InvalidOperationException("MiniGameCatalog could not be created or loaded.");
            }

            // Additive — preserve existing wild-whiz entries
            catalog.EnsureCategory(animals);
            catalog.EnsureGame(classification);

            EditorUtility.SetDirty(animals);
            EditorUtility.SetDirty(classification);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();

            CreateBootstrapScene(catalog);
            CreateLobbyScene(catalog);
            CreateClassificationScene();
            EnsureBuildScene("Assets/App/Bootstrap/Bootstrap.unity", true);
            EnsureBuildScene("Assets/App/Lobby/Lobby.unity", true);
            EnsureBuildScene("Assets/App/Games/Classification/Classification.unity", true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void ConfigureOrientation()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
        }

        private static void CreateBootstrapScene(GameCatalog catalog)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new("ApplicationBootstrap");
            ApplicationBootstrap bootstrap = root.AddComponent<ApplicationBootstrap>();
            bootstrap.SetCatalog(catalog);
            SerializedObject serialized = new(bootstrap);
            serialized.FindProperty("lobbySceneName").stringValue = "Lobby";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(bootstrap);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, "Assets/App/Bootstrap/Bootstrap.unity");
        }

        private static void CreateLobbyScene(GameCatalog catalog)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Canvas canvas = CreateCanvas();
            GameObject controllerObject = new("LobbyController");
            controllerObject.transform.SetParent(canvas.transform, false);
            LobbyController controller = controllerObject.AddComponent<LobbyController>();
            controller.SetCatalog(catalog);
            controller.SetInterfaceFont(AssetDatabase.LoadAssetAtPath<Font>(VolteRegularPath));
            controller.SetBrandLogo(AssetDatabase.LoadAssetAtPath<Sprite>(BrandLogoPath));
            controller.SetMascotSprite(AssetDatabase.LoadAssetAtPath<Sprite>(WolfieHubPath));
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, "Assets/App/Lobby/Lobby.unity");
        }

        private static void CreateClassificationScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Canvas canvas = CreateCanvas();
            GameObject gameObject = new("ClassificationGame");
            gameObject.transform.SetParent(canvas.transform, false);
            gameObject.AddComponent<ClassificationGame>();
            EditorSceneManager.SaveScene(scene, "Assets/App/Games/Classification/Classification.unity");
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            return canvas;
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

        private static void EnsureFolders()
        {
            string[] folders =
            {
                "Assets/App",
                "Assets/App/Bootstrap",
                "Assets/App/Lobby",
                "Assets/App/Catalog",
                CatalogFolder,
                "Assets/App/Navigation",
                "Assets/App/Shared",
                "Assets/App/Games",
                "Assets/App/Games/Common",
                "Assets/App/Games/Classification",
                "Assets/App/Games/WildWhiz",
                "Assets/App/Games/WildWhiz/Art",
                "Assets/App/Games/WildWhiz/Audio",
                "Assets/App/Games/WildWhiz/Data",
                "Assets/App/Games/Memory",
                "Assets/App/Games/Matching"
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
