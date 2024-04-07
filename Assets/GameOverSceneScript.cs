using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverSceneScript : MonoBehaviour
{
    // Kui vajutatakse 'Proovi uuesti' nupu peale, siis mäng algab valitud algoritmiga uuesti
    // If the 'Proovi uuesti' button is pressed, then the game will restart with the previously selected algorithm
    public void TryAgain()
    {
        SceneManager.LoadScene("GameScene");
    }

    // Kui vajutatakse 'Menüü' nupu peale, siis mäng läheb tagasi pea menüüsse
    // If the 'Menüü' button is pressed, then the game will go back to the main menu
    public void ExitToMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
