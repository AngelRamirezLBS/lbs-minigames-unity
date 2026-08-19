namespace Lbs.MiniGames.Games.Classification
{
    public sealed class ClassificationRound
    {
        private readonly string expectedClassification;

        public ClassificationRound(string expectedClassification)
        {
            this.expectedClassification = expectedClassification;
        }

        public int Attempts { get; private set; }
        public bool IsCompleted { get; private set; }

        public bool TryClassify(string classification)
        {
            if (IsCompleted)
            {
                return false;
            }

            Attempts++;
            IsCompleted = classification == expectedClassification;
            return IsCompleted;
        }
    }
}
