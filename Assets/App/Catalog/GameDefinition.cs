using UnityEngine;

namespace Lbs.MiniGames.Catalog
{
    [CreateAssetMenu(menuName = "LBS Mini Games/Catalog/Game Definition", fileName = "GameDefinition")]
    public sealed class GameDefinition : ScriptableObject
    {
        [SerializeField] private string gameId;
        [SerializeField] private string visibleName;
        [SerializeField] private GameCategory category;
        [SerializeField] private Sprite thumbnail;
        [SerializeField] private string sceneName;
        [SerializeField, TextArea] private string description;

        public string GameId => gameId;
        public string VisibleName => visibleName;
        public GameCategory Category => category;
        public Sprite Thumbnail => thumbnail;
        public string SceneName => sceneName;
        public string Description => description;

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(gameId)
                && !string.IsNullOrWhiteSpace(visibleName)
                && category != null
                && !string.IsNullOrWhiteSpace(sceneName);
        }

        public void Configure(
            string id,
            string name,
            GameCategory gameCategory,
            string targetSceneName,
            string gameDescription)
        {
            gameId = id;
            visibleName = name;
            category = gameCategory;
            sceneName = targetSceneName;
            description = gameDescription;
        }

        public void SetThumbnail(Sprite sprite)
        {
            thumbnail = sprite;
        }
    }
}
