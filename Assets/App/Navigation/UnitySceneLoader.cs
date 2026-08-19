using UnityEngine.SceneManagement;

namespace Lbs.MiniGames.Navigation
{
    public sealed class UnitySceneLoader : ISceneLoader
    {
        public void Load(string sceneName)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
