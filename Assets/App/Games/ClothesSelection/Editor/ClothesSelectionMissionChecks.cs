#if UNITY_EDITOR
using Lbs.MiniGames.Games.ClothesSelection;
using Lbs.MiniGames.Shared;
using Lbs.MiniGames.Shared.Results;
using Lbs.MiniGames.Shared.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lbs.MiniGames.Games.ClothesSelection.Editor
{
    public static class ClothesSelectionMissionChecks
    {
        public static void Run()
        {
            int failures = 0;
            string[] sprites =
            {
                "Assets/App/Games/ClothesSelection/Art/BaseOfAll.png",
                "Assets/App/Games/ClothesSelection/Art/FurnitureWObjects.png",
                "Assets/App/Games/ClothesSelection/Art/Heel.png",
                "Assets/App/Games/ClothesSelection/Art/Gloves.png",
                "Assets/App/Games/ClothesSelection/Art/Shoes.png"
            };
            foreach (string path in sprites)
            {
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (AssetDatabase.LoadAssetAtPath<Sprite>(path) == null || importer == null || importer.textureType != TextureImporterType.Sprite) failures++;
            }

            Scene scene = EditorSceneManager.OpenScene("Assets/App/Games/ClothesSelection/ClothesSelection.unity", OpenSceneMode.Single);
            ClothesSelectionGame game = Object.FindFirstObjectByType<ClothesSelectionGame>();
            if (game == null) failures++;
            else
            {
                game.Configure(null);
                failures += CheckRect("FurnitureWObjects", new Vector2(1005, 365), new Vector2(705, 500));
                failures += CheckRect("heelCard", new Vector2(550, 840), new Vector2(420, 160));
                failures += CheckRect("glovesCard", new Vector2(1005, 840), new Vector2(420, 160));
                failures += CheckRect("shoesCard", new Vector2(1455, 840), new Vector2(420, 160));
                failures += CheckRect("Exit", LevelChromeLayout.ExitCenter, LevelChromeLayout.ExitSize);
                failures += CheckRect("Hong", LevelChromeLayout.HongCenter, LevelChromeLayout.HongSize);
                RoundedSurface glovesCard = GameObject.Find("glovesCard")?.GetComponent<RoundedSurface>();
                if (glovesCard == null || glovesCard.OutlineThickness != 5f || glovesCard.color.a <= 0f || glovesCard.color.r > .9f) failures++;
                if (game.GetType().GetField("celebration4Star", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(game) == null
                    || game.GetType().GetField("celebration5Star", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(game) == null
                    || game.GetType().GetField("circleConfetti", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(game) == null
                    || game.GetType().GetField("rectangularConfetti", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(game) == null
                    || game.GetType().GetField("serpentina", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(game) == null
                    || game.GetType().GetField("serpentina2", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(game) == null
                    || game.GetType().GetField("serpentina3", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(game) == null) failures++;
                System.Reflection.BindingFlags privateInstance = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                game.GetType().GetMethod("CreateCelebration", privateInstance)?.Invoke(game, null);
                if (GameObject.Find("ResultCelebration")?.GetComponent<FinalCelebrationParticles>() == null) failures++;
                game.GetType().GetMethod("CreateFinal", privateInstance)?.Invoke(game, null);
                if (GameObject.Find("ResultBackdropDim") == null
                    || GameObject.Find("FinalHaloBlur")?.GetComponent<EllipseSurface>() == null
                    || GameObject.Find("FinalHalo")?.GetComponent<EllipseSurface>() == null) failures++;
                if (System.IO.File.Exists("Assets/App/Games/ClothesSelection/GameReference.jpeg")) failures++;
                if (GameObject.Find("Instruction") != null || GameObject.Find("Timer") != null || GameObject.Find("Counter") != null) failures++;
            }

            EditorSceneManager.CloseScene(scene, true);
            Debug.Log($"CLOTHES_SELECTION_MISSION_SUMMARY failures={failures} sprites={sprites.Length} layout=checked chrome=checked");
            EditorApplication.Exit(failures == 0 ? 0 : 1);
        }

        private static int CheckRect(string name, Vector2 expectedCenter, Vector2 expectedSize)
        {
            RectTransform rect = GameObject.Find(name)?.GetComponent<RectTransform>();
            if (rect == null || !rect.gameObject.activeInHierarchy) return 1;
            Vector2 center = new(rect.anchoredPosition.x + 960f, 540f - rect.anchoredPosition.y);
            return Vector2.Distance(center, expectedCenter) < 1f && Vector2.Distance(rect.sizeDelta, expectedSize) < 1f ? 0 : 1;
        }
    }
}
#endif
