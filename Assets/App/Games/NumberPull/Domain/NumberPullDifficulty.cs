namespace Lbs.MiniGames.Games.NumberPull.Domain
{
    public enum NumberPullDifficultyTier
    {
        LowerPrimary,
        UpperPrimaryAndSecondary,
        PreparatoryHighSchool
    }

    public readonly struct NumberPullDifficulty
    {
        public NumberPullDifficulty(
            NumberPullDifficultyTier tier,
            int targetPulls,
            float durationSeconds,
            int maximumAnswerMagnitude)
        {
            Tier = tier;
            TargetPulls = targetPulls;
            DurationSeconds = durationSeconds;
            MaximumAnswerMagnitude = maximumAnswerMagnitude;
        }

        public NumberPullDifficultyTier Tier { get; }
        public int TargetPulls { get; }
        public float DurationSeconds { get; }
        public int MaximumAnswerMagnitude { get; }

        public static NumberPullDifficulty For(NumberPullDifficultyTier tier)
        {
            return tier switch
            {
                NumberPullDifficultyTier.LowerPrimary => new NumberPullDifficulty(tier, 4, 105f, 20),
                NumberPullDifficultyTier.UpperPrimaryAndSecondary => new NumberPullDifficulty(tier, 5, 90f, 100),
                NumberPullDifficultyTier.PreparatoryHighSchool => new NumberPullDifficulty(tier, 6, 75f, 144),
                _ => throw new System.ArgumentOutOfRangeException(nameof(tier))
            };
        }
    }
}
