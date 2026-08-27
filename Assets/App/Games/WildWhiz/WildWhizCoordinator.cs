using System;
using System.Collections.Generic;

namespace Lbs.MiniGames.Games.WildWhiz
{
    public sealed class WildWhizCoordinator
    {
        private readonly WildWhizLevelSet levelSet;
        private readonly List<HashSet<string>> resolvedPerLevel;
        private int currentLevelIndex;

        public WildWhizCoordinator(WildWhizLevelSet set)
        {
            levelSet = set ?? throw new ArgumentNullException(nameof(set));
            if (!levelSet.IsValid())
            {
                throw new ArgumentException("LevelSet is invalid — check distinct ids and expected mappings.", nameof(set));
            }

            resolvedPerLevel = new List<HashSet<string>>(levelSet.Levels.Count);
            for (int i = 0; i < levelSet.Levels.Count; i++)
            {
                resolvedPerLevel.Add(new HashSet<string>(StringComparer.Ordinal));
            }

            currentLevelIndex = 0;
            Attempts = 0;
        }

        public int CurrentLevelIndex => currentLevelIndex;

        public WildWhizLevel CurrentLevel => levelSet.Levels[currentLevelIndex];

        public int Attempts { get; private set; }

        public int ResolvedCount => resolvedPerLevel[currentLevelIndex].Count;

        public int TotalCount => CurrentLevel.Items.Count;

        public bool IsLevelCompleted => ResolvedCount == TotalCount;

        public bool IsAllCompleted
        {
            get
            {
                if (levelSet.Levels.Count == 0)
                {
                    return false;
                }

                for (int i = 0; i < resolvedPerLevel.Count; i++)
                {
                    if (resolvedPerLevel[i].Count != levelSet.Levels[i].Items.Count)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public bool TryClassify(string tokenId, string targetId)
        {
            if (IsLevelCompleted)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(tokenId) || string.IsNullOrWhiteSpace(targetId))
            {
                return false;
            }

            if (!CurrentLevel.Expected.TryGetValue(tokenId, out string expectedTarget))
            {
                return false;
            }

            if (resolvedPerLevel[currentLevelIndex].Contains(tokenId))
            {
                return false;
            }

            Attempts++;

            if (!StringComparer.Ordinal.Equals(targetId, expectedTarget))
            {
                return false;
            }

            resolvedPerLevel[currentLevelIndex].Add(tokenId);
            return true;
        }

        public bool TryAdvance()
        {
            if (!IsLevelCompleted)
            {
                return false;
            }

            if (currentLevelIndex >= levelSet.Levels.Count - 1)
            {
                return false;
            }

            currentLevelIndex++;
            return true;
        }

        public void Reset()
        {
            currentLevelIndex = 0;
            Attempts = 0;
            foreach (HashSet<string> s in resolvedPerLevel)
            {
                s.Clear();
            }
        }
    }
}
