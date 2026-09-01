using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Lbs.MiniGames.Catalog.Editor
{
    public static class GameCatalogValidation
    {
        [MenuItem("Tools/LBS Mini Games/Validate Game Catalogs")]
        public static void ValidateCatalogs()
        {
            bool hasIssues = false;
            Dictionary<string, GameLocation> gamesById = new(StringComparer.Ordinal);

            foreach (string catalogGuid in AssetDatabase.FindAssets("t:GameCatalog"))
            {
                string catalogPath = AssetDatabase.GUIDToAssetPath(catalogGuid);
                GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(catalogPath);
                if (catalog == null)
                {
                    continue;
                }

                foreach (GameDefinition game in catalog.Games)
                {
                    if (game == null)
                    {
                        hasIssues = true;
                        Debug.LogError($"Game catalog '{catalogPath}' contains a missing game definition reference.", catalog);
                        continue;
                    }

                    string definitionPath = AssetDatabase.GetAssetPath(game);
                    if (!game.IsValid())
                    {
                        hasIssues = true;
                        Debug.LogError($"Game definition '{definitionPath}' in catalog '{catalogPath}' is malformed.", game);
                    }

                    if (!HasEnabledBuildScene(game.SceneName))
                    {
                        hasIssues = true;
                        Debug.LogError($"Game definition '{definitionPath}' in catalog '{catalogPath}' configures scene '{game.SceneName}', which is not an enabled Build Settings scene.", game);
                    }

                    if (string.IsNullOrWhiteSpace(game.GameId))
                    {
                        continue;
                    }

                    if (gamesById.TryGetValue(game.GameId, out GameLocation existing))
                    {
                        hasIssues = true;
                        Debug.LogError($"Duplicate game ID '{game.GameId}': catalog '{existing.CatalogPath}', definition '{existing.DefinitionPath}'; catalog '{catalogPath}', definition '{definitionPath}'.", game);
                        continue;
                    }

                    gamesById.Add(game.GameId, new GameLocation(catalogPath, definitionPath));
                }
            }

            if (!hasIssues)
            {
                Debug.Log("Game catalog validation passed.");
            }
        }

        private static bool HasEnabledBuildScene(string sceneName)
        {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled && string.Equals(Path.GetFileNameWithoutExtension(scene.path), sceneName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private readonly struct GameLocation
        {
            public readonly string CatalogPath;
            public readonly string DefinitionPath;

            public GameLocation(string catalogPath, string definitionPath)
            {
                CatalogPath = catalogPath;
                DefinitionPath = definitionPath;
            }
        }
    }
}
