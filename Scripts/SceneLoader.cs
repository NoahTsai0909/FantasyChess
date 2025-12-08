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
    }

    public static SceneLoader Instance { get; private set; }

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
        SceneManager.LoadScene((int)scene);
    }

    public void LoadSceneAsync(GameScene scene)
    {
        SceneManager.LoadSceneAsync((int)scene);
    }
}
