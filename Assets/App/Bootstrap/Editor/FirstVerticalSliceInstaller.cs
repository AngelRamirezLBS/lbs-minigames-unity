using System.Collections.Generic;
using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Games.Classification;
using Lbs.MiniGames.Games.ShapeAnalogy;
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
        private const string ShapeCategoryPath = CatalogFolder + "/ShapeCategory.asset";
        private const string ShapeDefinitionPath = CatalogFolder + "/ShapeAnalogyGame.asset";
        private const string ShapeScenePath = "Assets/App/Games/ShapeAnalogy/ShapeAnalogy.unity";

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

            catalog.Configure(new List<GameCategory> { animals }, new List<GameDefinition> { classification });
            GameCategory shapes = CreateOrLoad<GameCategory>(ShapeCategoryPath);
            shapes.Configure("shape-analogy", "Shape Analogy", "Find the shape and fill relationship.");
            GameDefinition shapeGame = CreateOrLoad<GameDefinition>(ShapeDefinitionPath);
            shapeGame.Configure("shape.analogy", "Shape Analogy", shapes, "ShapeAnalogy", "Look at the relationship and drag the correct card into the empty space.");
            catalog.Add(shapes, shapeGame);

            EditorUtility.SetDirty(animals);
            EditorUtility.SetDirty(classification);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();

            CreateBootstrapScene(catalog);
            CreateLobbyScene(catalog);
            CreateClassificationScene();
            CreateShapeAnalogyScene();
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/App/Bootstrap/Bootstrap.unity", true),
                new EditorBuildSettingsScene("Assets/App/Lobby/Lobby.unity", true),
                new EditorBuildSettingsScene("Assets/App/Games/Classification/Classification.unity", true)
                ,new EditorBuildSettingsScene(ShapeScenePath, true)
            };
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

        private static void CreateShapeAnalogyScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Canvas canvas = CreateCanvas();
            GameObject gameObject = new("ShapeAnalogyGame");
            gameObject.transform.SetParent(canvas.transform, false);
            ShapeAnalogyGame game = gameObject.AddComponent<ShapeAnalogyGame>();
            SerializedObject serialized = new(game);
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(VolteRegularPath);
            serialized.FindProperty("instruction").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/App/Games/ShapeAnalogy/Sounds/Instruction.mp3");
            serialized.FindProperty("tryAgain").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/App/Games/ShapeAnalogy/Sounds/TryAgain.mp3");
            serialized.FindProperty("starEmpty").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/Star_UnFilled.png");
            serialized.FindProperty("starFull").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/Star_FullFilled.png");
            serialized.FindProperty("heartEmpty").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/Heart_UnFilled.png");
            serialized.FindProperty("heartFull").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/Heart_FullFilled.png");
            serialized.FindProperty("missing").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/Missingitem.png");
            serialized.FindProperty("finalStar").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/FinalStar.png");
            serialized.FindProperty("exitIcon").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/ExitLevelToHub.png");
            serialized.FindProperty("hongNeutral").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/Hong_Neutral.png");
            serialized.FindProperty("hong1").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/Hong_Listening1.png");
            serialized.FindProperty("hong2").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/Hong_Listening2.png");
            serialized.FindProperty("hong3").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/Hong_Listening3.png");
            serialized.FindProperty("celebration4Star").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/Celebration/4Star.png");
            serialized.FindProperty("celebration5Star").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/Celebration/5star.png");
            serialized.FindProperty("circleConfetti").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/Celebration/CircleConfetti.png");
            serialized.FindProperty("rectangularConfetti").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/Celebration/RectangularConfetti.png");
            serialized.FindProperty("serpentina").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/Celebration/Serpentina.png");
            serialized.FindProperty("serpentina2").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/Celebration/Serpentina2.png");
            serialized.FindProperty("serpentina3").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/Celebration/Serpentina3.png");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene, ShapeScenePath);
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
                "Assets/App/Games/ShapeAnalogy",
                "Assets/App/Games/ShapeAnalogy/Sounds",
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
