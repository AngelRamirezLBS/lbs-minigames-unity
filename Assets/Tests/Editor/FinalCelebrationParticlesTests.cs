using Lbs.MiniGames.Shared.Results;
using NUnit.Framework;
using UnityEngine;

namespace Lbs.MiniGames.Tests
{
    public sealed class FinalCelebrationParticlesTests
    {
        private GameObject root;
        private Sprite sprite;
        private Texture2D texture;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("FinalCelebrationParticlesTestRoot", typeof(RectTransform), typeof(FinalCelebrationParticles));
            texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(.5f, .5f));
        }

        [TearDown]
        public void TearDown()
        {
            if (root != null) Object.DestroyImmediate(root);
            if (sprite != null) Object.DestroyImmediate(sprite);
            if (texture != null) Object.DestroyImmediate(texture);
        }

        [Test]
        public void Initialize_Twice_ReplacesGeneratedParticleGroups()
        {
            FinalCelebrationParticles particles = root.GetComponent<FinalCelebrationParticles>();

            particles.Initialize(sprite, sprite, sprite, sprite, sprite, sprite, sprite);
            particles.Initialize(sprite, sprite, sprite, sprite, sprite, sprite, sprite);

            Assert.AreEqual(1, CountDirectChildrenNamed("Stars"));
            Assert.AreEqual(1, CountDirectChildrenNamed("ConfettiStreamers"));
            Assert.AreEqual(7, root.GetComponentsInChildren<ParticleSystem>(true).Length);
        }

        private int CountDirectChildrenNamed(string name)
        {
            int count = 0;
            for (int index = 0; index < root.transform.childCount; index++)
            {
                if (root.transform.GetChild(index).name == name) count++;
            }
            return count;
        }
    }
}
