using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LogicManagerScript : MonoBehaviour
{
    public int playerScore;
    public TextMeshProUGUI scoreText;

    public void addScore(int scoreToAdd)
    {
        playerScore += scoreToAdd;
        scoreText.text = "SCORE: " + playerScore.ToString();
    }
}
