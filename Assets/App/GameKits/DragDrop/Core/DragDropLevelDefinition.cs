using System.Collections.Generic;
using UnityEngine;
using Lbs.MiniGames.Catalog;

namespace Lbs.MiniGames.GameKits.DragDrop
{
    /// <summary>
    /// Immutable ScriptableObject foundation for drag-drop level content/layout.
    /// Supports difficulty-specific tuning; mutable runtime state is owned elsewhere.
    /// </summary>
    [CreateAssetMenu(menuName = "LBS Mini Games/GameKits/DragDrop Level Definition", fileName = "DragDropLevelDefinition")]
    public sealed class DragDropLevelDefinition : ScriptableObject
    {
        [SerializeField] private string levelId;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private DifficultyDefinition difficulty;
        [SerializeField] private List<DragDropCardDefinition> cards = new();

        public string LevelId => levelId;
        public string DisplayName => displayName;
        public string Description => description;
        public DifficultyDefinition Difficulty => difficulty;
        public IReadOnlyList<DragDropCardDefinition> Cards => cards;

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(levelId)
                   && !string.IsNullOrWhiteSpace(displayName)
                   && cards != null && cards.Count > 0;
        }

#if UNITY_EDITOR
        public void Configure(string id, string name, string desc, DifficultyDefinition diff, List<DragDropCardDefinition> cardDefs)
        {
            levelId = id;
            displayName = name;
            description = desc;
            difficulty = diff;
            cards = cardDefs != null ? new List<DragDropCardDefinition>(cardDefs) : new List<DragDropCardDefinition>();
        }
#endif

        [System.Serializable]
        public sealed class DragDropCardDefinition
        {
            public string tokenId;
            public Sprite sprite;
            public Vector2 boardCenter;
            public Vector2 size;
            public bool isCorrect;
        }
    }
}
