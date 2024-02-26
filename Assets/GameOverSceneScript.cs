using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverSceneScript : MonoBehaviour
{
    public void TryAgain()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void ExitToMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
