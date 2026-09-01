using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lbs.MiniGames.Bootstrap
{
    public sealed class ApplicationBootstrap : MonoBehaviour
    {
        [SerializeField] private GameCatalog catalog;
        [SerializeField] private string lobbySceneName = "Lobby";

        private AppServices services;

        public void SetCatalog(GameCatalog gameCatalog)
        {
            catalog = gameCatalog;
        }

        private void Awake()
        {
            Application.targetFrameRate = 90;
            DontDestroyOnLoad(gameObject);

            GameSession session = new();
            services = new AppServices(session, new GameLauncher(session, new UnitySceneLoader(), lobbySceneName));
            SceneManager.sceneLoaded += ConfigureLoadedScene;
        }

        private void Start()
        {
            if (catalog == null)
            {
                Debug.LogError("Bootstrap requires a GameCatalog.", this);
                return;
            }

            services.GameLauncher.ShowLobby();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= ConfigureLoadedScene;
        }

        private void ConfigureLoadedScene(Scene scene, LoadSceneMode mode)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (behaviour is IAppScene appScene)
                    {
                        appScene.Configure(services);
                    }
                }
            }
        }
    }
}
