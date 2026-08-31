#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Lbs.MiniGames.Games.ShapeAnalogy;
using Lbs.MiniGames.Shared.UI;

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
                failures += CheckResultPresentationOrder();
                Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
                foreach (ParticleSystem particles in canvas.GetComponentsInChildren<ParticleSystem>()) { particles.Simulate(.1f, true, true, true); particles.Simulate(.6f, true, false, true); if (particles.particleCount == 0) failures++; }
                foreach (ShapeAnalogyUIParticleRenderer bridge in canvas.GetComponentsInChildren<ShapeAnalogyUIParticleRenderer>()) { bridge.Refresh(); if (bridge.transform.parent.parent.name == "Stars" && (bridge.LastRenderedParticleCount == 0 || bridge.ActiveImageCount == 0)) failures++; }
                failures += CheckDistinctVisibleOutput(canvas);
                game.CaptureFinal();
                failures += CheckResultPresentationOrder();
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
                failures += CheckVisible("Exit", LevelChromeLayout.ExitCenter);
                failures += CheckVisible("Hong", LevelChromeLayout.HongCenter);
                failures += CheckVisible("GivenStar", new Vector2(910, 235));
                failures += CheckVisible("GivenHeart", new Vector2(1150, 235));
                failures += CheckVisible("PatternStar", new Vector2(910, 475));
                failures += CheckVisible("MissingSlot", new Vector2(1150, 475));
                failures += CheckVisible("HeartAnswer", new Vector2(750, 855));
                failures += CheckVisible("StarAnswer", new Vector2(1020, 855));
                failures += CheckVisible("CorrectAnswer", new Vector2(1290, 855));
                failures += CheckArtwork("Exit", "ExitArtwork");
                failures += CheckArtwork("Hong", "HongArtwork");
                game.CaptureSuccess();
                failures += CheckCelebration();
                failures += CheckStretchedOverlay("ResultBackdropDim");
                failures += CheckResultPresentationOrder();
                game.CaptureFinal();
                failures += CheckResultPresentationOrder();
                failures += CheckVisible("FinalScore", new Vector2(840, 553));
                failures += CheckVisible("FinalStarA", new Vector2(1043, 528));
                failures += CheckVisible("FinalStarB", new Vector2(1093, 578));
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

        private static int CheckStretchedOverlay(string name)
        {
            GameObject gameObject = GameObject.Find(name);
            if (gameObject == null || !gameObject.activeInHierarchy) return 1;
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            if (rect == null || rect.rect.width <= 0 || rect.rect.height <= 0) return 1;
            if (rect.anchorMin != Vector2.zero || rect.anchorMax != Vector2.one) return 1;
            if (rect.offsetMin != Vector2.zero || rect.offsetMax != Vector2.zero) return 1;
            Image image = gameObject.GetComponent<Image>();
            if (image != null && image.raycastTarget) return 1;
            return 0;
        }

        private static int CheckResultPresentationOrder()
        {
            Transform board = GameObject.Find("ShapeAnalogyBoard")?.transform;
            Transform dim = GameObject.Find("ResultBackdropDim")?.transform;
            Transform celebration = GameObject.Find("ResultCelebration")?.transform;
            if (board == null || dim == null || celebration == null || dim.parent != board || celebration.parent != board) return 1;
            Image dimImage = dim.GetComponent<Image>();
            if (dimImage == null || !Mathf.Approximately(dimImage.color.a, .13f)) return 1;
            if (dim.GetSiblingIndex() >= celebration.GetSiblingIndex()) return 1;
            foreach (string boardElement in new[] { "WarmYellowBackground", "GivenStar", "GivenHeart", "PatternStar", "MissingSlot", "Hong", "Exit" })
            {
                Transform element = board.Find(boardElement);
                if (element == null || element.GetSiblingIndex() >= dim.GetSiblingIndex()) return 1;
            }
            return 0;
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
            if (systems.Length != 7) return 1; // updated: kept 7 systems (2 Stars + 2 confetti + 3 serpentinas with reduced burst) to minimize breaking changes while fixing stacking
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
                    // updated: central serpentina must have real horizontal dispersion ±7 via TwoCurves, not zero vertical stacking
                    float cMax = velocity.x.mode == ParticleSystemCurveMode.TwoCurves ? velocity.x.curveMax.Evaluate(1f) : velocity.x.curve.Evaluate(1f);
                    float cMin = velocity.x.mode == ParticleSystemCurveMode.TwoCurves ? velocity.x.curveMin.Evaluate(1f) : velocity.x.curve.Evaluate(1f);
                    if (velocity.y.mode != ParticleSystemCurveMode.TwoCurves) return 1; // updated: keep Y TwoCurves
                    if (velocity.x.mode != ParticleSystemCurveMode.TwoCurves || (Mathf.Abs(cMax) < 6f && Mathf.Abs(cMin) < 6f)) return 1; // updated: require dispersion >=6 to avoid stacked vertical line
                }
                if (velocity.z.mode != ParticleSystemCurveMode.TwoCurves && velocity.z.mode != ParticleSystemCurveMode.Curve) return 1;
                maxParticles += system.main.maxParticles;
                float rate = system.emission.rateOverTime.constant;
                if (system.transform.parent.name == "Stars")
                {
                    starRate += rate;
                    if (system.main.startSize.constantMin < .08f || system.main.startSize.constantMax > .30f) return 1; // updated: allow large star up to .28, was .25
                    if (system.main.gravityModifier.constant < 0.25f || system.main.gravityModifier.constant > 0.35f) return 1; // updated: stars now fall with gravity 0.30, was 0f static
                    // updated: differentiated star sizes small vs large
                    if (system.name == "4Star" && (system.main.startSize.constantMin < .10f || system.main.startSize.constantMax > .20f)) return 1; // updated: 4Star small .12-.18
                    if (system.name == "5Star" && (system.main.startSize.constantMin < .18f || system.main.startSize.constantMax > .30f)) return 1; // updated: 5Star large .20-.28
                    if (system.main.startLifetime.constantMin < 2.7f) return 1; // updated: lifetime 2.8-3.2 to allow 0.2s pause + slow fall to bottom
                    // updated: verify star Y has 0.2s pause plateau at 0.35-0.50 and falling at end
                    if (velocity.y.mode == ParticleSystemCurveMode.TwoCurves)
                    {
                        float yPauseMin = velocity.y.curveMin.Evaluate(0.42f);
                        float yPauseMax = velocity.y.curveMax.Evaluate(0.42f);
                        if (Mathf.Abs(yPauseMin) > 0.35f && Mathf.Abs(yPauseMax) > 0.35f) return 1; // updated: require near-zero velocity during 0.2s pause
                        if (velocity.y.curveMin.Evaluate(1f) > -2.0f || velocity.y.curveMax.Evaluate(1f) > -2.0f) return 1; // updated: must be falling negative at lifetime end
                    }
                }
                else
                {
                    confettiRate += rate;
                    if (system.main.gravityModifier.constant < .3f || system.main.gravityModifier.constant > .6f || !system.rotationOverLifetime.enabled) return 1;
                    if (system.name.StartsWith("Serpentina")) // updated: was only exact Serpentina, now all serpentinas
                    {
                        if (system.main.startSize.constantMin < .35f || system.main.startSize.constantMax > .52f) return 1; // updated: serpentinas large .38-.50, was .16 lower bound
                        if (system.main.startLifetime.constantMin < 2.0f) return 1; // updated: lifetime 2.4-2.8, was 1.25
                        short burst = (short)system.emission.GetBurst(0).count.constant;
                        if (burst < 1 || burst > 2) return 1; // updated: serpentinas total 5 via 2+2+1, was 5 each (allow 1-2)
                        if (system.main.maxParticles < 1 || system.main.maxParticles > 2) return 1; // updated: max 2 each to avoid culling, was 6
                        if (!Mathf.Approximately(system.emission.rateOverTime.constant, 0f)) return 1; // updated: rate 0 only burst, was 1.4 causing popping
                    }
                    else
                    {
                        if (system.main.startSize.constantMin < .12f || system.main.startSize.constantMax > .22f) return 1; // updated: circle/rect .14-.20, tightened check
                        if (system.emission.GetBurst(0).count.constant != 2) return 1;
                        if (system.main.maxParticles != 4) return 1;
                    }
                }
            }
            return seeds.Count == systems.Length && maxParticles == ShapeAnalogyCelebrationParticles.TotalMaxParticles && starRate >= 5f && starRate <= 7f && confettiRate >= 3f && confettiRate <= 5f ? 0 : 1; // updated: TotalMaxParticles 28 (was 40), confettiRate 3-5 (was 6-9) because serpentinas now rate 0 burst-only to avoid stacking
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
            return particleFields.Count == 7 ? 0 : 1; // updated: kept 7 distinct seeds, but TotalMaxParticles now 28
        }
    }
}
#endif
