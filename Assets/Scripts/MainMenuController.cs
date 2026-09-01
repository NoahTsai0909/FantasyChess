using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SceneLoader;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button compendiumButton;
    [SerializeField] private Button settingsButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Application.runInBackground = true; // Prevents pausing when tabbed out

        playButton.onClick.AddListener(() => SceneLoader.Instance.LoadScene(GameScene.AdventureSelectionScene));
        compendiumButton.onClick.AddListener(() => Debug.Log("Compendium coming soon!"));
        settingsButton.onClick.AddListener(() => Debug.Log("Settings coming soon!"));
    }

    // Update is called once per frame
    void StartRun()
    {
        SceneLoader.Instance.LoadScene(GameScene.MapScene);
    }
}
