using System.Collections.Generic;
using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Games.Classification;
using Lbs.MiniGames.Games.ShapeAnalogy;
using Lbs.MiniGames.Games.AnimalDrag;
using Lbs.MiniGames.Games.ClothesSelection;
using Lbs.MiniGames.Games.ObjectSelection;
using Lbs.MiniGames.Games.MakeAnEmojiDrag;
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
    public static class FirstVerticalSliceInstaller
    {
        private const string CatalogFolder = "Assets/App/Catalog/Data";
        private const string CategoryPath = CatalogFolder + "/AnimalsCategory.asset";
        private const string DefinitionPath = CatalogFolder + "/ClassificationGame.asset";
        private const string CatalogPath = CatalogFolder + "/MiniGameCatalog.asset";
        private const string MathematicsCategoryPath = CatalogFolder + "/MathematicsCategory.asset";
        private const string NumberPullDefinitionPath = CatalogFolder + "/NumberPullGame.asset";
        private const string WildWhizCategoryPath = CatalogFolder + "/WildWhizCategory.asset";
        private const string WildWhizDefinitionPath = CatalogFolder + "/WildWhizGame.asset";
        private const string VolteRegularPath = "Assets/App/Theme/Fonts/Volte-Regular.otf";
        private const string BrandLogoPath = "Assets/App/Theme/Brand/LbsPlusLogo.png";
        private const string WolfieHubPath = "Assets/App/Theme/Brand/WolfieHub.png";
        private const string ClassificationThumbnailPath = "Assets/App/Games/Classification/ClassificationThumbnail.png";
        private const string ShapeCategoryPath = CatalogFolder + "/ShapeCategory.asset";
        private const string ShapeDefinitionPath = CatalogFolder + "/ShapeAnalogyGame.asset";
        private const string ShapeScenePath = "Assets/App/Games/ShapeAnalogy/ShapeAnalogy.unity";
        private const string ClothesDefinitionPath = CatalogFolder + "/ClothesSelectionGame.asset";
        private const string ClothesScenePath = "Assets/App/Games/ClothesSelection/ClothesSelection.unity";
        private const string ObjectSelectionDefinitionPath = CatalogFolder + "/ObjectSelectionGame.asset";
        private const string ObjectSelectionScenePath = "Assets/App/Games/ObjectSelection/ObjectSelection.unity";
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
            ConfigureMakeAnEmojiDragSprites();
            ConfigureTrianglesCountSprites();
            ConfigureCubePlatformSprites();
            ConfigureCandiesLogicSprites();
            ConfigureSquaresSuccessionSprites();
            ConfigureKitchenMathLogicSprites();
            ConfigureFunnyFaceDragSprites();
            ConfigureChemistrySelectionSprites();
            ConfigureTrianglesShapeLogicSprites();
            ConfigureOrientation();

            GameCategory animals = CreateOrLoad<GameCategory>(CategoryPath);
            animals.Configure("animals", "Animals", "Learn how animals belong to different groups.");

            // Difficulty definitions (future-ready, non-destructive)
            DifficultyDefinition easy = CreateOrLoad<DifficultyDefinition>(DifficultyEasyPath);
            easy.Configure("easy", "Easy", 0, "Gentle introduction with fewer distractors.");
            DifficultyDefinition medium = CreateOrLoad<DifficultyDefinition>(DifficultyMediumPath);
            medium.Configure("medium", "Medium", 1, "Balanced challenge.");
            DifficultyDefinition hard = CreateOrLoad<DifficultyDefinition>(DifficultyHardPath);
            hard.Configure("hard", "Hard", 2, "Extra cards and timed pressure.");
            var allDifficulties = new List<DifficultyDefinition> { easy, medium, hard };

            GameDefinition classification = CreateOrLoad<GameDefinition>(DefinitionPath);
            classification.Configure(
                "classification.animals",
                "Animal Classification",
                animals,
                "Classification",
                "Drag the dolphin to the correct group.");
            classification.SetThumbnail(AssetDatabase.LoadAssetAtPath<Sprite>(ClassificationThumbnailPath));
            classification.ConfigureDifficulties(allDifficulties, medium);

            GameCategory mathematics = AssetDatabase.LoadAssetAtPath<GameCategory>(MathematicsCategoryPath);
            GameDefinition numberPull = AssetDatabase.LoadAssetAtPath<GameDefinition>(NumberPullDefinitionPath);
            GameCategory wildWhiz = AssetDatabase.LoadAssetAtPath<GameCategory>(WildWhizCategoryPath);
            GameDefinition wildWhizGame = AssetDatabase.LoadAssetAtPath<GameDefinition>(WildWhizDefinitionPath);

            GameCatalog catalog = CreateOrLoad<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                throw new System.InvalidOperationException("MiniGameCatalog could not be created or loaded.");
            }

            catalog.EnsureCategory(animals);
            catalog.EnsureCategory(mathematics);
            catalog.EnsureCategory(wildWhiz);
            GameCategory shapes = CreateOrLoad<GameCategory>(ShapeCategoryPath);
            shapes.Configure("shape-analogy", "Shape Analogy", "Find the shape and fill relationship.");
            GameDefinition shapeGame = CreateOrLoad<GameDefinition>(ShapeDefinitionPath);
            shapeGame.Configure("shape.analogy", "Shape Analogy", shapes, "ShapeAnalogy", "Look at the relationship and drag the correct card into the empty space.");
            shapeGame.ConfigureDifficulties(allDifficulties, medium);
            catalog.EnsureCategory(shapes);
            catalog.EnsureGame(classification);
            catalog.EnsureGame(numberPull);
            catalog.EnsureGame(wildWhizGame);
            catalog.EnsureGame(shapeGame);
            GameDefinition clothesGame = CreateOrLoad<GameDefinition>(ClothesDefinitionPath);
            clothesGame.Configure("clothes.selection", "Clothes Selection", shapes, "ClothesSelection", "Choose the item that is not footwear.");
            clothesGame.SetThumbnail(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ClothesSelection/Art/FurnitureWObjects.png"));
            clothesGame.ConfigureDifficulties(allDifficulties, medium);
            catalog.EnsureGame(clothesGame);
            GameDefinition objectSelectionGame = CreateOrLoad<GameDefinition>(ObjectSelectionDefinitionPath);
            objectSelectionGame.Configure("object.selection", "Object Selection", shapes, "ObjectSelection", "Choose the object that does not belong in the group.");
            objectSelectionGame.SetThumbnail(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ObjectSelection/Art/tenis.png"));
            objectSelectionGame.ConfigureDifficulties(allDifficulties, medium);
            catalog.EnsureGame(objectSelectionGame);
            GameDefinition makeAnEmojiDragGame = CreateOrLoad<GameDefinition>(MakeAnEmojiDragDefinitionPath);
            makeAnEmojiDragGame.Configure("make.emoji.drag", "Make An Emoji Drag", shapes, "MakeAnEmojiDrag", "Drag the matching strips to make a happy emoji.");
            makeAnEmojiDragGame.SetThumbnail(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/MakeAnEmojiDrag/Art/segunda.png"));
            makeAnEmojiDragGame.ConfigureDifficulties(allDifficulties, medium);
            catalog.EnsureGame(makeAnEmojiDragGame);
            GameDefinition animalDragGame = CreateOrLoad<GameDefinition>(AnimalDragDefinitionPath);
            animalDragGame.Configure("animal.drag", "Animal Drag", shapes, "AnimalDrag", "Help the animals get to their homes.");
            animalDragGame.SetThumbnail(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/AnimalDrag/Art/casitas.png"));
            animalDragGame.ConfigureDifficulties(allDifficulties, medium);
            catalog.EnsureGame(animalDragGame);
            GameDefinition trianglesCountGame = CreateOrLoad<GameDefinition>(TrianglesCountDefinitionPath);
            trianglesCountGame.Configure("triangles.count", "Triangles Count", shapes, "TrianglesCount", "Count the triangles.");
            trianglesCountGame.SetThumbnail(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/TrianglesCount/Art/Triangle_Principal.png"));
            trianglesCountGame.ConfigureDifficulties(allDifficulties, medium);
            catalog.EnsureGame(trianglesCountGame);
            GameDefinition cubePlatformGame = CreateOrLoad<GameDefinition>(CubePlatformDefinitionPath);
            cubePlatformGame.Configure("cube.platform", "Cube Platform", shapes, "CubePlatform", "Which box is the lightest?");
            cubePlatformGame.SetThumbnail(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/CubePlatform/Art/Box3.png"));
            cubePlatformGame.ConfigureDifficulties(allDifficulties, medium);
            catalog.EnsureGame(cubePlatformGame);
            GameDefinition candiesLogicGame = CreateOrLoad<GameDefinition>(CandiesLogicDefinitionPath);
            candiesLogicGame.Configure("candies.logic", "Candies Logic", shapes, "CandiesLogic", "What is the big circle?");
            candiesLogicGame.SetThumbnail(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/CandiesLogic/Art/CandiesandSweets.png"));
            candiesLogicGame.ConfigureDifficulties(allDifficulties, medium);
            catalog.EnsureGame(candiesLogicGame);
            GameDefinition squaresSuccessionGame = CreateOrLoad<GameDefinition>(SquaresSuccessionDefinitionPath);
            squaresSuccessionGame.Configure("squares.succession", "Squares Succession", shapes, "SquaresSuccession", "What comes next?");
            Sprite squaresThumb = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/SquaresSuccession/Art/Result.png");
            if (squaresThumb == null) squaresThumb = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/SquaresSuccession/Art/SecuenceSquares.png");
            squaresSuccessionGame.SetThumbnail(squaresThumb);
            squaresSuccessionGame.ConfigureDifficulties(allDifficulties, medium);
            catalog.EnsureGame(squaresSuccessionGame);
            GameDefinition kitchenMathLogicGame = CreateOrLoad<GameDefinition>(KitchenMathLogicDefinitionPath);
            kitchenMathLogicGame.Configure("kitchen.math.logic", "Kitchen Math Logic", shapes, "KitchenMathLogic", "What is the result? Toast + cheese + knife =");
            kitchenMathLogicGame.SetThumbnail(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/KitchenMathLogic/Art/Option4.png"));
            kitchenMathLogicGame.ConfigureDifficulties(allDifficulties, medium);
            catalog.EnsureGame(kitchenMathLogicGame);
            GameDefinition funnyFaceDragGame = CreateOrLoad<GameDefinition>(FunnyFaceDragDefinitionPath);
            funnyFaceDragGame.Configure("funnyface.drag", "Funny Face Drag", shapes, "FunnyFaceDrag", "Complete the funny face");
            Sprite funnyThumb = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/FunnyFaceDrag/Art/Drag3.png");
            if (funnyThumb == null) funnyThumb = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/FunnyFaceDrag/Art/Principal1.png");
            funnyFaceDragGame.SetThumbnail(funnyThumb);
            funnyFaceDragGame.ConfigureDifficulties(allDifficulties, medium);
            catalog.EnsureGame(funnyFaceDragGame);
            GameDefinition chemistrySelectionGame = CreateOrLoad<GameDefinition>(ChemistrySelectionDefinitionPath);
            chemistrySelectionGame.Configure("chemistry.selection", "Chemistry Selection", shapes, "ChemistrySelection", "Choose the correct chemistry relationship.");
            chemistrySelectionGame.SetThumbnail(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ChemistrySelection/Art/Principal.png"));
            chemistrySelectionGame.ConfigureDifficulties(allDifficulties, medium);
            catalog.EnsureGame(chemistrySelectionGame);
            GameDefinition trianglesShapeLogicGame = CreateOrLoad<GameDefinition>(TrianglesShapeLogicDefinitionPath);
            trianglesShapeLogicGame.Configure("triangles.shape.logic", "Triangles Shape Logic", shapes, "TrianglesShapeLogic", "Complete the table. Use the correct colored triangles.");
            trianglesShapeLogicGame.SetThumbnail(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/TrianglesShapeLogic/Art/Principal.png"));
            trianglesShapeLogicGame.ConfigureDifficulties(allDifficulties, medium);
            catalog.EnsureGame(trianglesShapeLogicGame);

            // Global audio config (persistent music) — non-destructive, uses existing bg track
            AppAudioConfig audioConfig = CreateOrLoad<AppAudioConfig>(AudioConfigPath);
            Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration celebrationConfiguration = CreateOrLoad<Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration>(FinalCelebrationConfigurationPath);
            AudioClip bg = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/App/Games/ShapeAnalogy/Sounds/Music/bg_cabinet_menu.mp3");
            if (bg == null) bg = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/ShapeAnalogy/Music/bg_cabinet_menu.mp3");
            if (bg != null) audioConfig.Configure(bg, 0.25f, 0.125f);

            EditorUtility.SetDirty(animals);
            EditorUtility.SetDirty(classification);
            EditorUtility.SetDirty(easy);
            EditorUtility.SetDirty(medium);
            EditorUtility.SetDirty(hard);
            if (mathematics != null) EditorUtility.SetDirty(mathematics);
            if (numberPull != null) EditorUtility.SetDirty(numberPull);
            if (wildWhiz != null) EditorUtility.SetDirty(wildWhiz);
            if (wildWhizGame != null) EditorUtility.SetDirty(wildWhizGame);
            EditorUtility.SetDirty(shapes);
            EditorUtility.SetDirty(catalog);
            EditorUtility.SetDirty(shapeGame);
            EditorUtility.SetDirty(clothesGame);
            EditorUtility.SetDirty(objectSelectionGame);
            EditorUtility.SetDirty(makeAnEmojiDragGame);
            EditorUtility.SetDirty(animalDragGame);
            EditorUtility.SetDirty(trianglesCountGame);
            EditorUtility.SetDirty(cubePlatformGame);
            EditorUtility.SetDirty(candiesLogicGame);
            EditorUtility.SetDirty(squaresSuccessionGame);
            EditorUtility.SetDirty(kitchenMathLogicGame);
            EditorUtility.SetDirty(funnyFaceDragGame);
            EditorUtility.SetDirty(chemistrySelectionGame);
            EditorUtility.SetDirty(trianglesShapeLogicGame);
            EditorUtility.SetDirty(audioConfig);
            AssetDatabase.SaveAssets();
            celebrationConfiguration = AssetDatabase.LoadAssetAtPath<Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration>(FinalCelebrationConfigurationPath);
            if (celebrationConfiguration == null)
            {
                throw new System.InvalidOperationException("FinalCelebrationConfiguration could not be loaded from its persistent asset path.");
            }

            CreateBootstrapScene(catalog);
            CreateLobbyScene(catalog);
            CreateClassificationScene();
            EnsureBuildScene("Assets/App/Bootstrap/Bootstrap.unity", true);
            EnsureBuildScene("Assets/App/Lobby/Lobby.unity", true);
            EnsureBuildScene("Assets/App/Games/Classification/Classification.unity", true);
            EnsureBuildScene("Assets/App/Games/NumberPull/NumberPull.unity", true);
            EnsureBuildScene("Assets/App/Games/WildWhiz/WildWhiz.unity", true);
            CreateShapeAnalogyScene(celebrationConfiguration);
            EnsureBuildScene(ShapeScenePath, true);
            CreateClothesSelectionScene(celebrationConfiguration);
            EnsureBuildScene(ClothesScenePath, true);
            CreateObjectSelectionScene(celebrationConfiguration);
            EnsureBuildScene(ObjectSelectionScenePath, true);
            CreateMakeAnEmojiDragScene(celebrationConfiguration);
            EnsureBuildScene(MakeAnEmojiDragScenePath, true);
            ConfigureAnimalDragSprites();
            CreateAnimalDragScene(celebrationConfiguration);
            EnsureBuildScene(AnimalDragScenePath, true);
            ConfigureTrianglesCountSprites();
            CreateTrianglesCountScene(celebrationConfiguration);
            EnsureBuildScene(TrianglesCountScenePath, true);
            ConfigureCubePlatformSprites();
            CreateCubePlatformScene(celebrationConfiguration);
            EnsureBuildScene(CubePlatformScenePath, true);
            ConfigureCandiesLogicSprites();
            CreateCandiesLogicScene(celebrationConfiguration);
            EnsureBuildScene(CandiesLogicScenePath, true);
            ConfigureSquaresSuccessionSprites();
            CreateSquaresSuccessionScene(celebrationConfiguration);
            EnsureBuildScene(SquaresSuccessionScenePath, true);
            ConfigureKitchenMathLogicSprites();
            CreateKitchenMathLogicScene(celebrationConfiguration);
            EnsureBuildScene(KitchenMathLogicScenePath, true);
            ConfigureFunnyFaceDragSprites();
            CreateFunnyFaceDragScene(celebrationConfiguration);
            EnsureBuildScene(FunnyFaceDragScenePath, true);
            ConfigureChemistrySelectionSprites();
            CreateChemistrySelectionScene(celebrationConfiguration);
            EnsureBuildScene(ChemistrySelectionScenePath, true);
            ConfigureTrianglesShapeLogicSprites();
            CreateTrianglesShapeLogicScene(celebrationConfiguration);
            EnsureBuildScene(TrianglesShapeLogicScenePath, true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Lbs.MiniGames.Catalog.Editor.GameCatalogValidation.ValidateCatalogs();
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

        private static void CreateShapeAnalogyScene(Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration celebrationConfiguration)
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

        private static void ConfigureMakeAnEmojiDragSprites()
        {
            string[] names = { "primera.png", "segunda.png", "tercera.png" };
            foreach (string name in names)
            {
                TextureImporter importer = AssetImporter.GetAtPath("Assets/App/Games/MakeAnEmojiDrag/Art/" + name) as TextureImporter;
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

        private static void ConfigureAnimalDragSprites()
        {
            string[] names = { "casitas.png", "gato.png", "cerdo.png" };
            foreach (string name in names)
            {
                TextureImporter importer = AssetImporter.GetAtPath("Assets/App/Games/AnimalDrag/Art/" + name) as TextureImporter;
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

        private static void ConfigureTrianglesCountSprites()
        {
            string[] names = { "Triangle_Principal.png", "Reveal.png" };
            foreach (string name in names)
            {
                TextureImporter importer = AssetImporter.GetAtPath("Assets/App/Games/TrianglesCount/Art/" + name) as TextureImporter;
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

        private static void ConfigureCubePlatformSprites()
        {
            string[] names = { "PlatformWBoxes.png", "Box1.png", "Box2.png", "Box3.png" };
            foreach (string name in names)
            {
                TextureImporter importer = AssetImporter.GetAtPath("Assets/App/Games/CubePlatform/Art/" + name) as TextureImporter;
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

        private static void ConfigureCandiesLogicSprites()
        {
            string[] names = { "CandiesandSweets.png" };
            foreach (string name in names)
            {
                TextureImporter importer = AssetImporter.GetAtPath("Assets/App/Games/CandiesLogic/Art/" + name) as TextureImporter;
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

        private static void ConfigureSquaresSuccessionSprites()
        {
            string[] names = { "SecuenceSquares.png", "Option1.png", "Option2.png", "Option3.png", "Result.png", "Reveal.png" };
            foreach (string name in names)
            {
                TextureImporter importer = AssetImporter.GetAtPath("Assets/App/Games/SquaresSuccession/Art/" + name) as TextureImporter;
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

        private static void ConfigureFunnyFaceDragSprites()
        {
            string[] names = { "Background.png", "Principal1.png", "Principal2.png", "Drag1.png", "Drag2.png", "Drag3.png" };
            foreach (string name in names)
            {
                TextureImporter importer = AssetImporter.GetAtPath("Assets/App/Games/FunnyFaceDrag/Art/" + name) as TextureImporter;
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

        private static void ConfigureChemistrySelectionSprites()
        {
            string[] names = { "Principal.png", "Option1.png", "Option2.png", "Option3.png" };
            foreach (string name in names)
            {
                TextureImporter importer = AssetImporter.GetAtPath("Assets/App/Games/ChemistrySelection/Art/" + name) as TextureImporter;
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

        private static void ConfigureTrianglesShapeLogicSprites()
        {
            string[] names = { "Principal.png", "Triangle1Drag1Drop2.png", "Triangle2Drag2Drop1.png" };
            foreach (string name in names)
            {
                TextureImporter importer = AssetImporter.GetAtPath("Assets/App/Games/TrianglesShapeLogic/Art/" + name) as TextureImporter;
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

        private static void ConfigureKitchenMathLogicSprites()
        {
            string[] names = { "Principal.png", "Option1.png", "Option2.png", "Option3.png", "Option4.png" };
            foreach (string name in names)
            {
                TextureImporter importer = AssetImporter.GetAtPath("Assets/App/Games/KitchenMathLogic/Art/" + name) as TextureImporter;
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
            GameObject gameObject = new("ClothesSelectionGame"); gameObject.transform.SetParent(canvas.transform, false);
            ClothesSelectionGame game = gameObject.AddComponent<ClothesSelectionGame>();
            SerializedObject serialized = new(game);
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(VolteRegularPath);
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
            GameObject gameObject = new("ObjectSelectionGame"); gameObject.transform.SetParent(canvas.transform, false);
            ObjectSelectionGame game = gameObject.AddComponent<ObjectSelectionGame>();
            SerializedObject serialized = new(game);
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(VolteRegularPath);
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
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(VolteRegularPath);
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
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(VolteRegularPath);
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

        private static void CreateCubePlatformScene(Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration celebrationConfiguration)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Canvas canvas = CreateCanvas();
            GameObject gameObject = new("CubePlatformGame"); gameObject.transform.SetParent(canvas.transform, false);
            CubePlatformGame game = gameObject.AddComponent<CubePlatformGame>();
            SerializedObject serialized = new(game);
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(VolteRegularPath);
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

        private static void CreateTrianglesCountScene(Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration celebrationConfiguration)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Canvas canvas = CreateCanvas();
            GameObject gameObject = new("TrianglesCountGame"); gameObject.transform.SetParent(canvas.transform, false);
            TrianglesCountGame game = gameObject.AddComponent<TrianglesCountGame>();
            SerializedObject serialized = new(game);
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(VolteRegularPath);
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

        private static void CreateSquaresSuccessionScene(Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration celebrationConfiguration)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Canvas canvas = CreateCanvas();
            GameObject gameObject = new("SquaresSuccessionGame"); gameObject.transform.SetParent(canvas.transform, false);
            SquaresSuccessionGame game = gameObject.AddComponent<SquaresSuccessionGame>();
            SerializedObject serialized = new(game);
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(VolteRegularPath);
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
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(VolteRegularPath);
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
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(VolteRegularPath);
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
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(VolteRegularPath);
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
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(VolteRegularPath);
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

        private static void CreateCandiesLogicScene(Lbs.MiniGames.Shared.Results.FinalCelebrationConfiguration celebrationConfiguration)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Canvas canvas = CreateCanvas();
            GameObject gameObject = new("CandiesLogicGame"); gameObject.transform.SetParent(canvas.transform, false);
            CandiesLogicGame game = gameObject.AddComponent<CandiesLogicGame>();
            SerializedObject serialized = new(game);
            serialized.FindProperty("font").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(VolteRegularPath);
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
                "Assets/App/Games/WildWhiz",
                "Assets/App/Games/WildWhiz/Art",
                "Assets/App/Games/WildWhiz/Audio",
                "Assets/App/Games/WildWhiz/Data",
                "Assets/App/Games/ShapeAnalogy",
                "Assets/App/Games/ShapeAnalogy/Sounds",
                "Assets/App/Games/Memory",
                "Assets/App/Games/Matching",
                "Assets/App/Games/ObjectSelection",
                "Assets/App/Games/ObjectSelection/Art",
                "Assets/App/Games/ChemistrySelection",
                "Assets/App/Games/ChemistrySelection/Art",
                "Assets/App/Games/TrianglesShapeLogic",
                "Assets/App/Games/TrianglesShapeLogic/Art"
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
