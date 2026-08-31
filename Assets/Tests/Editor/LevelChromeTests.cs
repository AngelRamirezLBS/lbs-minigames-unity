using NUnit.Framework;
using Lbs.MiniGames.Shared.UI;
using UnityEngine;

namespace Lbs.MiniGames.Tests
{
    public sealed class LevelChromeTests
    {
        [Test]
        public void Layout_Centralizes_Approved_Coordinates()
        {
            Assert.AreEqual(new Vector2(145f, 150f), LevelChromeLayout.ExitCenter);
            Assert.AreEqual(new Vector2(170f, 170f), LevelChromeLayout.ExitSize);
            Assert.AreEqual(new Vector2(145f, 930f), LevelChromeLayout.HongCenter);
            Assert.AreEqual(new Vector2(220f, 220f), LevelChromeLayout.HongSize);
            Assert.AreEqual(new Vector2(1920f, 1080f), LevelChromeLayout.ReferenceResolution);
            Assert.AreEqual(0.5f, LevelChromeLayout.ReferenceMatch);
        }

        [Test]
        public void ToAnchoredPosition_Maps_TopOrigin_To_CenterPivot()
        {
            // Top-left (0,0) at 1920x1080 should map to (-960,540)
            Vector2 topLeft = LevelChromeLayout.ToAnchoredPosition(new Vector2(0f, 0f));
            Assert.AreEqual(new Vector2(-960f, 540f), topLeft);
            // Center (960,540) should map to (0,0)
            Vector2 center = LevelChromeLayout.ToAnchoredPosition(new Vector2(960f, 540f));
            Assert.AreEqual(Vector2.zero, center);
            // The centralized Hong center maps relative to the reference canvas center.
            Vector2 hong = LevelChromeLayout.ToAnchoredPosition(LevelChromeLayout.HongCenter);
            Assert.AreEqual(
                new Vector2(
                    LevelChromeLayout.HongCenter.x - LevelChromeLayout.ReferenceResolution.x * 0.5f,
                    LevelChromeLayout.ReferenceResolution.y * 0.5f - LevelChromeLayout.HongCenter.y),
                hong);
        }

        [Test]
        public void LevelChromeConfig_Default_Matches_Layout()
        {
            var cfg = LevelChromeConfig.CreateDefault();
            Assert.IsTrue(cfg.IsValid());
            Assert.AreEqual(LevelChromeLayout.ExitCenter, cfg.ExitCenter);
            Assert.AreEqual(LevelChromeLayout.ExitSize, cfg.ExitSize);
            Assert.AreEqual(LevelChromeLayout.HongCenter, cfg.HongCenter);
            Assert.AreEqual(LevelChromeLayout.HongSize, cfg.HongSize);
            Object.DestroyImmediate(cfg);
        }
    }
}
