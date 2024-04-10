using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuSCRIPT : MonoBehaviour
{
    public GameObject logicManager;         // Loogika halduri m�nguobjekt
    public int algorithmType;               // Algoritmi t��p
    public ToggleGroup toggleGroup;         // Valikugrupi m�nguobjekt(3 algoritmi valikut) - 3 possible algorithm options

    void Start()
    {
        // Algv��rtustame algoritmi t��bi
        // Set the algorithm type to default value
        algorithmType = 1;
        PlayerPrefs.SetInt("AlgorithmType", algorithmType);
        PlayerPrefs.Save();

        // Valikud �hendatakse valikugrupiga ja lisatakse kuulaja, et avastada muudatusi valikutes
        // Register toggles with the toggle group and add listener to detect toggle changes
        foreach (Toggle toggle in toggleGroup.GetComponentsInChildren<Toggle>())
        {
            toggle.group = toggleGroup;
            toggle.onValueChanged.AddListener(delegate { OnToggleValueChanged(toggle); });
        }
    }

    // Kutsutakse v�lja, kui valikutes on toimunud muudatus
    // Is called when the toggle selection is changed
    void OnToggleValueChanged(Toggle toggle)
    {
        // Kui valik on valitud m�ngija poolt
        // If the toggle is selected by player
        if (toggle.isOn)
        {
            // V�tab valiku m�nguobjekti nimest numbri ja salvestab selle algoritmi t��biks
            // Takes a number from the toggle game object's name and saves it as the algorithm type
            string numberStr = new string(toggle.gameObject.name.Where(char.IsDigit).ToArray());
            int number = int.Parse(numberStr);
            algorithmType = number;

            // Salvestab valitud algoritmi t��bi m�ngija eelistusse
            // Save algorithmType to PlayerPrefs
            PlayerPrefs.SetInt("AlgorithmType", algorithmType);
            PlayerPrefs.Save();
        }
    }

    // Kui 'M�ngi' nupule vajutatakse, siis alustatakse m�nguga
    // If the 'M�ngi' button is pressed, then the game is started
    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    // Kui 'V�lju' nupule vajutatakse, siis v�ljutakse m�ngust
    // If the 'V�lju' button is pressed, then the game is shut down
    public void QuitGame()
    {
        Application.Quit();
    }
}
