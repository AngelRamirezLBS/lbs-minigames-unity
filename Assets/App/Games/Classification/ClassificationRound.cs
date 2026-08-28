using System;
using System.Collections.Generic;

namespace Lbs.MiniGames.Games.Classification
{
    public sealed class ClassificationRound
    {
        private readonly Dictionary<string, string> expectedClassifications = new();
        private readonly HashSet<string> resolvedAnimals = new();

        public ClassificationRound(params Animal[] animals)
        {
            if (animals == null || animals.Length == 0)
            {
                throw new ArgumentException("A classification round needs at least one animal.", nameof(animals));
            }

            foreach (Animal animal in animals)
            {
                if (string.IsNullOrWhiteSpace(animal.Id)
                    || string.IsNullOrWhiteSpace(animal.Group)
                    || !expectedClassifications.TryAdd(animal.Id, animal.Group))
                {
                    throw new ArgumentException("Each animal needs a unique identifier and group.", nameof(animals));
                }
            }
        }

        public int Attempts { get; private set; }
        public bool IsCompleted { get; private set; }
        public int ResolvedCount => resolvedAnimals.Count;
        public int TotalCount => expectedClassifications.Count;

        public bool TryClassify(string animalId, string classification)
        {
            if (IsCompleted
                || string.IsNullOrWhiteSpace(animalId)
                || !expectedClassifications.TryGetValue(animalId, out string? expectedGroup)
                || resolvedAnimals.Contains(animalId))
            {
                return false;
            }

            Attempts++;
            if (classification != expectedGroup)
            {
                return false;
            }

            resolvedAnimals.Add(animalId);
            IsCompleted = resolvedAnimals.Count == expectedClassifications.Count;
            return true;
        }

        public readonly struct Animal
        {
            public Animal(string id, string group)
            {
                Id = id;
                Group = group;
            }

            public string Id { get; }
            public string Group { get; }
        }
    }
}
