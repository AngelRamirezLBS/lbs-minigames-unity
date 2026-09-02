using System.Collections.Generic;

namespace Lbs.MiniGames.Games.FunnyFaceDrag
{
    public enum FunnyFacePhase { Ready, ResolvingIncorrect, Celebrating, Final }

    /// <summary>
    /// Runtime-owned progress for a two-correspondence drag level with one distractor.
    /// </summary>
    public sealed class FunnyFaceState
    {
        private readonly HashSet<string> acceptedPieces = new();

        public FunnyFacePhase Phase { get; private set; } = FunnyFacePhase.Ready;
        public bool HadError { get; private set; }
        public bool FinalInputEnabled { get; private set; }
        public int AcceptedCount => acceptedPieces.Count;
        public int Score => HadError ? 4 : 8;
        public int StarCount => HadError ? 1 : 2;

        public FunnyFaceDropOutcome Drop(string pieceId, string slotId, bool overlapsSlot)
        {
            if (Phase != FunnyFacePhase.Ready || acceptedPieces.Contains(pieceId)) return FunnyFaceDropOutcome.Outside;

            FunnyFaceDropOutcome outcome = FunnyFaceRule.Evaluate(pieceId, slotId, overlapsSlot);
            if (outcome == FunnyFaceDropOutcome.Incorrect)
            {
                HadError = true;
                Phase = FunnyFacePhase.ResolvingIncorrect;
            }
            else if (outcome == FunnyFaceDropOutcome.Correct)
            {
                acceptedPieces.Add(pieceId);
                if (acceptedPieces.Count == 2) Phase = FunnyFacePhase.Celebrating;
            }

            return outcome;
        }

        public void FinishIncorrect()
        {
            if (Phase == FunnyFacePhase.ResolvingIncorrect) Phase = FunnyFacePhase.Ready;
        }

        public void FinishCelebration()
        {
            if (Phase != FunnyFacePhase.Celebrating) return;
            Phase = FunnyFacePhase.Final;
            FinalInputEnabled = false;
        }

        public void EnableFinalInput()
        {
            if (Phase == FunnyFacePhase.Final) FinalInputEnabled = true;
        }

        public bool AcceptFinalInput() => Phase == FunnyFacePhase.Final && FinalInputEnabled;
    }
}
