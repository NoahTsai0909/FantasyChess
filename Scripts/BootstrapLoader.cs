using UnityEngine;
using static SceneLoader;

public class BootstrapLoader : MonoBehaviour
{
    void Start()
    {
        // All systems are now initialized via their Awake() methods
        // SceneLoader and RunManager are now ready

        // Load first real scene
        SceneLoader.Instance.LoadScene(GameScene.MainMenuScene);
    }
}
