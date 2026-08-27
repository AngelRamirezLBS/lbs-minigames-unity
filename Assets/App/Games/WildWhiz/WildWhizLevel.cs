using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lbs.MiniGames.Games.WildWhiz
{
    [Serializable]
    public struct ExpectedEntry
    {
        public string tokenId;
        public string targetId;

        public ExpectedEntry(string tokenId, string targetId)
        {
            this.tokenId = tokenId;
            this.targetId = targetId;
        }
    }

    [Serializable]
    public struct WildWhizLevel : ISerializationCallbackReceiver
    {
        [Serializable]
        public struct Item
        {
            [SerializeField]
            private string tokenId;
            [SerializeField]
            private string spriteKey;
            [SerializeField]
            private Sprite sprite;

            public Item(string tokenId, string spriteKey, Sprite sprite = null)
            {
                this.tokenId = tokenId;
                this.spriteKey = spriteKey;
                this.sprite = sprite;
            }

            public string TokenId => tokenId;
            public string SpriteKey => spriteKey;
            public Sprite Sprite => sprite;

            public void SetSprite(Sprite value) => sprite = value;
        }

        [SerializeField]
        private string id;
        [SerializeField]
        private string instruction;
        [SerializeField]
        private AudioClip instructionClip;
        [SerializeField]
        private string[] targets;
        [SerializeField]
        private Sprite[] targetSprites;
        [SerializeField]
        private Item[] items;
        [SerializeField]
        private ExpectedEntry[] expectedEntries;
        [NonSerialized]
        private Dictionary<string, string> expected;

        public WildWhizLevel(
            string id,
            string instruction,
            IReadOnlyList<string> targets,
            IReadOnlyList<Item> items,
            IReadOnlyDictionary<string, string> expected,
            AudioClip instructionClip = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Level id is required.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(instruction))
            {
                throw new ArgumentException("Level instruction is required.", nameof(instruction));
            }

            if (targets == null || targets.Count == 0)
            {
                throw new ArgumentException("Level needs at least one target.", nameof(targets));
            }

            if (items == null || items.Count == 0)
            {
                throw new ArgumentException("Level needs at least one item.", nameof(items));
            }

            if (expected == null || expected.Count == 0)
            {
                throw new ArgumentException("Level needs expected mappings.", nameof(expected));
            }

            HashSet<string> distinctTargets = new(StringComparer.Ordinal);
            foreach (string t in targets)
            {
                if (string.IsNullOrWhiteSpace(t) || !distinctTargets.Add(t))
                {
                    throw new ArgumentException("Targets must be non-empty and distinct.", nameof(targets));
                }
            }

            HashSet<string> distinctTokens = new(StringComparer.Ordinal);
            foreach (Item it in items)
            {
                if (string.IsNullOrWhiteSpace(it.TokenId) || !distinctTokens.Add(it.TokenId))
                {
                    throw new ArgumentException("Each item needs a unique non-empty tokenId.", nameof(items));
                }
            }

            if (expected.Count != items.Count)
            {
                throw new ArgumentException("Expected must cover every item exactly once.", nameof(expected));
            }

            foreach (Item it in items)
            {
                if (!expected.TryGetValue(it.TokenId, out string targetId) || string.IsNullOrWhiteSpace(targetId))
                {
                    throw new ArgumentException($"Missing expected mapping for token '{it.TokenId}'.", nameof(expected));
                }

                if (!distinctTargets.Contains(targetId))
                {
                    throw new ArgumentException($"Expected target '{targetId}' for token '{it.TokenId}' is not a declared target.", nameof(expected));
                }
            }

            this.id = id;
            this.instruction = instruction;
            this.instructionClip = instructionClip;
            this.targets = ToArray(targets);
            this.targetSprites = new Sprite[this.targets.Length];
            this.items = ToArray(items);
            this.expected = new Dictionary<string, string>(expected, StringComparer.Ordinal);
            this.expectedEntries = BuildEntries(this.expected);
        }

        public string Id => id;

        public string Instruction => instruction;

        public AudioClip InstructionClip => instructionClip;

        public IReadOnlyList<string> Targets => targets;
        public IReadOnlyList<Sprite> TargetSprites => targetSprites;

        public IReadOnlyList<Item> Items => items;

        public IReadOnlyDictionary<string, string> Expected
        {
            get
            {
                if (expected == null && expectedEntries != null && expectedEntries.Length > 0)
                {
                    expected = new Dictionary<string, string>(StringComparer.Ordinal);
                    foreach (ExpectedEntry e in expectedEntries)
                    {
                        if (!string.IsNullOrWhiteSpace(e.tokenId))
                        {
                            expected[e.tokenId] = e.targetId;
                        }
                    }
                }

                return expected;
            }
        }

        public bool IsValid()
        {
            // Ensure dictionary is populated from serialized entries if needed
            var exp = Expected;
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(instruction))
            {
                return false;
            }

            if (targets == null || targets.Length == 0 || items == null || items.Length == 0 || exp == null)
            {
                return false;
            }

            HashSet<string> targetSet = new(StringComparer.Ordinal);
            foreach (string t in targets)
            {
                if (string.IsNullOrWhiteSpace(t) || !targetSet.Add(t))
                {
                    return false;
                }
            }

            HashSet<string> tokenSet = new(StringComparer.Ordinal);
            foreach (Item it in items)
            {
                if (string.IsNullOrWhiteSpace(it.TokenId) || !tokenSet.Add(it.TokenId))
                {
                    return false;
                }
            }

            if (exp.Count != items.Length)
            {
                return false;
            }

            foreach (Item it in items)
            {
                if (!exp.TryGetValue(it.TokenId, out string targetId) || !targetSet.Contains(targetId))
                {
                    return false;
                }
            }

            return true;
        }

        public void OnBeforeSerialize()
        {
            if (expected != null && expected.Count > 0)
            {
                expectedEntries = BuildEntries(expected);
            }
        }

        public void OnAfterDeserialize()
        {
            if (expected == null && expectedEntries != null && expectedEntries.Length > 0)
            {
                expected = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (ExpectedEntry e in expectedEntries)
                {
                    if (!string.IsNullOrWhiteSpace(e.tokenId))
                    {
                        expected[e.tokenId] = e.targetId;
                    }
                }
            }
        }

        private static ExpectedEntry[] BuildEntries(IReadOnlyDictionary<string, string> map)
        {
            ExpectedEntry[] arr = new ExpectedEntry[map.Count];
            int i = 0;
            foreach (KeyValuePair<string, string> kv in map)
            {
                arr[i++] = new ExpectedEntry(kv.Key, kv.Value);
            }

            return arr;
        }

        private static T[] ToArray<T>(IReadOnlyList<T> source)
        {
            T[] arr = new T[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                arr[i] = source[i];
            }

            return arr;
        }
    }
}
