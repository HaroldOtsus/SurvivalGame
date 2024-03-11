using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuSCRIPT : MonoBehaviour
{
    public GameObject logicManager;

    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene");
        Instantiate(logicManager);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
