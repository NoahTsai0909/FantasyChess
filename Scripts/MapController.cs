using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SceneLoader;
using TMPro;

public class MapController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private Button combatButton;
    [SerializeField] private Button prepSceneButton;

    void Start()
    {
        UpdateUI();
        combatButton.onClick.AddListener(() => {
            StartCombat();
        });
        prepSceneButton.onClick.AddListener(() =>
        {
            inspectTeam();
        });

    }

    void UpdateUI()
    {
        if (RunManager.Instance != null)
        {
            Debug.Log(RunManager.Instance.currentGold);
            goldText.text = $"Gold: {RunManager.Instance.currentGold}";
        }
    }



    // Called by CombatButton onClick()
    public void StartCombat()
    {
        // Set the encounter (you'll need to assign this somehow)
        // RunManager.Instance.currentEncounter = someEncounter;

        SceneLoader.Instance.LoadScene(GameScene.CombatScene);
    }

    public void inspectTeam()
    {
        SceneLoader.Instance.LoadScene(GameScene.PrepScene);
    }
}
