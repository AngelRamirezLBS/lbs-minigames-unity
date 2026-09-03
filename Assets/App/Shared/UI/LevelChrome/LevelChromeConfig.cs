using UnityEngine;

namespace Lbs.MiniGames.Shared.UI
{
    /// <summary>
    /// Immutable configuration for reusable Exit + Hong chrome. Authored as ScriptableObject; runtime never mutates it.
    /// Defaults match approved coordinates; validation ensures consistency.
    /// </summary>
    [CreateAssetMenu(menuName = "LBS Mini Games/UI/Level Chrome Config", fileName = "LevelChromeConfig")]
    public sealed class LevelChromeConfig : ScriptableObject
    {
        [SerializeField] private Vector2 exitCenter = new(145f, 150f);
        [SerializeField] private Vector2 exitSize = new(170f, 170f);
        [SerializeField] private Vector2 hongCenter = new(145f, 930f);
        [SerializeField] private Vector2 hongSize = new(220f, 220f);
        [SerializeField, Range(0f, 40f)] private float hongCornerRadius = 28f;

        public Vector2 ExitCenter => exitCenter;
        public Vector2 ExitSize => exitSize;
        public Vector2 HongCenter => hongCenter;
        public Vector2 HongSize => hongSize;
        public float HongCornerRadius => hongCornerRadius;

        public bool IsValid()
        {
            return exitSize.x > 0 && exitSize.y > 0 && hongSize.x > 0 && hongSize.y > 0;
        }

        public static LevelChromeConfig CreateDefault()
        {
            var cfg = CreateInstance<LevelChromeConfig>();
            cfg.exitCenter = LevelChromeLayout.ExitCenter;
            cfg.exitSize = LevelChromeLayout.ExitSize;
            cfg.hongCenter = LevelChromeLayout.HongCenter;
            cfg.hongSize = LevelChromeLayout.HongSize;
            cfg.hongCornerRadius = 28f;
            return cfg;
        }

#if UNITY_EDITOR
        public void Configure(Vector2 exitCtr, Vector2 exitSz, Vector2 hongCtr, Vector2 hongSz)
        {
            exitCenter = exitCtr;
            exitSize = exitSz;
            hongCenter = hongCtr;
            hongSize = hongSz;
        }
#endif
    }
}
