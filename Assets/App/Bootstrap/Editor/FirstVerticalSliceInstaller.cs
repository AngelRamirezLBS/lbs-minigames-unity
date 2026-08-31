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
        private const string LogicaCategoryPath = CatalogFolder + "/LogicaCategory.asset";
        private const string MatematicasCategoryPath = CatalogFolder + "/MatematicasCategory.asset";
        private const string CienciaCategoryPath = CatalogFolder + "/CienciaCategory.asset";
        private const string DefinitionPath = CatalogFolder + "/ClassificationGame.asset";
        private const string CatalogPath = CatalogFolder + "/MiniGameCatalog.asset";
        private const string VolteRegularPath = "Assets/App/Theme/Fonts/Volte-Regular.otf";
        private const string BrandLogoPath = "Assets/App/Theme/Brand/LbsPlusLogo.png";
        private const string WolfieAvatarPath = "Assets/App/Theme/Brand/WolfieAvatar.png";
        private const string ClassificationThumbnailPath = "Assets/App/Games/Classification/ClassificationThumbnail.png";

        [MenuItem("Tools/LBS Mini Games/Install First Vertical Slice")]
        public static void Install()
        {
            EnsureFolders();
            ConfigureOrientation();

            // --- Catalog: 3 subject categories + placeholders (data assets only) ---
            GameCategory logica = CreateOrLoad<GameCategory>(LogicaCategoryPath);
            logica.Configure("logica", "Lógica", "Ejercita el pensamiento deductivo, la memoria y la resolución de problemas.");

            GameCategory matematicas = CreateOrLoad<GameCategory>(MatematicasCategoryPath);
            matematicas.Configure("matematicas", "Matemáticas", "Refuerza operaciones y conceptos numéricos de primaria.");

            GameCategory ciencia = CreateOrLoad<GameCategory>(CienciaCategoryPath);
            ciencia.Configure("ciencia", "Ciencia", "Descubre el cuerpo humano, la naturaleza y el planeta que habitamos.");

            // The real game is relinked to the Ciencia category.
            GameDefinition classification = CreateOrLoad<GameDefinition>(DefinitionPath);
            classification.Configure(
                "classification.animals",
                "Clasificación de animales",
                ciencia,
                "Classification",
                "Clasifica los animales según el grupo al que pertenecen.");
            classification.SetThumbnail(AssetDatabase.LoadAssetAtPath<Sprite>(ClassificationThumbnailPath));

            List<GameDefinition> definitions = new()
            {
                classification,
                CreatePlaceholderDefinition("logica.series", "Series y Patrones", logica, "Completa las secuencias lógicas y descubre el patrón oculto."),
                CreatePlaceholderDefinition("logica.memoria", "Memoria Lógica", logica, "Encuentra las parejas y entrena tu memoria."),
                CreatePlaceholderDefinition("logica.laberintos", "Laberintos", logica, "Guía al personaje a través de laberintos y sortea los obstáculos."),
                CreatePlaceholderDefinition("logica.sudoku", "Sudoku Lógico", logica, "Completa la cuadrícula sin repetir números."),
                CreatePlaceholderDefinition("matematicas.sumas", "Sumas y Restas", matematicas, "Resuelve sumas y restas de manera divertida."),
                CreatePlaceholderDefinition("matematicas.multiplicaciones", "Multiplicaciones", matematicas, "Practica las tablas de multiplicar a tu ritmo."),
                CreatePlaceholderDefinition("matematicas.fracciones", "Fracciones", matematicas, "Compara y combina fracciones sencillas."),
                CreatePlaceholderDefinition("matematicas.geometria", "Geometría", matematicas, "Reconoce figuras, ángulos y perímetros."),
                CreatePlaceholderDefinition("ciencia.cuerpo", "El Cuerpo Humano", ciencia, "Aprende las partes y órganos del cuerpo humano."),
                CreatePlaceholderDefinition("ciencia.planetas", "Planetas", ciencia, "Viaja por el sistema solar y conoce los planetas."),
                CreatePlaceholderDefinition("ciencia.animales", "Reino Animal", ciencia, "Descubre hábitats, alimentación y curiosidades.")
            };

            GameCatalog catalog = CreateOrLoad<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                throw new System.InvalidOperationException("MiniGameCatalog could not be created or loaded.");
            }

            catalog.Configure(new List<GameCategory> { logica, matematicas, ciencia }, definitions);

            EditorUtility.SetDirty(logica);
            EditorUtility.SetDirty(matematicas);
            EditorUtility.SetDirty(ciencia);
            foreach (GameDefinition definition in definitions)
            {
                EditorUtility.SetDirty(definition);
            }
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();

            CreateBootstrapScene(catalog);
            CreateLobbyScene(catalog);
            CreateClassificationScene();
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/App/Bootstrap/Bootstrap.unity", true),
                new EditorBuildSettingsScene("Assets/App/Lobby/Lobby.unity", true),
                new EditorBuildSettingsScene("Assets/App/Games/Classification/Classification.unity", true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static GameDefinition CreatePlaceholderDefinition(
            string id,
            string name,
            GameCategory category,
            string description)
        {
            // Preview placeholders deliberately ship with no scene so IsValid() is
            // false and they never launch. File names derive from the id.
            string path = $"{CatalogFolder}/Placeholder.{id}.asset";
            GameDefinition definition = CreateOrLoad<GameDefinition>(path);
            definition.Configure(id, name, category, string.Empty, description);
            return definition;
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
            controller.SetMascotSprite(LoadSprite(WolfieAvatarPath));
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

        private static Sprite LoadSprite(string path)
        {
            // A newly-added PNG imports as a regular Texture2D by default, which would
            // make LoadAssetAtPath<Sprite> return null. Force the importer to a single
            // Sprite (idempotent) — textureType + spriteMode — so the avatar resolves
            // once the project is imported.
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                }

                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                }

                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                }

                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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
