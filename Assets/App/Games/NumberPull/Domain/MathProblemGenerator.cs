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

        public MathProblemGenerator(int seed)
        {
            random = new Random(seed);
        }

        public MathProblem Next()
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
    }
}
