using System.Collections.Generic;
using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Games.Classification;
using Lbs.MiniGames.Games.ShapeAnalogy;
using Lbs.MiniGames.Games.ClothesSelection;
using Lbs.MiniGames.Games.ObjectSelection;
using Lbs.MiniGames.Games.MakeAnEmojiDrag;
using Lbs.MiniGames.Games.AnimalDrag;
using Lbs.MiniGames.Games.TrianglesCount;
using Lbs.MiniGames.Games.CubePlatform;
using Lbs.MiniGames.Games.CandiesLogic;
using Lbs.MiniGames.Games.KitchenMathLogic;
using Lbs.MiniGames.Games.FunnyFaceDrag;
using Lbs.MiniGames.Games.SquaresSuccession;
using Lbs.MiniGames.Games.ChemistrySelection;
using Lbs.MiniGames.Games.TrianglesShapeLogic;
using Lbs.MiniGames.Lobby;
using Lbs.MiniGames.Shared.Audio;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lbs.MiniGames.Bootstrap.Editor
{
    // Fusion installer for the new Hub design (feature/new-hub-design) that adopts the
    // shared launch/difficulty layer and the three polished real games from
    // integration/games (Shape Analogy -> Clothes Selection -> Object Selection).
    //
    // It keeps the new Hub's visual identity (Nunito typography, translucent header,
    // decorative background layer, Wolfie avatar) and reuses the shared catalog with
    // difficulty contracts. The three real games are wired into the "Lógica" category
    // alongside the existing mock placeholders (placeholders keep no scene, so
    // IsValid() == false and they render as "Próximamente" cards that never launch).
    public static class FirstVerticalSliceInstaller
    {
        private const string CatalogFolder = "Assets/App/Catalog/Data";
        private const string LogicaCategoryPath = CatalogFolder + "/LogicaCategory.asset";
        private const string MatematicasCategoryPath = CatalogFolder + "/MatematicasCategory.asset";
        private const string CienciaCategoryPath = CatalogFolder + "/CienciaCategory.asset";
        private const string DefinitionPath = CatalogFolder + "/ClassificationGame.asset";
        private const string CatalogPath = CatalogFolder + "/MiniGameCatalog.asset";
        private const string InterfaceFontPath = "Assets/App/Theme/Fonts/Nunito-Black.ttf";
        private const string CardTitleFontPath = "Assets/App/Theme/Fonts/Nunito-Medium.ttf";
        private const string BrandLogoPath = "Assets/App/Theme/Brand/LbsPlusLogo.png";
        private const string WolfieAvatarPath = "Assets/App/Theme/Brand/WolfieAvatar.png";
        private const string BackgroundFolder = "Assets/App/Theme/Background";
        private static readonly string[] BackgroundDecorationPaths =
        {
            BackgroundFolder + "/bg_blob_filled.png",
            BackgroundFolder + "/bg_blob_outline.png",
            BackgroundFolder + "/bg_spiral.png",
            BackgroundFolder + "/bg_hex_outline.png",
            BackgroundFolder + "/bg_dots.png",
            BackgroundFolder + "/bg_ribbon.png",
            BackgroundFolder + "/bg_cloud.png",
            BackgroundFolder + "/bg_blobs_small.png"
        };
        private const string ThumbnailsFolder = "Assets/App/Theme/Thumbnails";
        private const string SeriesThumbnailPath = ThumbnailsFolder + "/series_patrones.png";
        private const string MemoriaThumbnailPath = ThumbnailsFolder + "/memoria_logica.png";
        private const string LaberintosThumbnailPath = ThumbnailsFolder + "/laberintos.png";
        private const string MosaicoThumbnailPath = ThumbnailsFolder + "/mosaico_colores.png";
        private const string ClassificationThumbnailPath = "Assets/App/Games/Classification/ClassificationThumbnail.png";

        // Real games (from integration/games), wired into the Lógica category.
        private const string ShapeDefinitionPath = CatalogFolder + "/ShapeAnalogyGame.asset";
        private const string ShapeScenePath = "Assets/App/Games/ShapeAnalogy/ShapeAnalogy.unity";
        private const string ClothesDefinitionPath = CatalogFolder + "/ClothesSelectionGame.asset";
        private const string ClothesScenePath = "Assets/App/Games/ClothesSelection/ClothesSelection.unity";
        private const string ObjectSelectionDefinitionPath = CatalogFolder + "/ObjectSelectionGame.asset";
        private const string ObjectSelectionScenePath = "Assets/App/Games/ObjectSelection/ObjectSelection.unity";
        // Real math game (from integration/games), wired into the Matemáticas category.
        private const string NumberPullDefinitionPath = CatalogFolder + "/NumberPullGame.asset";
        private const string NumberPullScenePath = "Assets/App/Games/NumberPull/NumberPull.unity";
        // New logic-sequence games (from integration/games), wired into the Lógica category.
        private const string MakeAnEmojiDragDefinitionPath = CatalogFolder + "/MakeAnEmojiDragGame.asset";
        private const string MakeAnEmojiDragScenePath = "Assets/App/Games/MakeAnEmojiDrag/MakeAnEmojiDrag.unity";
        private const string AnimalDragDefinitionPath = CatalogFolder + "/AnimalDragGame.asset";
        private const string AnimalDragScenePath = "Assets/App/Games/AnimalDrag/AnimalDrag.unity";
        private const string TrianglesCountDefinitionPath = CatalogFolder + "/TrianglesCountGame.asset";
        private const string TrianglesCountScenePath = "Assets/App/Games/TrianglesCount/TrianglesCount.unity";
        private const string CubePlatformDefinitionPath = CatalogFolder + "/CubePlatformGame.asset";
        private const string CubePlatformScenePath = "Assets/App/Games/CubePlatform/CubePlatform.unity";
        private const string CandiesLogicDefinitionPath = CatalogFolder + "/CandiesLogicGame.asset";
        private const string CandiesLogicScenePath = "Assets/App/Games/CandiesLogic/CandiesLogic.unity";
        private const string SquaresSuccessionDefinitionPath = CatalogFolder + "/SquaresSuccessionGame.asset";
        private const string SquaresSuccessionScenePath = "Assets/App/Games/SquaresSuccession/SquaresSuccession.unity";
        private const string KitchenMathLogicDefinitionPath = CatalogFolder + "/KitchenMathLogicGame.asset";
        private const string KitchenMathLogicScenePath = "Assets/App/Games/KitchenMathLogic/KitchenMathLogic.unity";
        private const string FunnyFaceDragDefinitionPath = CatalogFolder + "/FunnyFaceDragGame.asset";
        private const string FunnyFaceDragScenePath = "Assets/App/Games/FunnyFaceDrag/FunnyFaceDrag.unity";
        private const string ChemistrySelectionDefinitionPath = CatalogFolder + "/ChemistrySelectionGame.asset";
        private const string ChemistrySelectionScenePath = "Assets/App/Games/ChemistrySelection/ChemistrySelection.unity";
        private const string TrianglesShapeLogicDefinitionPath = CatalogFolder + "/TrianglesShapeLogicGame.asset";
        private const string TrianglesShapeLogicScenePath = "Assets/App/Games/TrianglesShapeLogic/TrianglesShapeLogic.unity";
        private const string CircleMathDefinitionPath = CatalogFolder + "/CircleMathGame.asset";
        private const string CircleMathScenePath = "Assets/App/Games/CircleMath/CircleMath.unity";
        private const string DifficultyEasyPath = CatalogFolder + "/DifficultyEasy.asset";
        private const string DifficultyMediumPath = CatalogFolder + "/DifficultyMedium.asset";
        private const string DifficultyHardPath = CatalogFolder + "/DifficultyHard.asset";
        private const string AudioConfigPath = "Assets/App/Shared/Audio/AppAudioConfig.asset";
        private const string FinalCelebrationConfigurationPath = "Assets/App/Shared/Results/DefaultFinalCelebrationConfiguration.asset";

        [MenuItem("Tools/LBS Mini Games/Install First Vertical Slice")]
        public static void Install()
        {
            EnsureFolders();
            ConfigureClothesSprites();
            ConfigureObjectSelectionSprites();
            ConfigureThumbnails();
            ConfigureOrientation();

            // --- Hub categories (new design) ---
            GameCategory logica = CreateOrLoad<GameCategory>(LogicaCategoryPath);
            logica.Configure("logica", "Lógica, matemáticas, lenguaje", "Ejercita el pensamiento deductivo, la memoria y la resolución de problemas.");

            GameCategory matematicas = CreateOrLoad<GameCategory>(MatematicasCategoryPath);
            matematicas.Configure("matematicas", "Juega y Aprende", "Refuerza operaciones y conceptos numéricos de primaria.");

            GameCategory ciencia = CreateOrLoad<GameCategory>(CienciaCategoryPath);
            ciencia.Configure("ciencia", "Ciencia", "Descubre el cuerpo humano, la naturaleza y el planeta que habitamos.");

            // --- Difficulty definitions (shared contract) ---
            DifficultyDefinition easy = CreateOrLoad<DifficultyDefinition>(DifficultyEasyPath);
            easy.Configure("easy", "Easy", 0, "Gentle introduction with fewer distractors.");
            DifficultyDefinition medium = CreateOrLoad<DifficultyDefinition>(DifficultyMediumPath);
            medium.Configure("medium", "Medium", 1, "Balanced challenge.");
            DifficultyDefinition hard = CreateOrLoad<DifficultyDefinition>(DifficultyHardPath);
            hard.Configure("hard", "Hard", 2, "Extra cards and timed pressure.");
            var allDifficulties = new List<DifficultyDefinition> { easy, medium, hard };

            // --- Real game: Classification (relinked to the Ciencia category) ---
            GameDefinition classification = CreateOrLoad<GameDefinition>(DefinitionPath);
            classification.Configure(
                "classification.animals",
                "Clasificación de animales",
                ciencia,
                "Classification",
                "Clasifica los animales según el grupo al que pertenecen.");
            classification.SetThumbnail(AssetDatabase.LoadAssetAtPath<Sprite>(ClassificationThumbnailPath));
            classification.ConfigureDifficulties(allDifficulties, medium);
            // Classification was a test game; keep its assets but hide it from the Hub.
            classification.SetVisibleInHub(false);

            // --- Real games: Shape Analogy -> Clothes Selection -> Object Selection (Lógica) ---
            GameDefinition shapeGame = CreateOrLoad<GameDefinition>(ShapeDefinitionPath);
            shapeGame.Configure("shape.analogy", "Analogía de formas", logica, "ShapeAnalogy", "Mira la relación y arrastra la carta correcta al espacio vacío.");
            shapeGame.SetThumbnail(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/FinalStar.png"));
            shapeGame.ConfigureDifficulties(allDifficulties, medium);

            GameDefinition clothesGame = CreateOrLoad<GameDefinition>(ClothesDefinitionPath);
            clothesGame.Configure("clothes.selection", "Selección de ropa", logica, "ClothesSelection", "Elige el elemento que combina con el contexto mostrado.");
            clothesGame.SetThumbnail(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ClothesSelection/Art/FurnitureWObjects.png"));
            clothesGame.ConfigureDifficulties(allDifficulties, medium);

            GameDefinition objectSelectionGame = CreateOrLoad<GameDefinition>(ObjectSelectionDefinitionPath);
            objectSelectionGame.Configure("object.selection", "Selección de objetos", logica, "ObjectSelection", "Encuentra el objeto que no pertenece al grupo.");
            objectSelectionGame.SetThumbnail(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ObjectSelection/Art/tenis.png"));
            objectSelectionGame.ConfigureDifficulties(allDifficulties, medium);

            // --- Real game: Number Pull (Matemáticas, standalone, returns to Hub) ---
            GameDefinition numberPullGame = CreateOrLoad<GameDefinition>(NumberPullDefinitionPath);
            numberPullGame.Configure("math.number-pull", "Number Pull", matematicas, "NumberPull", "Resuelve sumas y restas rápidas en un tira y afloja de dos jugadores.");
            numberPullGame.ConfigureDifficulties(allDifficulties, medium);

            // --- New logic-sequence games (integration/games), wired into the Lógica category ---
            GameDefinition makeAnEmojiDragGame = CreateOrLoad<GameDefinition>(MakeAnEmojiDragDefinitionPath);
            makeAnEmojiDragGame.Configure("make.emoji.drag", "Make An Emoji Drag", logica, "MakeAnEmojiDrag", "Arrastra las tiras para formar una carita feliz.");
            makeAnEmojiDragGame.ConfigureDifficulties(allDifficulties, medium);

            GameDefinition animalDragGame = CreateOrLoad<GameDefinition>(AnimalDragDefinitionPath);
            animalDragGame.Configure("animal.drag", "Animal Drag", logica, "AnimalDrag", "Ayuda a los animales a llegar a sus casas.");
            animalDragGame.ConfigureDifficulties(allDifficulties, medium);

            GameDefinition trianglesCountGame = CreateOrLoad<GameDefinition>(TrianglesCountDefinitionPath);
            trianglesCountGame.Configure("triangles.count", "Triangles Count", logica, "TrianglesCount", "Cuenta los triángulos.");
            trianglesCountGame.ConfigureDifficulties(allDifficulties, medium);

            GameDefinition cubePlatformGame = CreateOrLoad<GameDefinition>(CubePlatformDefinitionPath);
            cubePlatformGame.Configure("cube.platform", "Cube Platform", logica, "CubePlatform", "¿Cuál caja es la más liviana?");
            cubePlatformGame.ConfigureDifficulties(allDifficulties, medium);

            GameDefinition candiesLogicGame = CreateOrLoad<GameDefinition>(CandiesLogicDefinitionPath);
            candiesLogicGame.Configure("candies.logic", "Candies Logic", logica, "CandiesLogic", "¿Qué es el círculo grande?");
            candiesLogicGame.ConfigureDifficulties(allDifficulties, medium);

            GameDefinition squaresSuccessionGame = CreateOrLoad<GameDefinition>(SquaresSuccessionDefinitionPath);
            squaresSuccessionGame.Configure("squares.succession", "Squares Succession", logica, "SquaresSuccession", "¿Qué viene después?");
            squaresSuccessionGame.ConfigureDifficulties(allDifficulties, medium);

            GameDefinition kitchenMathLogicGame = CreateOrLoad<GameDefinition>(KitchenMathLogicDefinitionPath);
            kitchenMathLogicGame.Configure("kitchen.math.logic", "Kitchen Math Logic", logica, "KitchenMathLogic", "¿Cuál es el resultado? Toast + queso + cuchillo =");
            kitchenMathLogicGame.ConfigureDifficulties(allDifficulties, medium);

            GameDefinition funnyFaceDragGame = CreateOrLoad<GameDefinition>(FunnyFaceDragDefinitionPath);
            funnyFaceDragGame.Configure("funnyface.drag", "Funny Face Drag", logica, "FunnyFaceDrag", "Completa la cara graciosa");
            funnyFaceDragGame.ConfigureDifficulties(allDifficulties, medium);

            GameDefinition chemistrySelectionGame = CreateOrLoad<GameDefinition>(ChemistrySelectionDefinitionPath);
            chemistrySelectionGame.Configure("chemistry.selection", "Chemistry Selection", logica, "ChemistrySelection", "Elige la relación química correcta.");
            chemistrySelectionGame.ConfigureDifficulties(allDifficulties, medium);

            GameDefinition trianglesShapeLogicGame = CreateOrLoad<GameDefinition>(TrianglesShapeLogicDefinitionPath);
            trianglesShapeLogicGame.Configure("triangles.shape.logic", "Triangles Shape Logic", logica, "TrianglesShapeLogic", "Completa la tabla. Usa los triángulos de color correctos.");
            trianglesShapeLogicGame.ConfigureDifficulties(allDifficulties, medium);

            GameDefinition circleMathGame = CreateOrLoad<GameDefinition>(CircleMathDefinitionPath);
            circleMathGame.Configure("circle.math", "Circle Math", logica, "CircleMath", "Which circles come next?");
            circleMathGame.SetThumbnail(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/CircleMath/Art/Principal.png"));
            circleMathGame.ConfigureDifficulties(allDifficulties, medium);

            List<GameDefinition> definitions = new()
            {
                classification,
                shapeGame,
                clothesGame,
                objectSelectionGame,
                makeAnEmojiDragGame,
                animalDragGame,
                trianglesCountGame,
                cubePlatformGame,
                candiesLogicGame,
                squaresSuccessionGame,
                kitchenMathLogicGame,
                funnyFaceDragGame,
                chemistrySelectionGame,
                trianglesShapeLogicGame,
                circleMathGame,
                numberPullGame,
                // --- Mock placeholders (no scene -> never launch) ---
                CreatePlaceholderDefinition("logica.series", "Series y Patrones", logica, "Completa las secuencias lógicas y descubre el patrón oculto.", SeriesThumbnailPath),
                CreatePlaceholderDefinition("logica.memoria", "Memoria Lógica", logica, "Encuentra las parejas y entrena tu memoria.", MemoriaThumbnailPath),
                CreatePlaceholderDefinition("logica.laberintos", "Laberintos", logica, "Guía al personaje a través de laberintos y sortea los obstáculos.", LaberintosThumbnailPath),
                CreatePlaceholderDefinition("logica.sudoku", "Mosaico de colores", logica, "Completa el mosaico y descubre los patrones de color.", MosaicoThumbnailPath),
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
            EditorUtility.SetDirty(easy);
            EditorUtility.SetDirty(medium);
            EditorUtility.SetDirty(hard);
            foreach (GameDefinition definition in definitions)
            {
                EditorUtility.SetDirty(definition);
            }
            EditorUtility.SetDirty(catalog);

            // --- Global audio config (persistent shared music, non-destructive) ---
            AppAudioConfig audioConfig = CreateOrLoad<AppAudioConfig>(AudioConfigPath);
            AudioClip bg = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/App/Games/ShapeAnalogy/Sounds/Music/bg_cabinet_menu.mp3");
            if (bg == null) bg = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/ShapeAnalogy/Music/bg_cabinet_menu.mp3");
            if (bg != null) audioConfig.Configure(bg, 0.25f, 0.125f);
            EditorUtility.SetDirty(audioConfig);
            AssetDatabase.SaveAssets();

            // --- Shared final celebration configuration ---
            // CreateOrLoad returns the asset whether it already existed or was just
            // created, so reuse that instance directly. Reloading via LoadAssetAtPath
            // immediately after creation can yield null (the asset is not yet fully
            // imported), which would serialize celebrationConfiguration as fileID: 0
            // and crash the celebration at runtime.
            Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration celebrationConfiguration =
                CreateOrLoad<Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration>(FinalCelebrationConfigurationPath);
            AssetDatabase.SaveAssets();
            if (celebrationConfiguration == null)
            {
                throw new System.InvalidOperationException("FinalCelebrationConfiguration could not be loaded from its persistent asset path.");
            }

            CreateBootstrapScene(catalog);
            CreateLobbyScene(catalog);
            CreateClassificationScene();
            CreateShapeAnalogyScene(celebrationConfiguration);
            CreateClothesSelectionScene(celebrationConfiguration);
            CreateObjectSelectionScene(celebrationConfiguration);
            CreateMakeAnEmojiDragScene(celebrationConfiguration);
            CreateAnimalDragScene(celebrationConfiguration);
            CreateTrianglesCountScene(celebrationConfiguration);
            CreateCubePlatformScene(celebrationConfiguration);
            CreateCandiesLogicScene(celebrationConfiguration);
            CreateSquaresSuccessionScene(celebrationConfiguration);
            CreateKitchenMathLogicScene(celebrationConfiguration);
            CreateFunnyFaceDragScene(celebrationConfiguration);
            CreateChemistrySelectionScene(celebrationConfiguration);
            CreateTrianglesShapeLogicScene(celebrationConfiguration);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/App/Bootstrap/Bootstrap.unity", true),
                new EditorBuildSettingsScene("Assets/App/Lobby/Lobby.unity", true),
                new EditorBuildSettingsScene("Assets/App/Games/Classification/Classification.unity", true),
                new EditorBuildSettingsScene(ShapeScenePath, true),
                new EditorBuildSettingsScene(ClothesScenePath, true),
                new EditorBuildSettingsScene(ObjectSelectionScenePath, true),
                new EditorBuildSettingsScene(NumberPullScenePath, true),
                new EditorBuildSettingsScene(MakeAnEmojiDragScenePath, true),
                new EditorBuildSettingsScene(AnimalDragScenePath, true),
                new EditorBuildSettingsScene(TrianglesCountScenePath, true),
                new EditorBuildSettingsScene(CubePlatformScenePath, true),
                new EditorBuildSettingsScene(CandiesLogicScenePath, true),
                new EditorBuildSettingsScene(SquaresSuccessionScenePath, true),
                new EditorBuildSettingsScene(KitchenMathLogicScenePath, true),
                new EditorBuildSettingsScene(FunnyFaceDragScenePath, true),
                new EditorBuildSettingsScene(ChemistrySelectionScenePath, true),
                new EditorBuildSettingsScene(TrianglesShapeLogicScenePath, true),
                new EditorBuildSettingsScene("Assets/App/Games/Thinking3D/Thinking3D.unity", true),
                new EditorBuildSettingsScene(CircleMathScenePath, true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Lbs.MiniGames.Catalog.Editor.GameCatalogValidation.ValidateCatalogs();
        }

        private static GameDefinition CreatePlaceholderDefinition(
            string id,
            string name,
            GameCategory category,
            string description,
            string thumbnailPath = null)
        {
            // Preview placeholders deliberately ship with no scene so IsValid() is
            // false and they never launch. File names derive from the id.
            string path = $"{CatalogFolder}/Placeholder.{id}.asset";
            GameDefinition definition = CreateOrLoad<GameDefinition>(path);
            definition.Configure(id, name, category, string.Empty, description);
            if (!string.IsNullOrWhiteSpace(thumbnailPath))
            {
                definition.SetThumbnail(AssetDatabase.LoadAssetAtPath<Sprite>(thumbnailPath));
            }

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
            AppAudioConfig audioConfig = AssetDatabase.LoadAssetAtPath<AppAudioConfig>(AudioConfigPath);
            if (audioConfig != null) serialized.FindProperty("audioConfig").objectReferenceValue = audioConfig;
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
            controller.SetInterfaceFont(AssetDatabase.LoadAssetAtPath<Font>(InterfaceFontPath));
            controller.SetCardTitleFont(AssetDatabase.LoadAssetAtPath<Font>(CardTitleFontPath));
            controller.SetBrandLogo(AssetDatabase.LoadAssetAtPath<Sprite>(BrandLogoPath));
            controller.SetMascotSprite(LoadSprite(WolfieAvatarPath));
            controller.SetBackgroundDecorations(LoadBackgroundDecorations());
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

        private static void CreateShapeAnalogyScene(Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration celebrationConfiguration)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Canvas canvas = CreateCanvas();
            GameObject gameObject = new("ShapeAnalogyGame");
            gameObject.transform.SetParent(canvas.transform, false);
            ShapeAnalogyGame game = gameObject.AddComponent<ShapeAnalogyGame>();
            SerializedObject serialized = new(game);
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(InterfaceFontPath);
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
            SerializedProperty celebrationConfigurationProperty = serialized.FindProperty("celebrationConfiguration");
            if (celebrationConfigurationProperty == null)
            {
                throw new System.InvalidOperationException("ShapeAnalogyGame is missing the celebrationConfiguration serialized property.");
            }
            celebrationConfigurationProperty.objectReferenceValue = celebrationConfiguration;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(game);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ShapeScenePath);
        }

        private static void ConfigureClothesSprites()
        {
            string[] names = { "BaseOfAll.png", "FurnitureWObjects.png", "Shoes.png", "Heel.png", "Gloves.png" };
            foreach (string name in names)
            {
                TextureImporter importer = AssetImporter.GetAtPath("Assets/App/Games/ClothesSelection/Art/" + name) as TextureImporter;
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
        }

        private static void ConfigureObjectSelectionSprites()
        {
            string[] names = { "fondo.png", "tenis.png", "sombrero.png", "gorro.png", "gorra.png" };
            foreach (string name in names)
            {
                TextureImporter importer = AssetImporter.GetAtPath("Assets/App/Games/ObjectSelection/Art/" + name) as TextureImporter;
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
        }

        private static void ConfigureThumbnails()
        {
            foreach (string path in new[] { SeriesThumbnailPath, MemoriaThumbnailPath, LaberintosThumbnailPath, MosaicoThumbnailPath })
            {
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
        }

        private static void CreateClothesSelectionScene(Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration celebrationConfiguration)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Canvas canvas = CreateCanvas();
            GameObject gameObject = new("ClothesSelectionGame");
            gameObject.transform.SetParent(canvas.transform, false);
            ClothesSelectionGame game = gameObject.AddComponent<ClothesSelectionGame>();
            SerializedObject serialized = new(game);
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(InterfaceFontPath);
            serialized.FindProperty("instruction").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/App/Games/ClothesSelection/Instruction.mp3");
            serialized.FindProperty("baseArtwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ClothesSelection/Art/BaseOfAll.png");
            serialized.FindProperty("shelfArtwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ClothesSelection/Art/FurnitureWObjects.png");
            serialized.FindProperty("shoesArtwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ClothesSelection/Art/Shoes.png");
            serialized.FindProperty("heelArtwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ClothesSelection/Art/Heel.png");
            serialized.FindProperty("glovesArtwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ClothesSelection/Art/Gloves.png");
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
            SerializedProperty celebrationConfigurationProperty = serialized.FindProperty("celebrationConfiguration");
            if (celebrationConfigurationProperty == null)
            {
                throw new System.InvalidOperationException("ClothesSelectionGame is missing the celebrationConfiguration serialized property.");
            }
            celebrationConfigurationProperty.objectReferenceValue = celebrationConfiguration;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(game);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ClothesScenePath);
        }

        private static void CreateObjectSelectionScene(Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration celebrationConfiguration)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Canvas canvas = CreateCanvas();
            GameObject gameObject = new("ObjectSelectionGame");
            gameObject.transform.SetParent(canvas.transform, false);
            ObjectSelectionGame game = gameObject.AddComponent<ObjectSelectionGame>();
            SerializedObject serialized = new(game);
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(InterfaceFontPath);
            serialized.FindProperty("instruction").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/App/Games/ObjectSelection/Instruction.mp3");
            serialized.FindProperty("backgroundArtwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ObjectSelection/Art/fondo.png");
            serialized.FindProperty("tenisArtwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ObjectSelection/Art/tenis.png");
            serialized.FindProperty("sombreroArtwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ObjectSelection/Art/sombrero.png");
            serialized.FindProperty("gorroArtwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ObjectSelection/Art/gorro.png");
            serialized.FindProperty("gorraArtwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ObjectSelection/Art/gorra.png");
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
            SerializedProperty celebrationConfigurationProperty = serialized.FindProperty("celebrationConfiguration");
            if (celebrationConfigurationProperty == null)
            {
                throw new System.InvalidOperationException("ObjectSelectionGame is missing the celebrationConfiguration serialized property.");
            }
            celebrationConfigurationProperty.objectReferenceValue = celebrationConfiguration;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(game);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ObjectSelectionScenePath);
        }

        private static void CreateMakeAnEmojiDragScene(Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration celebrationConfiguration)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Canvas canvas = CreateCanvas();
            GameObject gameObject = new("MakeAnEmojiDragGame"); gameObject.transform.SetParent(canvas.transform, false);
            MakeAnEmojiDragGame game = gameObject.AddComponent<MakeAnEmojiDragGame>();
            SerializedObject serialized = new(game);
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(InterfaceFontPath);
            serialized.FindProperty("instruction").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/App/Games/MakeAnEmojiDrag/Instruction.mp3");
            serialized.FindProperty("topArtwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/MakeAnEmojiDrag/Art/primera.png");
            serialized.FindProperty("middleArtwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/MakeAnEmojiDrag/Art/segunda.png");
            serialized.FindProperty("bottomArtwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/MakeAnEmojiDrag/Art/tercera.png");
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
            SerializedProperty celebrationConfigurationProperty = serialized.FindProperty("celebrationConfiguration");
            if (celebrationConfigurationProperty == null)
            {
                throw new System.InvalidOperationException("MakeAnEmojiDragGame is missing the celebrationConfiguration serialized property.");
            }
            celebrationConfigurationProperty.objectReferenceValue = celebrationConfiguration;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(game);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MakeAnEmojiDragScenePath);
        }

        private static void CreateAnimalDragScene(Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration celebrationConfiguration)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Canvas canvas = CreateCanvas();
            GameObject gameObject = new("AnimalDragGame"); gameObject.transform.SetParent(canvas.transform, false);
            AnimalDragGame game = gameObject.AddComponent<AnimalDragGame>();
            SerializedObject serialized = new(game);
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(InterfaceFontPath);
            serialized.FindProperty("instruction").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/App/Games/AnimalDrag/Instruction.mp3");
            serialized.FindProperty("casitasBackground").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/AnimalDrag/Art/casitas.png");
            serialized.FindProperty("catArtwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/AnimalDrag/Art/gato.png");
            serialized.FindProperty("pigArtwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/AnimalDrag/Art/cerdo.png");
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
            SerializedProperty celebrationConfigurationProperty = serialized.FindProperty("celebrationConfiguration");
            if (celebrationConfigurationProperty == null)
            {
                throw new System.InvalidOperationException("AnimalDragGame is missing the celebrationConfiguration serialized property.");
            }
            celebrationConfigurationProperty.objectReferenceValue = celebrationConfiguration;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(game);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, AnimalDragScenePath);
        }

        private static void CreateTrianglesCountScene(Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration celebrationConfiguration)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Canvas canvas = CreateCanvas();
            GameObject gameObject = new("TrianglesCountGame"); gameObject.transform.SetParent(canvas.transform, false);
            TrianglesCountGame game = gameObject.AddComponent<TrianglesCountGame>();
            SerializedObject serialized = new(game);
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(InterfaceFontPath);
            serialized.FindProperty("instruction").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/App/Games/TrianglesCount/Instruction.mp3");
            serialized.FindProperty("principalSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/TrianglesCount/Art/Triangle_Principal.png");
            serialized.FindProperty("revealSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/TrianglesCount/Art/Reveal.png");
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
            SerializedProperty celebrationConfigurationProperty = serialized.FindProperty("celebrationConfiguration");
            if (celebrationConfigurationProperty == null)
            {
                throw new System.InvalidOperationException("TrianglesCountGame is missing the celebrationConfiguration serialized property.");
            }
            celebrationConfigurationProperty.objectReferenceValue = celebrationConfiguration;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(game);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, TrianglesCountScenePath);
        }

        private static void CreateCubePlatformScene(Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration celebrationConfiguration)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Canvas canvas = CreateCanvas();
            GameObject gameObject = new("CubePlatformGame"); gameObject.transform.SetParent(canvas.transform, false);
            CubePlatformGame game = gameObject.AddComponent<CubePlatformGame>();
            SerializedObject serialized = new(game);
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(InterfaceFontPath);
            serialized.FindProperty("instruction").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/App/Games/CubePlatform/Instruction.mp3");
            serialized.FindProperty("platformSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/CubePlatform/Art/PlatformWBoxes.png");
            serialized.FindProperty("box1Sprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/CubePlatform/Art/Box1.png");
            serialized.FindProperty("box2Sprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/CubePlatform/Art/Box2.png");
            serialized.FindProperty("box3Sprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/CubePlatform/Art/Box3.png");
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
            SerializedProperty celebrationConfigurationProperty = serialized.FindProperty("celebrationConfiguration");
            if (celebrationConfigurationProperty == null)
            {
                throw new System.InvalidOperationException("CubePlatformGame is missing the celebrationConfiguration serialized property.");
            }
            celebrationConfigurationProperty.objectReferenceValue = celebrationConfiguration;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(game);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, CubePlatformScenePath);
        }

        private static void CreateCandiesLogicScene(Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration celebrationConfiguration)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Canvas canvas = CreateCanvas();
            GameObject gameObject = new("CandiesLogicGame"); gameObject.transform.SetParent(canvas.transform, false);
            CandiesLogicGame game = gameObject.AddComponent<CandiesLogicGame>();
            SerializedObject serialized = new(game);
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(InterfaceFontPath);
            serialized.FindProperty("instruction").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/App/Games/CandiesLogic/Instruction.mp3");
            serialized.FindProperty("principalSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/CandiesLogic/Art/CandiesandSweets.png");
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
            SerializedProperty celebrationConfigurationProperty = serialized.FindProperty("celebrationConfiguration");
            if (celebrationConfigurationProperty == null)
            {
                throw new System.InvalidOperationException("CandiesLogicGame is missing the celebrationConfiguration serialized property.");
            }
            celebrationConfigurationProperty.objectReferenceValue = celebrationConfiguration;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(game);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, CandiesLogicScenePath);
        }

        private static void CreateSquaresSuccessionScene(Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration celebrationConfiguration)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Canvas canvas = CreateCanvas();
            GameObject gameObject = new("SquaresSuccessionGame"); gameObject.transform.SetParent(canvas.transform, false);
            SquaresSuccessionGame game = gameObject.AddComponent<SquaresSuccessionGame>();
            SerializedObject serialized = new(game);
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(InterfaceFontPath);
            serialized.FindProperty("instruction").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/App/Games/SquaresSuccession/Instruction.mp3");
            serialized.FindProperty("principalSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/SquaresSuccession/Art/SecuenceSquares.png");
            serialized.FindProperty("revealSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/SquaresSuccession/Art/Result.png");
            serialized.FindProperty("option1Sprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/SquaresSuccession/Art/Option1.png");
            serialized.FindProperty("option2Sprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/SquaresSuccession/Art/Option2.png");
            serialized.FindProperty("option3Sprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/SquaresSuccession/Art/Option3.png");
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
            SerializedProperty celebrationConfigurationProperty = serialized.FindProperty("celebrationConfiguration");
            if (celebrationConfigurationProperty == null)
            {
                throw new System.InvalidOperationException("SquaresSuccessionGame is missing the celebrationConfiguration serialized property.");
            }
            celebrationConfigurationProperty.objectReferenceValue = celebrationConfiguration;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(game);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, SquaresSuccessionScenePath);
        }

        private static void CreateKitchenMathLogicScene(Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration celebrationConfiguration)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Canvas canvas = CreateCanvas();
            GameObject gameObject = new("KitchenMathLogicGame"); gameObject.transform.SetParent(canvas.transform, false);
            KitchenMathLogicGame game = gameObject.AddComponent<KitchenMathLogicGame>();
            SerializedObject serialized = new(game);
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(InterfaceFontPath);
            serialized.FindProperty("instruction").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/App/Games/KitchenMathLogic/Instruction.mp3");
            serialized.FindProperty("principalSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/KitchenMathLogic/Art/Principal.png");
            serialized.FindProperty("option1Sprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/KitchenMathLogic/Art/Option1.png");
            serialized.FindProperty("option2Sprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/KitchenMathLogic/Art/Option2.png");
            serialized.FindProperty("option3Sprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/KitchenMathLogic/Art/Option3.png");
            serialized.FindProperty("option4Sprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/KitchenMathLogic/Art/Option4.png");
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
            SerializedProperty celebrationConfigurationProperty = serialized.FindProperty("celebrationConfiguration");
            if (celebrationConfigurationProperty == null)
            {
                throw new System.InvalidOperationException("KitchenMathLogicGame is missing the celebrationConfiguration serialized property.");
            }
            celebrationConfigurationProperty.objectReferenceValue = celebrationConfiguration;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(game);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, KitchenMathLogicScenePath);
        }

        private static void CreateFunnyFaceDragScene(Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration celebrationConfiguration)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Canvas canvas = CreateCanvas();
            GameObject gameObject = new("FunnyFaceDragGame"); gameObject.transform.SetParent(canvas.transform, false);
            FunnyFaceDragGame game = gameObject.AddComponent<FunnyFaceDragGame>();
            SerializedObject serialized = new(game);
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(InterfaceFontPath);
            serialized.FindProperty("instruction").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/App/Games/FunnyFaceDrag/Instruction.mp3");
            serialized.FindProperty("backgroundSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/FunnyFaceDrag/Art/Background.png");
            serialized.FindProperty("principal1Sprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/FunnyFaceDrag/Art/Principal1.png");
            serialized.FindProperty("principal2Sprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/FunnyFaceDrag/Art/Principal2.png");
            serialized.FindProperty("drag1Sprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/FunnyFaceDrag/Art/Drag1.png");
            serialized.FindProperty("drag2Sprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/FunnyFaceDrag/Art/Drag2.png");
            serialized.FindProperty("drag3Sprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/FunnyFaceDrag/Art/Drag3.png");
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
            SerializedProperty celebrationConfigurationProperty = serialized.FindProperty("celebrationConfiguration");
            if (celebrationConfigurationProperty == null)
            {
                throw new System.InvalidOperationException("FunnyFaceDragGame is missing the celebrationConfiguration serialized property.");
            }
            celebrationConfigurationProperty.objectReferenceValue = celebrationConfiguration;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(game);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, FunnyFaceDragScenePath);
        }

        private static void CreateChemistrySelectionScene(Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration celebrationConfiguration)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Canvas canvas = CreateCanvas();
            GameObject gameObject = new("ChemistrySelectionGame"); gameObject.transform.SetParent(canvas.transform, false);
            ChemistrySelectionGame game = gameObject.AddComponent<ChemistrySelectionGame>();
            SerializedObject serialized = new(game);
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(InterfaceFontPath);
            serialized.FindProperty("instruction").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/App/Games/ChemistrySelection/Instruction.mp3");
            serialized.FindProperty("principalArtwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ChemistrySelection/Art/Principal.png");
            serialized.FindProperty("option1Artwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ChemistrySelection/Art/Option1.png");
            serialized.FindProperty("option2Artwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ChemistrySelection/Art/Option2.png");
            serialized.FindProperty("option3Artwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ChemistrySelection/Art/Option3.png");
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
            SerializedProperty celebrationConfigurationProperty = serialized.FindProperty("celebrationConfiguration");
            if (celebrationConfigurationProperty == null) throw new System.InvalidOperationException("ChemistrySelectionGame is missing the celebrationConfiguration serialized property.");
            celebrationConfigurationProperty.objectReferenceValue = celebrationConfiguration;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(game);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ChemistrySelectionScenePath);
        }

        private static void CreateTrianglesShapeLogicScene(Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration celebrationConfiguration)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Canvas canvas = CreateCanvas();
            GameObject gameObject = new("TrianglesShapeLogicGame"); gameObject.transform.SetParent(canvas.transform, false);
            TrianglesShapeLogicGame game = gameObject.AddComponent<TrianglesShapeLogicGame>();
            SerializedObject serialized = new(game);
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(InterfaceFontPath);
            serialized.FindProperty("instruction").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/App/Games/TrianglesShapeLogic/Instruction.mp3");
            serialized.FindProperty("principalArtwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/TrianglesShapeLogic/Art/Principal.png");
            serialized.FindProperty("redTriangleArtwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/TrianglesShapeLogic/Art/Triangle1Drag1Drop2.png");
            serialized.FindProperty("blueTriangleArtwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/TrianglesShapeLogic/Art/Triangle2Drag2Drop1.png");
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
            SerializedProperty celebrationConfigurationProperty = serialized.FindProperty("celebrationConfiguration");
            if (celebrationConfigurationProperty == null) throw new System.InvalidOperationException("TrianglesShapeLogicGame is missing the celebrationConfiguration serialized property.");
            celebrationConfigurationProperty.objectReferenceValue = celebrationConfiguration;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(game);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, TrianglesShapeLogicScenePath);
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

        private static Sprite[] LoadBackgroundDecorations()
        {
            // Order must match the DecorationSpec layout in LobbyController: blob, blob
            // outline, spiral, hex, dots, ribbon, cloud, small blobs. LoadSprite forces each
            // PNG to import as a single Sprite so they resolve once the project is imported.
            Sprite[] sprites = new Sprite[BackgroundDecorationPaths.Length];
            for (int index = 0; index < BackgroundDecorationPaths.Length; index++)
            {
                sprites[index] = LoadSprite(BackgroundDecorationPaths[index]);
            }

            return sprites;
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
                "Assets/App/Shared/Audio",
                "Assets/App/Shared/UI",
                "Assets/App/Shared/UI/LevelChrome",
                "Assets/App/GameKits",
                "Assets/App/GameKits/DragDrop",
                "Assets/App/GameKits/DragDrop/Core",
                "Assets/App/GameKits/DragDrop/Runtime",
                "Assets/App/Games",
                "Assets/App/Games/Common",
                "Assets/App/Games/Classification",
                "Assets/App/Games/ShapeAnalogy",
                "Assets/App/Games/ShapeAnalogy/Sounds",
                "Assets/App/Games/ShapeAnalogy/Celebration",
                "Assets/App/Games/ClothesSelection",
                "Assets/App/Games/ClothesSelection/Art",
                "Assets/App/Games/ObjectSelection",
                "Assets/App/Games/ObjectSelection/Art",
                "Assets/App/Games/NumberPull",
                "Assets/App/Games/NumberPull/Domain",
                "Assets/App/Games/NumberPull/Resources",
                "Assets/App/Games/NumberPull/Resources/Audio",
                "Assets/App/Games/NumberPull/Resources/Characters",
                "Assets/App/Games/NumberPull/Resources/Particles",
                "Assets/App/Games/NumberPull/Resources/UI",
                "Assets/App/Games/MakeAnEmojiDrag",
                "Assets/App/Games/AnimalDrag",
                "Assets/App/Games/TrianglesCount",
                "Assets/App/Games/CubePlatform",
                "Assets/App/Games/CandiesLogic",
                "Assets/App/Games/SquaresSuccession",
                "Assets/App/Games/KitchenMathLogic",
                "Assets/App/Games/FunnyFaceDrag",
                "Assets/App/Games/ChemistrySelection",
                "Assets/App/Games/TrianglesShapeLogic",
                "Assets/App/Games/Memory",
                "Assets/App/Games/Matching",
                "Assets/App/Theme",
                BackgroundFolder,
                ThumbnailsFolder
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
