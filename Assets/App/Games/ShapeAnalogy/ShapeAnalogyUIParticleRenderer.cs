using UnityEngine;
using UnityEngine.UI;

namespace Lbs.MiniGames.Games.ShapeAnalogy
{
    public sealed class ShapeAnalogyUIParticleRenderer : MonoBehaviour
    {
        private ParticleSystem particleSystem;
        private Sprite sprite;
        private Color[] palette;
        private float pixelsPerUnit;
        private ParticleSystem.Particle[] particles = new ParticleSystem.Particle[4];
        private Image[] images;

        public Sprite AssignedSprite => sprite;
        public int LastRenderedParticleCount { get; private set; }
        public int ActiveImageCount { get; private set; }

        public void Initialize(ParticleSystem source, Sprite sourceSprite, Color[] colors, float particlePixelsPerUnit)
        {
            particleSystem = source;
            sprite = sourceSprite;
            palette = colors;
            pixelsPerUnit = particlePixelsPerUnit;
            images = new Image[source.main.maxParticles];
            for (int i = 0; i < images.Length; i++)
            {
                GameObject imageObject = new("Particle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                imageObject.transform.SetParent(transform, false);
                Image image = imageObject.GetComponent<Image>();
                image.sprite = sprite;
                image.preserveAspect = true;
                image.raycastTarget = false;
                imageObject.SetActive(false);
                images[i] = image;
            }
        }

        private void LateUpdate() => Refresh();

        public void Refresh()
        {
            LastRenderedParticleCount = 0;
            ActiveImageCount = 0;
            if (!particleSystem || !sprite || palette == null || palette.Length == 0 || images == null) return;
            int count = particleSystem.particleCount;
            if (count > particles.Length) particles = new ParticleSystem.Particle[count];
            count = particleSystem.GetParticles(particles);
            LastRenderedParticleCount = count;
            for (int i = 0; i < images.Length; i++)
            {
                bool visible = i < count;
                images[i].gameObject.SetActive(visible);
                if (!visible) continue;
                ActiveImageCount++;
                ParticleSystem.Particle particle = particles[i];
                RectTransform rect = images[i].rectTransform;
                rect.anchoredPosition = new Vector2(particle.position.x * pixelsPerUnit, particle.position.y * pixelsPerUnit);
                rect.sizeDelta = Vector2.one * particle.GetCurrentSize(particleSystem) * pixelsPerUnit;
                rect.localRotation = Quaternion.Euler(0f, 0f, particle.rotation * Mathf.Rad2Deg);
                Color color = palette[particle.randomSeed % (uint)palette.Length];
                color.a *= Mathf.Clamp01(particle.remainingLifetime / particle.startLifetime);
                images[i].color = color;
            }
        }
    }
}
