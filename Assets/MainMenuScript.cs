using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuSCRIPT : MonoBehaviour
{
    public GameObject logicManager;         // Loogika halduri mänguobjekt
    public int algorithmType;               // Algoritmi tüüp
    public ToggleGroup toggleGroup;         // Valikugrupi mänguobjekt(3 algoritmi valikut) - 3 possible algorithm options

    void Start()
    {
        // Algväärtustame algoritmi tüübi
        // Set the algorithm type to default value
        algorithmType = 1;
        PlayerPrefs.SetInt("AlgorithmType", algorithmType);
        PlayerPrefs.Save();

        // Valikud ühendatakse valikugrupiga ja lisatakse kuulaja, et avastada muudatusi valikutes
        // Register toggles with the toggle group and add listener to detect toggle changes
        foreach (Toggle toggle in toggleGroup.GetComponentsInChildren<Toggle>())
        {
            toggle.group = toggleGroup;
            toggle.onValueChanged.AddListener(delegate { OnToggleValueChanged(toggle); });
        }
    }

    // Kutsutakse välja, kui valikutes on toimunud muudatus
    // Is called when the toggle selection is changed
    void OnToggleValueChanged(Toggle toggle)
    {
        // Kui valik on valitud mängija poolt
        // If the toggle is selected by player
        if (toggle.isOn)
        {
            // Võtab valiku mänguobjekti nimest numbri ja salvestab selle algoritmi tüübiks
            // Takes a number from the toggle game object's name and saves it as the algorithm type
            string numberStr = new string(toggle.gameObject.name.Where(char.IsDigit).ToArray());
            int number = int.Parse(numberStr);
            algorithmType = number;

            // Salvestab valitud algoritmi tüübi mängija eelistusse
            // Save algorithmType to PlayerPrefs
            PlayerPrefs.SetInt("AlgorithmType", algorithmType);
            PlayerPrefs.Save();
        }
    }

    // Kui 'Mängi' nupule vajutatakse, siis alustatakse mänguga
    // If the 'Mängi' button is pressed, then the game is started
    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    // Kui 'Välju' nupule vajutatakse, siis väljutakse mängust
    // If the 'Välju' button is pressed, then the game is shut down
    public void QuitGame()
    {
        Application.Quit();
    }
}
