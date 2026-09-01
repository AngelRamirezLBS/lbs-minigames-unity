using System.Collections.Generic;
using Lbs.MiniGames.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lbs.MiniGames.Bootstrap
{
    /// <summary>
    /// Bootstrap-owned configuration boundary for scenes loaded by any navigation path.
    /// </summary>
    public sealed class AppSceneConfigurationGate
    {
        private readonly HashSet<ulong> configuredSceneHandles = new();

        public bool Configure(Scene scene, AppServices services)
        {
            if (!scene.IsValid() || !scene.isLoaded || !configuredSceneHandles.Add(scene.handle.GetRawData())) return false;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (behaviour is IAppScene appScene) appScene.Configure(services);
                }
            }

            return true;
        }

        public void Forget(Scene scene)
        {
            if (scene.IsValid()) configuredSceneHandles.Remove(scene.handle.GetRawData());
        }
    }
}
