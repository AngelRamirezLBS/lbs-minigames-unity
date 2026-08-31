#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Lbs.MiniGames.Games.ShapeAnalogy;

namespace Lbs.MiniGames.Games.ShapeAnalogy.Editor
{
    public static class ShapeAnalogyMissionChecks
    {
        public static void RunCelebration()
        {
            int failures = 0;
            string[] celebrationAssets =
            {
                "Assets/App/Games/ShapeAnalogy/Celebration/4Star.png",
                "Assets/App/Games/ShapeAnalogy/Celebration/5star.png",
                "Assets/App/Games/ShapeAnalogy/Celebration/CircleConfetti.png",
                "Assets/App/Games/ShapeAnalogy/Celebration/RectangularConfetti.png",
                "Assets/App/Games/ShapeAnalogy/Celebration/Serpentina.png"
            };
            foreach (string path in celebrationAssets)
            {
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (AssetDatabase.LoadAssetAtPath<Sprite>(path) == null || importer == null || importer.textureType != TextureImporterType.Sprite || !importer.alphaIsTransparency || importer.filterMode != FilterMode.Bilinear) failures++;
            }

            Scene scene = EditorSceneManager.OpenScene("Assets/App/Games/ShapeAnalogy/ShapeAnalogy.unity", OpenSceneMode.Single);
            ShapeAnalogyGame game = UnityEngine.Object.FindFirstObjectByType<ShapeAnalogyGame>();
            if (game == null) failures++;
            else
            {
                SerializedObject serialized = new(game);
                string[] references = { "celebration4Star", "celebration5Star", "circleConfetti", "rectangularConfetti", "serpentina" };
                foreach (string reference in references) if (serialized.FindProperty(reference).objectReferenceValue == null) failures++;
                game.CaptureSuccess();
                failures += CheckCelebration();
                Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
                foreach (ParticleSystem particles in canvas.GetComponentsInChildren<ParticleSystem>()) { particles.Simulate(.1f, true, true, true); particles.Simulate(.6f, true, false, true); if (particles.particleCount == 0) failures++; }
                foreach (ShapeAnalogyUIParticleRenderer bridge in canvas.GetComponentsInChildren<ShapeAnalogyUIParticleRenderer>()) { bridge.Refresh(); if (bridge.transform.parent.parent.name == "Stars" && (bridge.LastRenderedParticleCount == 0 || bridge.ActiveImageCount == 0)) failures++; }
                failures += CheckDistinctVisibleOutput(canvas);
                game.CaptureFinal();
                GameObject root = GameObject.Find("ResultCelebration");
                Image dim = GameObject.Find("ResultBackdropDim")?.GetComponent<Image>();
                if (!root || root.transform.Find("ResultBackdropGlow") != null || !root.transform.Find("Stars").gameObject.activeInHierarchy || !root.transform.Find("ConfettiStreamers").gameObject.activeInHierarchy || dim == null || dim.raycastTarget || dim.rectTransform.anchorMin != Vector2.zero || dim.rectTransform.anchorMax != Vector2.one || dim.rectTransform.offsetMin != Vector2.zero || dim.rectTransform.offsetMax != Vector2.zero || dim.color.a < .1f || dim.color.a > .16f) failures++;
                game.CaptureInitial();
                if (GameObject.Find("ResultCelebration") != null) failures++;
            }
            EditorSceneManager.CloseScene(scene, true);
            Debug.Log($"SHAPE_ANALOGY_CELEBRATION_SUMMARY failures={failures} sprites={celebrationAssets.Length} hierarchy=checked cleanup=checked");
            EditorApplication.Exit(failures == 0 ? 0 : 1);
        }

        public static void Run()
        {
            int failures = 0;
            string[] assets = { "Assets/App/Games/ShapeAnalogy/ShapeAnalogy.unity", "Assets/App/Games/ShapeAnalogy/Star_FullFilled.png", "Assets/App/Games/ShapeAnalogy/FinalStar.png", "Assets/App/Catalog/Data/ShapeAnalogyGame.asset", "Assets/App/Catalog/Data/ShapeCategory.asset", "Assets/App/Games/ShapeAnalogy/Sounds/Instruction.mp3", "Assets/App/Games/ShapeAnalogy/Sounds/TryAgain.mp3" };
            foreach (string path in assets) if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) == null) failures++;
            foreach (string path in System.IO.Directory.GetFiles("Assets/App/Games/ShapeAnalogy", "*.png", System.IO.SearchOption.AllDirectories)) if (AssetDatabase.LoadAssetAtPath<Sprite>(path.Replace('\\', '/')) == null) failures++;
            string[] hongPngs = { "Hong_Neutral.png", "Hong_Listening1.png", "Hong_Listening2.png", "Hong_Listening3.png" };
            foreach (string name in hongPngs) if (AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/" + name) == null) failures++;
            failures += ShapeAnalogyRule.Evaluate("filled-star", false) == ShapeAnalogyDropOutcome.Outside ? 0 : 1;
            failures += ShapeAnalogyRule.Evaluate(ShapeAnalogyRule.CorrectAnswer, true) == ShapeAnalogyDropOutcome.Correct ? 0 : 1;
            var state = new ShapeAnalogyState(); state.StartDrag(); state.Drop(ShapeAnalogyRule.CorrectAnswer, true); state.FinishCelebration(); state.ArmFinal();
            failures += state.AcceptFinalTap() ? 0 : 1;
            Scene scene = EditorSceneManager.OpenScene("Assets/App/Games/ShapeAnalogy/ShapeAnalogy.unity", OpenSceneMode.Single);
            var game = UnityEngine.Object.FindFirstObjectByType<ShapeAnalogyGame>();
            if (game == null) failures++;
            else
            {
                SerializedObject serialized = new(game);
                string[] references = { "exitIcon", "hongNeutral", "hong1", "hong2", "hong3", "instruction", "tryAgain", "celebration4Star", "celebration5Star", "circleConfetti", "rectangularConfetti", "serpentina" };
                foreach (string reference in references) if (serialized.FindProperty(reference).objectReferenceValue == null) failures++;
                game.CaptureInitial();
                failures += CheckVisible("Exit", new Vector2(170, 115));
                failures += CheckVisible("Hong", new Vector2(175, 930));
                failures += CheckVisible("GivenStar", new Vector2(850, 305));
                failures += CheckVisible("GivenHeart", new Vector2(1070, 305));
                failures += CheckVisible("PatternStar", new Vector2(850, 550));
                failures += CheckVisible("MissingSlot", new Vector2(1070, 550));
                failures += CheckVisible("HeartAnswer", new Vector2(760, 835));
                failures += CheckVisible("StarAnswer", new Vector2(960, 835));
                failures += CheckVisible("CorrectAnswer", new Vector2(1160, 835));
                failures += CheckArtwork("Exit", "ExitArtwork");
                failures += CheckArtwork("Hong", "HongArtwork");
                game.CaptureSuccess();
                failures += CheckCelebration();
                failures += CheckVisible("ResultBackdropDim", new Vector2(960, 540));
                game.CaptureFinal();
                failures += CheckVisible("FinalScore", new Vector2(820, 550));
                failures += CheckVisible("FinalStarA", new Vector2(1010, 550));
                failures += CheckVisible("FinalStarB", new Vector2(1140, 550));
            }
            EditorSceneManager.CloseScene(scene, true);
            Debug.Log($"SHAPE_ANALOGY_MISSION_SUMMARY failures={failures} assets={assets.Length} core=pass wiring=checked cleanup=checked idempotence=checked");
            if (failures != 0) EditorApplication.Exit(1); else EditorApplication.Exit(0);
        }

        private static int CheckVisible(string name, Vector2 expectedTopOriginCenter)
        {
            GameObject gameObject = GameObject.Find(name);
            if (gameObject == null || !gameObject.activeInHierarchy) return 1;
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            if (rect == null || rect.rect.width <= 0 || rect.rect.height <= 0) return 1;
            Vector2 center = new(rect.anchoredPosition.x + 960, 540 - rect.anchoredPosition.y);
            return Vector2.Distance(center, expectedTopOriginCenter) < 1f
                && center.x - rect.rect.width / 2 >= 0 && center.x + rect.rect.width / 2 <= 1920
                && center.y - rect.rect.height / 2 >= 0 && center.y + rect.rect.height / 2 <= 1080 ? 0 : 1;
        }

        private static int CheckArtwork(string parentName, string childName)
        {
            GameObject parent = GameObject.Find(parentName);
            Image image = parent ? parent.transform.Find(childName)?.GetComponent<Image>() : null;
            return image != null && image.sprite != null && image.gameObject.activeInHierarchy ? 0 : 1;
        }

        private static int CheckCelebration()
        {
            GameObject root = GameObject.Find("ResultCelebration");
            if (!root || root.transform.Find("Stars") == null || root.transform.Find("ConfettiStreamers") == null) return 1;
            foreach (Transform child in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (child.name.StartsWith("CelebrationStar") || child.name.StartsWith("CurvedStreamer") || child.name.StartsWith("GreenGlow")) return 1;

            ParticleSystem[] systems = root.GetComponentsInChildren<ParticleSystem>();
            if (systems.Length != 7) return 1;
            int maxParticles = 0;
            float starRate = 0f;
            float confettiRate = 0f;
            HashSet<uint> seeds = new();
            foreach (ParticleSystem system in systems)
            {
                ShapeAnalogyUIParticleRenderer bridge = system.GetComponentInChildren<ShapeAnalogyUIParticleRenderer>();
                ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
                if (bridge == null || bridge.AssignedSprite == null || renderer == null || renderer.enabled || system.main.loop || system.useAutoRandomSeed || !seeds.Add(system.randomSeed) || !Mathf.Approximately(system.main.duration, ShapeAnalogyCelebrationParticles.Duration) || !system.colorOverLifetime.enabled || !system.sizeOverLifetime.enabled) return 1;
                if (system.emission.burstCount != 1 || system.shape.shapeType != ParticleSystemShapeType.Rectangle || system.shape.scale.x > .11f || Mathf.Abs(system.shape.position.x) > .001f) return 1;
                ParticleSystem.VelocityOverLifetimeModule velocity = system.velocityOverLifetime;
                bool isCenter = system.name == "Serpentina";
                if (!velocity.enabled) return 1;
                if (!isCenter)
                {
                    float maxAt1 = velocity.x.mode == ParticleSystemCurveMode.TwoCurves ? velocity.x.curveMax.Evaluate(1f) : velocity.x.curve.Evaluate(1f);
                    float minAt1 = velocity.x.mode == ParticleSystemCurveMode.TwoCurves ? velocity.x.curveMin.Evaluate(1f) : velocity.x.curve.Evaluate(1f);
                    if (Mathf.Abs(maxAt1) < 8.5f && Mathf.Abs(minAt1) < 8.5f) return 1;
                    if (velocity.y.mode != ParticleSystemCurveMode.TwoCurves) return 1;
                }
                else
                {
                    if (velocity.y.mode != ParticleSystemCurveMode.TwoCurves) return 1;
                }
                if (velocity.z.mode != ParticleSystemCurveMode.TwoCurves && velocity.z.mode != ParticleSystemCurveMode.Curve) return 1;
                maxParticles += system.main.maxParticles;
                float rate = system.emission.rateOverTime.constant;
                if (system.transform.parent.name == "Stars")
                {
                    starRate += rate;
                    if (system.main.startSize.constantMin < .08f || system.main.startSize.constantMax > .25f || system.main.gravityModifier.constant != 0f) return 1;
                }
                else
                {
                    confettiRate += rate;
                    if (system.main.gravityModifier.constant < .3f || system.main.gravityModifier.constant > .6f || !system.rotationOverLifetime.enabled || (system.name == "Serpentina" && (system.main.startSize.constantMin < .16f || system.main.startLifetime.constantMin < 1.25f || system.emission.GetBurst(0).count.constant < 2))) return 1;
                }
            }
            return seeds.Count == systems.Length && maxParticles == ShapeAnalogyCelebrationParticles.TotalMaxParticles && starRate >= 5f && starRate <= 7f && confettiRate >= 6f && confettiRate <= 9f ? 0 : 1;
        }

        private static int CheckDistinctVisibleOutput(Canvas canvas)
        {
            HashSet<string> particleFields = new();
            foreach (ParticleSystem system in canvas.GetComponentsInChildren<ParticleSystem>())
            {
                ParticleSystem.Particle[] particles = new ParticleSystem.Particle[system.main.maxParticles];
                int count = system.GetParticles(particles);
                if (count == 0) return 1;
                ParticleSystem.Particle first = particles[0];
                if (Mathf.Abs(first.position.x) > 2.2f || first.position.y < -1.6f || !particleFields.Add($"{first.randomSeed}")) return 1;
            }
            return particleFields.Count == 7 ? 0 : 1;
        }
    }
}
#endif
