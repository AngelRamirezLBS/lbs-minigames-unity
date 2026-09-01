using System.Collections.Generic;

namespace Lbs.MiniGames.Games.AnimalDrag
{
    public enum AnimalDragPhase { Ready, ResolvingIncorrect, Celebrating, Final }

    /// <summary>
    /// Runtime-owned progress for a two-correspondence drag level.
    /// </summary>
    public sealed class AnimalDragState
    {
        private readonly HashSet<string> acceptedPieces = new();

        public AnimalDragPhase Phase { get; private set; } = AnimalDragPhase.Ready;
        public bool HadError { get; private set; }
        public bool FinalInputEnabled { get; private set; }
        public int AcceptedCount => acceptedPieces.Count;
        public int Score => HadError ? 4 : 8;
        public int StarCount => HadError ? 1 : 2;

        public AnimalDragDropOutcome Drop(string pieceId, string slotId, bool overlapsSlot)
        {
            if (Phase != AnimalDragPhase.Ready || acceptedPieces.Contains(pieceId)) return AnimalDragDropOutcome.Outside;

            AnimalDragDropOutcome outcome = AnimalDragRule.Evaluate(pieceId, slotId, overlapsSlot);
            if (outcome == AnimalDragDropOutcome.Incorrect)
            {
                HadError = true;
                Phase = AnimalDragPhase.ResolvingIncorrect;
            }
            else if (outcome == AnimalDragDropOutcome.Correct)
            {
                acceptedPieces.Add(pieceId);
                if (acceptedPieces.Count == 2) Phase = AnimalDragPhase.Celebrating;
            }

            return outcome;
        }

        public void FinishIncorrect()
        {
            if (Phase == AnimalDragPhase.ResolvingIncorrect) Phase = AnimalDragPhase.Ready;
        }

        public void FinishCelebration()
        {
            if (Phase != AnimalDragPhase.Celebrating) return;
            Phase = AnimalDragPhase.Final;
            FinalInputEnabled = false;
        }

        public void EnableFinalInput()
        {
            if (Phase == AnimalDragPhase.Final) FinalInputEnabled = true;
        }

        public bool AcceptFinalInput() => Phase == AnimalDragPhase.Final && FinalInputEnabled;
    }
}
