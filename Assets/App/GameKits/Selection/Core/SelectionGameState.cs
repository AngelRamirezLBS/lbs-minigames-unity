namespace Lbs.MiniGames.GameKits.Selection
{
    public enum SelectionPhase { Ready, ResolvingIncorrect, Celebrating, Final }

    public sealed class SelectionGameState
    {
        public SelectionPhase Phase { get; private set; } = SelectionPhase.Ready;
        public bool HasMistake { get; private set; }
        public int Score => HasMistake ? 4 : 8;
        public int StarCount => HasMistake ? 1 : 2;
        public bool IsFinalInputEnabled { get; private set; }

        public bool Select(string answerId, string correctAnswer)
        {
            if (Phase != SelectionPhase.Ready) return false;
            if (answerId == correctAnswer) { Phase = SelectionPhase.Celebrating; return true; }
            HasMistake = true; Phase = SelectionPhase.ResolvingIncorrect; return false;
        }
        public void FinishIncorrect() { if (Phase == SelectionPhase.ResolvingIncorrect) Phase = SelectionPhase.Ready; }
        public void FinishCelebration()
        {
            if (Phase != SelectionPhase.Celebrating) return;
            Phase = SelectionPhase.Final;
            IsFinalInputEnabled = false;
        }

        public void EnableFinalInput()
        {
            if (Phase == SelectionPhase.Final) IsFinalInputEnabled = true;
        }

        public bool AcceptFinalInput() => Phase == SelectionPhase.Final && IsFinalInputEnabled;
    }
}
