using UnityEngine;
using UnityEngine.UI;
using Lbs.MiniGames.Shared;

namespace Lbs.MiniGames.Shared.Results
{
    public sealed class FinalCelebrationParticles : MonoBehaviour
    {
        public const float Duration = 2.4f;
        public const float PixelsPerUnit = 400f;
        public const int TotalMaxParticles = 28; // updated: 14 stars (7+7) + 8 confetti (4+4) + 6 serpentinas (2+2+2) = 28 total, avoids burst+rate > max popping
        private static readonly Color[] StarColors =
        {
            new(0.1f, 0.85f, 1f), new(1f, 0.24f, 0.68f), new(1f, 0.55f, 0.12f), new(0.58f, 0.28f, 0.96f)
        };
        private static readonly Color[] ConfettiColors =
        {
            new(0.46f, 0.18f, 0.82f), new(0.62f, 0.3f, 0.96f), new(0.76f, 0.42f, 1f)
        };

        public void Initialize(Sprite fourStar, Sprite fiveStar, Sprite circleConfetti, Sprite rectangularConfetti, Sprite serpentina)
        {
            Initialize(fourStar, fiveStar, circleConfetti, rectangularConfetti, serpentina, null, null);
        }

        public void Initialize(Sprite fourStar, Sprite fiveStar, Sprite circleConfetti, Sprite rectangularConfetti, Sprite serpentina, Sprite serpentina2, Sprite serpentina3)
        {
            RectTransform root = transform as RectTransform;
            CreateGroup(root, "Stars", new[] { fourStar, fiveStar }, new[] { "4Star", "5Star" }, new uint[] { 4103, 5851 }, 3f, 7, StarColors, false);
            CreateGroup(root, "ConfettiStreamers", new[] { circleConfetti, rectangularConfetti, serpentina, serpentina2, serpentina3 }, new[] { "CircleConfetti", "RectangularConfetti", "Serpentina", "Serpentina2", "Serpentina3" }, new uint[] { 7129, 8641, 9901, 10421, 11833 }, 1.7f, 4, ConfettiColors, true);
        }

        public void StopAndClear()
        {
            foreach (ParticleSystem particleSystem in GetComponentsInChildren<ParticleSystem>(true)) particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Transform stars = transform.Find("Stars");
            Transform confetti = transform.Find("ConfettiStreamers");
            if (stars) stars.gameObject.SetActive(false);
            if (confetti) confetti.gameObject.SetActive(false);
        }

        private static void CreateGroup(RectTransform parent, string groupName, Sprite[] sprites, string[] names, uint[] seeds, float ratePerSystem, int maxParticlesPerSystem, Color[] palette, bool isConfetti)
        {
            GameObject groupObject = new(groupName, typeof(RectTransform));
            RectTransform group = groupObject.GetComponent<RectTransform>();
            group.SetParent(parent, false);
            UiFactory.Stretch(group, 0);
            for (int i = 0; i < sprites.Length; i++)
            {
                if (!sprites[i])
                {
                    Sprite fallback = null;
                    bool isSerp = names[i].StartsWith("Serpentina");
                    if (isSerp && sprites.Length > 2 && sprites[2]) fallback = sprites[2];
                    if (!fallback && sprites[0]) fallback = sprites[0];
                    if (fallback) sprites[i] = fallback;
                    else continue;
                }
                GameObject particleObject = new(names[i], typeof(RectTransform), typeof(ParticleSystem));
                RectTransform rect = particleObject.GetComponent<RectTransform>();
                rect.SetParent(group, false);
                UiFactory.Stretch(rect, 0);
                ParticleSystem particleSystem = particleObject.GetComponent<ParticleSystem>();
                bool isSerpentina = names[i].StartsWith("Serpentina");
                int effectiveMax = isSerpentina ? 2 : maxParticlesPerSystem; // updated: serpentinas max 2 each (2+2+2=6) avoids culling, was 6 each (18) stacked stretched
                float effectiveRate = isSerpentina ? 0f : ratePerSystem; // updated: serpentinas rate 0 only burst, avoids burst+rate > max popping, was 1.4f continuous
                Configure(particleSystem, seeds[i], .025f * i, effectiveRate, effectiveMax, isConfetti, isSerpentina, names[i]);
                ParticleSystemRenderer renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
                renderer.enabled = false;
                GameObject bridgeObject = new("UIRenderer", typeof(RectTransform), typeof(CanvasRenderer), typeof(FinalCelebrationUIParticleRenderer));
                RectTransform bridgeRect = bridgeObject.GetComponent<RectTransform>();
                bridgeRect.SetParent(rect, false);
                UiFactory.Stretch(bridgeRect, 0);
                bridgeObject.GetComponent<FinalCelebrationUIParticleRenderer>().Initialize(particleSystem, sprites[i], palette, PixelsPerUnit);
                particleSystem.Play(true);
            }
        }

        private static void Configure(ParticleSystem particleSystem, uint seed, float burstTime, float ratePerSystem, int maxParticles, bool isConfetti, bool isSerpentina, string systemName)
        {
            particleSystem.useAutoRandomSeed = false;
            particleSystem.randomSeed = seed;
            ParticleSystem.MainModule main = particleSystem.main;
            main.duration = Duration;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.useUnscaledTime = true;
            main.maxParticles = maxParticles;
            main.startSpeed = 0f;
            // updated: per-system size/lifetime/gravity to support star pause+fall and large serpentinas
            if (systemName == "4Star")
            {
                main.startLifetime = new ParticleSystem.MinMaxCurve(2.8f, 3.2f); // updated: longer lifetime to allow 0.2s pause at top + slow fall to bottom before disappearing
                main.startSize = new ParticleSystem.MinMaxCurve(.12f, .18f); // updated: 4Star small variant, was .14-.22 uniform
                main.gravityModifier = 0.30f; // updated: stars now fall with gravity 0.30 slow, was 0f static
            }
            else if (systemName == "5Star")
            {
                main.startLifetime = new ParticleSystem.MinMaxCurve(2.8f, 3.2f); // updated: longer lifetime for pause+fall
                main.startSize = new ParticleSystem.MinMaxCurve(.20f, .28f); // updated: 5Star large variant, was .14-.22 uniform
                main.gravityModifier = 0.30f; // updated: was 0f
            }
            else if (isSerpentina)
            {
                main.startLifetime = new ParticleSystem.MinMaxCurve(2.4f, 2.8f);
                main.startSize = new ParticleSystem.MinMaxCurve(.38f, .50f); // updated: serpentinas large .38-.50, was .38-.45
                main.gravityModifier = 0.45f;
            }
            else
            {
                main.startLifetime = new ParticleSystem.MinMaxCurve(2.4f, 2.8f);
                main.startSize = new ParticleSystem.MinMaxCurve(.14f, .20f);
                main.gravityModifier = 0.45f;
            }

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = ratePerSystem;
            // updated: serpentinas total 5 large particles via 2+2+1 bursts, was 5 each (15) stacked stretched
            short burstCount;
            if (systemName == "Serpentina") burstCount = 2; // updated: was 5 shared for all serpentinas
            else if (systemName == "Serpentina2") burstCount = 2; // updated: was 5
            else if (systemName == "Serpentina3") burstCount = 1; // updated: was 5, total 2+2+1=5 large serpentinas
            else burstCount = 2;
            emission.SetBursts(new[] { new ParticleSystem.Burst(burstTime, burstCount) });

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Rectangle;
            shape.scale = new Vector3(.06f, .01f, .01f);
            shape.position = new Vector3(0f, -1.45f, 0f);

            ParticleSystem.VelocityOverLifetimeModule velocity = particleSystem.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            // updated: star trajectory rise -> 0.2s static pause at top -> slow fall to bottom
            AnimationCurve outwardLeft = new(new Keyframe(0f, 0f), new Keyframe(0.32f, 1.5f), new Keyframe(0.55f, -6.6f), new Keyframe(1f, -9.0f));
            AnimationCurve outwardRight = new(new Keyframe(0f, 0f), new Keyframe(0.32f, -1.5f), new Keyframe(0.55f, 6.6f), new Keyframe(1f, 9.0f));
            AnimationCurve starYSlow = new(new Keyframe(0f, 4.5f), new Keyframe(0.35f, 0f), new Keyframe(0.50f, 0f), new Keyframe(1f, -2.8f)); // updated: plateau 0.35-0.50 = 0.2s pause, then slow fall -2.8
            AnimationCurve starYFast = new(new Keyframe(0f, 5.2f), new Keyframe(0.35f, 0f), new Keyframe(0.50f, 0f), new Keyframe(1f, -3.2f)); // updated: plateau pause, then fall -3.2
            AnimationCurve upwardSlow = new(new Keyframe(0f, 3.0f), new Keyframe(0.35f, 2.6f), new Keyframe(0.60f, 0.9f), new Keyframe(0.75f, 0.0f), new Keyframe(1f, -1.8f));
            AnimationCurve upwardFast = new(new Keyframe(0f, 3.6f), new Keyframe(0.35f, 3.2f), new Keyframe(0.60f, 1.1f), new Keyframe(0.75f, 0.0f), new Keyframe(1f, -2.0f));
            AnimationCurve zero = new(new Keyframe(0f, 0f), new Keyframe(1f, 0f));
            AnimationCurve serpXMin = new(new Keyframe(0f, 0f), new Keyframe(0.30f, -1.2f), new Keyframe(0.55f, -4.8f), new Keyframe(1f, -7.0f)); // updated: central serpentina dispersion -7
            AnimationCurve serpXMax = new(new Keyframe(0f, 0f), new Keyframe(0.30f, 1.2f), new Keyframe(0.55f, 4.8f), new Keyframe(1f, 7.0f)); // updated: dispersion +7, avoids vertical stacking (was zero)
            bool isLeft = systemName == "4Star" || systemName == "CircleConfetti" || systemName == "Serpentina2";
            bool isRight = systemName == "5Star" || systemName == "RectangularConfetti" || systemName == "Serpentina3";
            bool isCenter = systemName == "Serpentina";
            if (systemName == "4Star" || systemName == "5Star")
            {
                // updated: stars keep outward dispersion but Y now has pause plateau
                if (isLeft) velocity.x = new ParticleSystem.MinMaxCurve(1f, outwardLeft, outwardLeft);
                else if (isRight) velocity.x = new ParticleSystem.MinMaxCurve(1f, outwardRight, outwardRight);
                else velocity.x = new ParticleSystem.MinMaxCurve(1f, zero, zero);
                velocity.y = new ParticleSystem.MinMaxCurve(1f, starYSlow, starYFast); // updated: pause 0.2s at top
            }
            else if (isCenter) velocity.x = new ParticleSystem.MinMaxCurve(1f, serpXMin, serpXMax); // updated: TwoCurves -7 to +7 dispersion, was zero vertical
            else if (isLeft) velocity.x = new ParticleSystem.MinMaxCurve(1f, outwardLeft, outwardLeft);
            else if (isRight) velocity.x = new ParticleSystem.MinMaxCurve(1f, outwardRight, outwardRight);
            else velocity.x = new ParticleSystem.MinMaxCurve(1f, zero, zero);
            if (systemName == "4Star" || systemName == "5Star")
            {
                // y already set for stars
            }
            else if (isCenter) velocity.y = new ParticleSystem.MinMaxCurve(1f, upwardSlow, upwardFast);
            else velocity.y = new ParticleSystem.MinMaxCurve(1f, upwardSlow, upwardFast);
            velocity.z = new ParticleSystem.MinMaxCurve(1f, zero, zero);

            ParticleSystem.RotationOverLifetimeModule rotation = particleSystem.rotationOverLifetime;
            rotation.enabled = isConfetti;
            rotation.z = isSerpentina
                ? new ParticleSystem.MinMaxCurve(-35f * Mathf.Deg2Rad, 35f * Mathf.Deg2Rad) // updated: slower rotation - was ±155/±100 looked weird
                : new ParticleSystem.MinMaxCurve(-25f * Mathf.Deg2Rad, 25f * Mathf.Deg2Rad); // updated: slower rotation - was ±155/±100 looked weird

            ParticleSystem.ColorOverLifetimeModule fade = particleSystem.colorOverLifetime;
            fade.enabled = true;
            Gradient gradient = new();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) }, new[] { new GradientAlphaKey(.9f, 0f), new GradientAlphaKey(.75f, .7f), new GradientAlphaKey(0f, 1f) });
            fade.color = new ParticleSystem.MinMaxGradient(gradient);

            ParticleSystem.SizeOverLifetimeModule shrink = particleSystem.sizeOverLifetime;
            shrink.enabled = true;
            AnimationCurve curve = new(new Keyframe(0f, 1f), new Keyframe(.72f, .85f), new Keyframe(1f, .45f));
            shrink.size = new ParticleSystem.MinMaxCurve(1f, curve);
        }
    }
}
