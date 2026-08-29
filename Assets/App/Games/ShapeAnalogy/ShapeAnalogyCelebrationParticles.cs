using UnityEngine;
using UnityEngine.UI;
using Lbs.MiniGames.Shared;

namespace Lbs.MiniGames.Games.ShapeAnalogy
{
    public sealed class ShapeAnalogyCelebrationParticles : MonoBehaviour
    {
        public const float Duration = 2.4f;
        public const float PixelsPerUnit = 400f;
        public const int TotalMaxParticles = 40;
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
                int effectiveMax = isSerpentina ? 6 : maxParticlesPerSystem;
                float effectiveRate = isSerpentina ? 1.4f : ratePerSystem;
                Configure(particleSystem, seeds[i], .025f * i, effectiveRate, effectiveMax, isConfetti, isSerpentina, names[i]);
                ParticleSystemRenderer renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
                renderer.enabled = false;
                GameObject bridgeObject = new("UIRenderer", typeof(RectTransform), typeof(CanvasRenderer), typeof(ShapeAnalogyUIParticleRenderer));
                RectTransform bridgeRect = bridgeObject.GetComponent<RectTransform>();
                bridgeRect.SetParent(rect, false);
                UiFactory.Stretch(bridgeRect, 0);
                bridgeObject.GetComponent<ShapeAnalogyUIParticleRenderer>().Initialize(particleSystem, sprites[i], palette, PixelsPerUnit);
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
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.4f, 2.8f);
            main.startSpeed = 0f;
            main.startSize = isSerpentina ? new ParticleSystem.MinMaxCurve(.38f, .45f) : isConfetti ? new ParticleSystem.MinMaxCurve(.14f, .20f) : new ParticleSystem.MinMaxCurve(.14f, .28f);
            main.gravityModifier = isConfetti ? 0.55f : 0.35f;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = ratePerSystem;
            // Ordenada: Stars 7 burst 2; Circle/Rect 4 burst 2; Serpentina/2/3 6 burst 5 → total 40 ordenado
            short burstCount = isSerpentina ? (short)5 : (short)2;
            emission.SetBursts(new[] { new ParticleSystem.Burst(burstTime, burstCount) });

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Rectangle;
            shape.scale = new Vector3(.06f, .01f, .01f);
            shape.position = new Vector3(0f, -1.45f, 0f);

            ParticleSystem.VelocityOverLifetimeModule velocity = particleSystem.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            // Flujo ordenado: X determinística por sistema → sube hasta arriba → pausa 0.2s → cae vertical
            AnimationCurve outwardLeft = new(new Keyframe(0f, 0f), new Keyframe(0.32f, 0.5f), new Keyframe(0.55f, -2.2f), new Keyframe(1f, -3.0f));
            AnimationCurve outwardRight = new(new Keyframe(0f, 0f), new Keyframe(0.32f, -0.5f), new Keyframe(0.55f, 2.2f), new Keyframe(1f, 3.0f));
            AnimationCurve upwardSlow = new(new Keyframe(0f, 3.0f), new Keyframe(0.35f, 2.6f), new Keyframe(0.60f, 0.9f), new Keyframe(0.75f, 0.0f), new Keyframe(1f, -1.8f));
            AnimationCurve upwardFast = new(new Keyframe(0f, 3.6f), new Keyframe(0.35f, 3.2f), new Keyframe(0.60f, 1.1f), new Keyframe(0.75f, 0.0f), new Keyframe(1f, -2.0f));
            AnimationCurve zero = new(new Keyframe(0f, 0f), new Keyframe(1f, 0f));
            // X determinística por sistema: evita caos aleatorio MinMaxCurve(left,right)
            // 4Star/Circle/Serpentina2 → left; 5Star/Rect/Serpentina3 → right; Serpentina central → vertical (0)
            bool isLeft = systemName == "4Star" || systemName == "CircleConfetti" || systemName == "Serpentina2";
            bool isRight = systemName == "5Star" || systemName == "RectangularConfetti" || systemName == "Serpentina3";
            bool isCenter = systemName == "Serpentina";
            if (isCenter) velocity.x = new ParticleSystem.MinMaxCurve(1f, zero);
            else if (isLeft) velocity.x = new ParticleSystem.MinMaxCurve(1f, outwardLeft);
            else if (isRight) velocity.x = new ParticleSystem.MinMaxCurve(1f, outwardRight);
            else velocity.x = new ParticleSystem.MinMaxCurve(1f, zero);
            velocity.y = new ParticleSystem.MinMaxCurve(1f, upwardSlow, upwardFast);
            velocity.z = new ParticleSystem.MinMaxCurve(1f, zero, zero);

            ParticleSystem.RotationOverLifetimeModule rotation = particleSystem.rotationOverLifetime;
            rotation.enabled = isConfetti;
            rotation.z = isSerpentina
                ? new ParticleSystem.MinMaxCurve(-155f * Mathf.Deg2Rad, 155f * Mathf.Deg2Rad)
                : new ParticleSystem.MinMaxCurve(-100f * Mathf.Deg2Rad, 100f * Mathf.Deg2Rad);

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
