using UnityEngine;

namespace Lbs.MiniGames.Catalog
{
    /// <summary>
    /// Immutable difficulty configuration. Authored as ScriptableObject, never mutated at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "LBS Mini Games/Catalog/Difficulty Definition", fileName = "DifficultyDefinition")]
    public sealed class DifficultyDefinition : ScriptableObject
    {
        [SerializeField] private string difficultyId;
        [SerializeField] private string displayName;
        [SerializeField] private int sortOrder;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;

        public string DifficultyId => difficultyId;
        public string DisplayName => displayName;
        public int SortOrder => sortOrder;
        public string Description => description;
        public Sprite Icon => icon;

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(difficultyId)
                   && !string.IsNullOrWhiteSpace(displayName);
        }

#if UNITY_EDITOR
        public void Configure(string id, string name, int order, string desc, Sprite sprite = null)
        {
            difficultyId = id;
            displayName = name;
            sortOrder = order;
            description = desc;
            icon = sprite;
        }
#endif
    }
}
