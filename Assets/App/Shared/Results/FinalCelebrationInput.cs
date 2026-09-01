using UnityEngine;

namespace Lbs.MiniGames.Shared.Results
{
    public readonly struct FinalCelebrationInput
    {
        public FinalCelebrationInput(
            int score,
            int starCount,
            Font scoreFont,
            Sprite finalStar,
            Sprite fourStar,
            Sprite fiveStar,
            Sprite circleConfetti,
            Sprite rectangularConfetti,
            Sprite serpentina,
            Sprite serpentina2,
            Sprite serpentina3)
        {
            Score = score;
            StarCount = starCount;
            ScoreFont = scoreFont;
            FinalStar = finalStar;
            FourStar = fourStar;
            FiveStar = fiveStar;
            CircleConfetti = circleConfetti;
            RectangularConfetti = rectangularConfetti;
            Serpentina = serpentina;
            Serpentina2 = serpentina2;
            Serpentina3 = serpentina3;
        }

        public int Score { get; }
        public int StarCount { get; }
        public Font ScoreFont { get; }
        public Sprite FinalStar { get; }
        public Sprite FourStar { get; }
        public Sprite FiveStar { get; }
        public Sprite CircleConfetti { get; }
        public Sprite RectangularConfetti { get; }
        public Sprite Serpentina { get; }
        public Sprite Serpentina2 { get; }
        public Sprite Serpentina3 { get; }
    }
}
