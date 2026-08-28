using System.Collections.Generic;
using UnityEngine;

namespace Lbs.MiniGames.Catalog
{
    [CreateAssetMenu(menuName = "LBS Mini Games/Catalog/Game Catalog", fileName = "GameCatalog")]
    public sealed class GameCatalog : ScriptableObject
    {
        [SerializeField] private List<GameCategory> categories = new();
        [SerializeField] private List<GameDefinition> games = new();

        public IReadOnlyList<GameCategory> Categories => categories;

        public IEnumerable<GameDefinition> GetGames(GameCategory category)
        {
            foreach (GameDefinition game in games)
            {
                if (game != null && game.Category == category)
                {
                    yield return game;
                }
            }
        }

        public void Configure(List<GameCategory> catalogCategories, List<GameDefinition> catalogGames)
        {
            categories = catalogCategories;
            games = catalogGames;
        }

        public void EnsureCategory(GameCategory category)
        {
            if (category == null || string.IsNullOrWhiteSpace(category.CategoryId))
            {
                return;
            }

            if (categories == null)
            {
                categories = new List<GameCategory>();
            }

            foreach (GameCategory existing in categories)
            {
                if (existing != null && string.Equals(existing.CategoryId, category.CategoryId, System.StringComparison.Ordinal))
                {
                    return;
                }
            }

            categories.Add(category);
        }

        public void EnsureGame(GameDefinition game)
        {
            if (game == null || string.IsNullOrWhiteSpace(game.GameId))
            {
                return;
            }

            if (games == null)
            {
                games = new List<GameDefinition>();
            }

            foreach (GameDefinition existing in games)
            {
                if (existing != null && string.Equals(existing.GameId, game.GameId, System.StringComparison.Ordinal))
                {
                    return;
                }
            }

            games.Add(game);
        }
    }
}
