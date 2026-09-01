using System.Collections.Generic;

namespace Lbs.MiniGames.Games.MakeAnEmojiDrag
{
    public enum MakeAnEmojiDragPhase { Ready, ResolvingIncorrect, Celebrating, Final }

    /// <summary>
    /// Runtime-owned progress for an any-order, three-correspondence drag level.
    /// </summary>
    public sealed class MakeAnEmojiDragState
    {
        private readonly HashSet<string> acceptedPieces = new();

        public MakeAnEmojiDragPhase Phase { get; private set; } = MakeAnEmojiDragPhase.Ready;
        public bool HadError { get; private set; }
        public bool FinalInputEnabled { get; private set; }
        public int AcceptedCount => acceptedPieces.Count;
        public int Score => HadError ? 4 : 8;
        public int StarCount => HadError ? 1 : 2;

        public MakeAnEmojiDragDropOutcome Drop(string pieceId, string slotId, bool overlapsSlot)
        {
            if (Phase != MakeAnEmojiDragPhase.Ready || acceptedPieces.Contains(pieceId)) return MakeAnEmojiDragDropOutcome.Outside;

            MakeAnEmojiDragDropOutcome outcome = MakeAnEmojiDragRule.Evaluate(pieceId, slotId, overlapsSlot);
            if (outcome == MakeAnEmojiDragDropOutcome.Incorrect)
            {
                HadError = true;
                Phase = MakeAnEmojiDragPhase.ResolvingIncorrect;
            }
            else if (outcome == MakeAnEmojiDragDropOutcome.Correct)
            {
                acceptedPieces.Add(pieceId);
                if (acceptedPieces.Count == 3) Phase = MakeAnEmojiDragPhase.Celebrating;
            }

            return outcome;
        }

        public void FinishIncorrect()
        {
            if (Phase == MakeAnEmojiDragPhase.ResolvingIncorrect) Phase = MakeAnEmojiDragPhase.Ready;
        }

        public void FinishCelebration()
        {
            if (Phase != MakeAnEmojiDragPhase.Celebrating) return;
            Phase = MakeAnEmojiDragPhase.Final;
            FinalInputEnabled = false;
        }

        public void EnableFinalInput()
        {
            if (Phase == MakeAnEmojiDragPhase.Final) FinalInputEnabled = true;
        }

        public bool AcceptFinalInput() => Phase == MakeAnEmojiDragPhase.Final && FinalInputEnabled;
    }
}
