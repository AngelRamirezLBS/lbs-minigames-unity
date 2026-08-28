using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lbs.MiniGames.Games.WildWhiz
{
    [CreateAssetMenu(menuName = "LBS Mini Games/Wild Whiz/Level Set", fileName = "WildWhizLevelSet")]
    public sealed class WildWhizLevelSet : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<WildWhizLevel> levels = new();

        public IReadOnlyList<WildWhizLevel> Levels => levels;

        public bool IsValid()
        {
            if (levels == null || levels.Count == 0)
            {
                return false;
            }

            HashSet<string> ids = new(StringComparer.Ordinal);
            foreach (WildWhizLevel level in levels)
            {
                if (!level.IsValid())
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(level.Id) || !ids.Add(level.Id))
                {
                    return false;
                }
            }

            return true;
        }

        public void Configure(IEnumerable<WildWhizLevel> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            levels = new List<WildWhizLevel>(source);
        }

        public void OnBeforeSerialize()
        {
            // Propagate to each level
            if (levels != null)
            {
                for (int i = 0; i < levels.Count; i++)
                {
                    WildWhizLevel l = levels[i];
                    l.OnBeforeSerialize();
                    levels[i] = l;
                }
            }
        }

        public void OnAfterDeserialize()
        {
            if (levels != null)
            {
                for (int i = 0; i < levels.Count; i++)
                {
                    WildWhizLevel l = levels[i];
                    l.OnAfterDeserialize();
                    levels[i] = l;
                }
            }
        }

        public static WildWhizLevelSet CreateDefault()
        {
            WildWhizLevelSet set = CreateInstance<WildWhizLevelSet>();
            set.Configure(BuildDefaultLevels());
            return set;
        }

        public static IReadOnlyList<WildWhizLevel> BuildDefaultLevels()
        {
            WildWhizLevel habitats = new(
                "habit-1",
                "Sort by habitat.",
                new[] { "forest", "ocean" },
                new[]
                {
                    new WildWhizLevel.Item("fox", "fox"),
                    new WildWhizLevel.Item("bear", "bear"),
                    new WildWhizLevel.Item("dolphin", "dolphin"),
                    new WildWhizLevel.Item("octopus", "octopus"),
                },
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    { "fox", "forest" },
                    { "bear", "forest" },
                    { "dolphin", "ocean" },
                    { "octopus", "ocean" },
                });

            WildWhizLevel diets = new(
                "diet-2",
                "Sort by diet.",
                new[] { "herbivore", "carnivore" },
                new[]
                {
                    new WildWhizLevel.Item("rabbit", "rabbit"),
                    new WildWhizLevel.Item("giraffe", "giraffe"),
                    new WildWhizLevel.Item("lion", "lion"),
                    new WildWhizLevel.Item("wolf", "wolf"),
                },
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    { "rabbit", "herbivore" },
                    { "giraffe", "herbivore" },
                    { "lion", "carnivore" },
                    { "wolf", "carnivore" },
                });

            WildWhizLevel movement = new(
                "move-3",
                "Sort by movement.",
                new[] { "fly", "swim", "walk" },
                new[]
                {
                    new WildWhizLevel.Item("eagle", "eagle"),
                    new WildWhizLevel.Item("parrot", "parrot"),
                    new WildWhizLevel.Item("shark", "shark"),
                    new WildWhizLevel.Item("elephant", "elephant"),
                },
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    { "eagle", "fly" },
                    { "parrot", "fly" },
                    { "shark", "swim" },
                    { "elephant", "walk" },
                });

            return new[] { habitats, diets, movement };
        }
    }
}
