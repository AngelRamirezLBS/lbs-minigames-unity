using System.Collections.Generic;

namespace Lbs.MiniGames.Games.StickersPlacement
{
    public static class StickersPlacementRule
    {
        public const string YellowSticker = "yellow";
        public const string PinkSticker = "pink";
        public const string BlueSticker = "blue";

        public const string Slot1 = "slot1";
        public const string Slot3 = "slot3";
        public const string Slot4 = "slot4";

        // Purple is pre-placed on slot2; only these three are draggable.
        private static readonly Dictionary<string, string> CorrectSlots = new()
        {
            { YellowSticker, Slot4 },
            { PinkSticker, Slot3 },
            { BlueSticker, Slot1 },
        };

        public static string CorrectSlotFor(string tokenId)
        {
            return tokenId != null && CorrectSlots.TryGetValue(tokenId, out string slot) ? slot : null;
        }

        public static bool IsCorrectPlacement(string tokenId, string slotId)
        {
            return slotId != null && slotId == CorrectSlotFor(tokenId);
        }
    }
}
