using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    public enum GameScene
    {
        Bootstrap = 0,
        MapScene,
        CombatScene,
        PrepScene,
        MainMenuScene,
        EventScene,
        ShopScene,
        RunSummaryScene,
        AdventureSelectionScene,
    }

    public static SceneLoader Instance { get; private set; }

    public GameScene lastScene;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(GameScene scene)
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        lastScene = (GameScene)currentIndex;
        SceneManager.LoadScene((int)scene);
    }

    public void LoadSceneAsync(GameScene scene)
    {
        SceneManager.LoadSceneAsync((int)scene);
    }
}
