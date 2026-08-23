using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SceneLoader;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button StartRunButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartRunButton.onClick.AddListener(() => {
            StartRun();
        });
    }

    // Update is called once per frame
    void StartRun()
    {
        SceneLoader.Instance.LoadScene(GameScene.MapScene);
    }
}
