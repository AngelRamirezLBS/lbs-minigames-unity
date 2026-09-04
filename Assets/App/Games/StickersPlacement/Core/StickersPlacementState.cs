using System.Collections.Generic;

namespace Lbs.MiniGames.Games.StickersPlacement
{
    public enum StickersPlacementPhase
    {
        Ready,
        Resolving,
        Celebrating,
        Final,
    }

    /// <summary>
    /// Pure state for the deferred-verification drag game. Drops never fail here;
    /// correctness is only evaluated when the confirm button is pressed.
    /// Slot2 (purple) counts as filled from the start.
    /// </summary>
    public sealed class StickersPlacementState
    {
        private readonly Dictionary<string, string> placements = new();

        public StickersPlacementPhase Phase { get; private set; } = StickersPlacementPhase.Ready;
        public bool HasMistake { get; private set; }
        public int Score => HasMistake ? 4 : 8;
        public int StarCount => HasMistake ? 1 : 2;
        public bool IsFinalInputEnabled { get; private set; }

        public void Place(string tokenId, string slotId)
        {
            if (Phase != StickersPlacementPhase.Ready || tokenId == null || slotId == null) return;
            placements[tokenId] = slotId;
        }

        public void Remove(string tokenId)
        {
            if (Phase != StickersPlacementPhase.Ready || tokenId == null) return;
            placements.Remove(tokenId);
        }

        public bool IsPlaced(string tokenId) => tokenId != null && placements.ContainsKey(tokenId);

        public string SlotOf(string tokenId) =>
            tokenId != null && placements.TryGetValue(tokenId, out string slot) ? slot : null;

        public string OccupantOf(string slotId)
        {
            foreach (KeyValuePair<string, string> kv in placements)
                if (kv.Value == slotId) return kv.Key;
            return null;
        }

        public bool AllFilled => placements.Count >= 3;

        public void ClearPlacements() => placements.Clear();

        public bool Confirm()
        {
            if (Phase != StickersPlacementPhase.Ready || !AllFilled) return false;
            Phase = StickersPlacementPhase.Resolving;
            return true;
        }

        public bool ResolveConfirm()
        {
            if (Phase != StickersPlacementPhase.Resolving) return false;
            foreach (KeyValuePair<string, string> kv in placements)
                if (!StickersPlacementRule.IsCorrectPlacement(kv.Key, kv.Value)) return false;
            return true;
        }

        public void FinishCorrect()
        {
            Phase = StickersPlacementPhase.Celebrating;
        }

        public void FinishCelebration()
        {
            if (Phase == StickersPlacementPhase.Celebrating) Phase = StickersPlacementPhase.Final;
        }

        public void FinishIncorrect()
        {
            HasMistake = true;
            if (Phase == StickersPlacementPhase.Resolving) Phase = StickersPlacementPhase.Ready;
        }

        public void EnableFinalInput() => IsFinalInputEnabled = true;
        public bool AcceptFinalInput() => Phase == StickersPlacementPhase.Final && IsFinalInputEnabled;
    }
}
