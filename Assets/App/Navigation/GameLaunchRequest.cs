using System;
using Lbs.MiniGames.Catalog;

namespace Lbs.MiniGames.Navigation
{
    /// <summary>
    /// Immutable launch request containing game and optional difficulty. Difficulty may be null only for legacy fallback.
    /// </summary>
    public readonly struct GameLaunchRequest : IEquatable<GameLaunchRequest>
    {
        public GameLaunchRequest(GameDefinition game, DifficultyDefinition difficulty)
        {
            Game = game;
            Difficulty = difficulty;
        }

        public GameDefinition Game { get; }
        public DifficultyDefinition Difficulty { get; }
        public string DifficultyId => Difficulty != null ? Difficulty.DifficultyId : null;

        public bool IsValid
        {
            get
            {
                if (Game == null || !Game.IsValid()) return false;
                if (Difficulty == null) return true; // legacy fallback allowed
                if (!Difficulty.IsValid()) return false;
                // Reject difficulty not supported when game declares supported list
                if (Game.SupportedDifficulties != null && Game.SupportedDifficulties.Count > 0 && !Game.SupportsDifficulty(Difficulty))
                {
                    return false;
                }
                return true;
            }
        }

        public bool Equals(GameLaunchRequest other)
        {
            return Equals(Game, other.Game) && Equals(Difficulty, other.Difficulty);
        }

        public override bool Equals(object obj)
        {
            return obj is GameLaunchRequest other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Game != null ? Game.GetHashCode() : 0) * 397) ^ (Difficulty != null ? Difficulty.GetHashCode() : 0);
            }
        }

        public static bool operator ==(GameLaunchRequest left, GameLaunchRequest right) => left.Equals(right);
        public static bool operator !=(GameLaunchRequest left, GameLaunchRequest right) => !left.Equals(right);

        public override string ToString()
        {
            return $"Game:{Game?.GameId ?? "null"} Difficulty:{DifficultyId ?? "legacy"}";
        }
    }
}
