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

        public void Add(GameCategory category, GameDefinition game)
        {
            if (category != null && !categories.Contains(category)) categories.Add(category);
            if (game != null && !games.Contains(game)) games.Add(game);
        }
    }
}
