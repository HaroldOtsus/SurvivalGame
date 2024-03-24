using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuSCRIPT : MonoBehaviour
{
    public GameObject logicManager;
    public int algorithmType;
    public ToggleGroup toggleGroup;

    void Start()
    {
        // Register toggles with the toggle group and add listener to detect toggle changes
        foreach (Toggle toggle in toggleGroup.GetComponentsInChildren<Toggle>())
        {
            toggle.group = toggleGroup;
            toggle.onValueChanged.AddListener(delegate { OnToggleValueChanged(toggle); });
        }
    }

    void OnToggleValueChanged(Toggle toggle)
    {
        if (toggle.isOn)
        {
            Debug.Log("Toggle " + toggle.gameObject.name + " is now selected.");
            string numberStr = new string(toggle.gameObject.name.Where(char.IsDigit).ToArray());
            int number = int.Parse(numberStr);
            algorithmType = number;

            // Save algorithmType to PlayerPrefs
            PlayerPrefs.SetInt("AlgorithmType", algorithmType);
            PlayerPrefs.Save();
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
