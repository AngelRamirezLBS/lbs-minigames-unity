using UnityEngine;

namespace Lbs.MiniGames.Catalog
{
    [CreateAssetMenu(menuName = "LBS Mini Games/Catalog/Game Category", fileName = "GameCategory")]
    public sealed class GameCategory : ScriptableObject
    {
        [SerializeField] private string categoryId;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;

        public string CategoryId => categoryId;
        public string DisplayName => displayName;
        public string Description => description;

        public void Configure(string id, string name, string categoryDescription)
        {
            categoryId = id;
            displayName = name;
            description = categoryDescription;
        }
    }
}
