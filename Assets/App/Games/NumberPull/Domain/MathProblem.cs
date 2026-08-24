namespace Lbs.MiniGames.Games.NumberPull.Domain
{
    public enum MathOperation
    {
        Addition,
        Subtraction,
        Multiplication,
        Division
    }

    public readonly struct MathProblem
    {
        public MathProblem(int leftOperand, int rightOperand, MathOperation operation)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
            Operation = operation;
            Answer = operation switch
            {
                MathOperation.Addition => leftOperand + rightOperand,
                MathOperation.Subtraction => leftOperand - rightOperand,
                MathOperation.Multiplication => leftOperand * rightOperand,
                MathOperation.Division => leftOperand / rightOperand,
                _ => throw new System.ArgumentOutOfRangeException(nameof(operation))
            };
        }

        public int LeftOperand { get; }
        public int RightOperand { get; }
        public MathOperation Operation { get; }
        public int Answer { get; }

        public string Format()
        {
            string symbol = Operation switch
            {
                MathOperation.Addition => "+",
                MathOperation.Subtraction => "−",
                MathOperation.Multiplication => "×",
                MathOperation.Division => "÷",
                _ => throw new System.ArgumentOutOfRangeException()
            };
            return $"{FormatOperand(LeftOperand)} {symbol} {FormatOperand(RightOperand)}";
        }

        private static string FormatOperand(int value)
        {
            return value < 0 ? $"({value})" : value.ToString();
        }
    }
}
