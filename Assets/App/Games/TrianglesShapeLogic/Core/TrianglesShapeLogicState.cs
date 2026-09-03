using System.Collections.Generic;

namespace Lbs.MiniGames.Games.TrianglesShapeLogic
{
    public enum TrianglesShapeLogicPhase { Ready, ResolvingIncorrect, Celebrating, Final }

    /// <summary>
    /// Runtime-owned progress for a two-correspondence drag level.
    /// </summary>
    public sealed class TrianglesShapeLogicState
    {
        private readonly HashSet<string> acceptedPieces = new();

        public TrianglesShapeLogicPhase Phase { get; private set; } = TrianglesShapeLogicPhase.Ready;
        public bool HadError { get; private set; }
        public bool FinalInputEnabled { get; private set; }
        public int AcceptedCount => acceptedPieces.Count;
        public int Score => HadError ? 4 : 8;
        public int StarCount => HadError ? 1 : 2;

        public TrianglesShapeLogicDropOutcome Drop(string pieceId, string slotId, bool overlapsSlot)
        {
            if (Phase != TrianglesShapeLogicPhase.Ready || acceptedPieces.Contains(pieceId)) return TrianglesShapeLogicDropOutcome.Outside;

            TrianglesShapeLogicDropOutcome outcome = TrianglesShapeLogicRule.Evaluate(pieceId, slotId, overlapsSlot);
            if (outcome == TrianglesShapeLogicDropOutcome.Incorrect)
            {
                HadError = true;
                Phase = TrianglesShapeLogicPhase.ResolvingIncorrect;
            }
            else if (outcome == TrianglesShapeLogicDropOutcome.Correct)
            {
                acceptedPieces.Add(pieceId);
                if (acceptedPieces.Count == 2) Phase = TrianglesShapeLogicPhase.Celebrating;
            }

            return outcome;
        }

        public void FinishIncorrect()
        {
            if (Phase == TrianglesShapeLogicPhase.ResolvingIncorrect) Phase = TrianglesShapeLogicPhase.Ready;
        }

        public void FinishCelebration()
        {
            if (Phase != TrianglesShapeLogicPhase.Celebrating) return;
            Phase = TrianglesShapeLogicPhase.Final;
            FinalInputEnabled = false;
        }

        public void EnableFinalInput()
        {
            if (Phase == TrianglesShapeLogicPhase.Final) FinalInputEnabled = true;
        }

        public bool AcceptFinalInput() => Phase == TrianglesShapeLogicPhase.Final && FinalInputEnabled;
    }
}
