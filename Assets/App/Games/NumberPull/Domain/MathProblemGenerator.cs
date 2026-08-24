using System;

namespace Lbs.MiniGames.Games.NumberPull.Domain
{
    public interface IMathProblemGenerator
    {
        MathProblem Next();
    }

    public sealed class MathProblemGenerator : IMathProblemGenerator
    {
        public const int MaximumAnswer = 20;

        private readonly Random random;
        private readonly NumberPullDifficultyTier tier;

        public MathProblemGenerator(int seed)
            : this(seed, NumberPullDifficultyTier.LowerPrimary)
        {
        }

        public MathProblemGenerator(int seed, NumberPullDifficultyTier tier)
        {
            random = new Random(seed);
            this.tier = tier;
        }

        public MathProblem Next()
        {
            return tier switch
            {
                NumberPullDifficultyTier.LowerPrimary => NextLowerPrimary(),
                NumberPullDifficultyTier.UpperPrimaryAndSecondary => NextUpperPrimaryAndSecondary(),
                NumberPullDifficultyTier.PreparatoryHighSchool => NextPreparatoryHighSchool(),
                _ => throw new ArgumentOutOfRangeException(nameof(tier))
            };
        }

        private MathProblem NextLowerPrimary()
        {
            if (random.Next(0, 2) == 0)
            {
                int left = random.Next(0, MaximumAnswer + 1);
                int right = random.Next(0, MaximumAnswer - left + 1);
                return new MathProblem(left, right, MathOperation.Addition);
            }

            int minuend = random.Next(0, MaximumAnswer + 1);
            int subtrahend = random.Next(0, minuend + 1);
            return new MathProblem(minuend, subtrahend, MathOperation.Subtraction);
        }

        private MathProblem NextUpperPrimaryAndSecondary()
        {
            switch (random.Next(0, 4))
            {
                case 0:
                {
                    int left = random.Next(0, 101);
                    int right = random.Next(0, 101 - left);
                    return new MathProblem(left, right, MathOperation.Addition);
                }
                case 1:
                {
                    int minuend = random.Next(0, 101);
                    return new MathProblem(minuend, random.Next(0, minuend + 1), MathOperation.Subtraction);
                }
                case 2:
                {
                    int left = random.Next(2, 11);
                    return new MathProblem(left, random.Next(2, 11), MathOperation.Multiplication);
                }
                default:
                {
                    int divisor = random.Next(2, 11);
                    int quotient = random.Next(2, 11);
                    return new MathProblem(divisor * quotient, divisor, MathOperation.Division);
                }
            }
        }

        private MathProblem NextPreparatoryHighSchool()
        {
            switch (random.Next(0, 4))
            {
                case 0:
                    return new MathProblem(random.Next(-20, 21), random.Next(-20, 21), MathOperation.Addition);
                case 1:
                    return new MathProblem(random.Next(-20, 21), random.Next(-20, 21), MathOperation.Subtraction);
                case 2:
                    return new MathProblem(NextSignedFactor(), NextSignedFactor(), MathOperation.Multiplication);
                default:
                {
                    int divisor = NextSignedFactor();
                    int quotient = NextSignedFactor();
                    return new MathProblem(divisor * quotient, divisor, MathOperation.Division);
                }
            }
        }

        private int NextSignedFactor()
        {
            int magnitude = random.Next(2, 13);
            return random.Next(0, 2) == 0 ? magnitude : -magnitude;
        }
    }
}
