namespace Lbs.MiniGames.Games.NumberPull.Domain
{
    public enum MathOperation
    {
        Addition,
        Subtraction
    }

    public readonly struct MathProblem
    {
        public MathProblem(int leftOperand, int rightOperand, MathOperation operation)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
            Operation = operation;
            Answer = operation == MathOperation.Addition
                ? leftOperand + rightOperand
                : leftOperand - rightOperand;
        }

        public int LeftOperand { get; }
        public int RightOperand { get; }
        public MathOperation Operation { get; }
        public int Answer { get; }

        public string Format()
        {
            return Operation == MathOperation.Addition
                ? $"{LeftOperand} + {RightOperand}"
                : $"{LeftOperand} − {RightOperand}";
        }
    }
}
