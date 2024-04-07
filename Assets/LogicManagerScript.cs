using UnityEngine;
using TMPro;

public class LogicManagerScript : MonoBehaviour
{
    public int playerScore = 0;         // M�ngija punktid - Player score
    public TextMeshProUGUI scoreText;   // Teksti m�nguobjekt, mis kuvab punkte - Text game object that displays score

    // Lisab punkte vastavalt h�vitatud vastase t��bile(Hetkel k�ik vastased annavad 1-e punkti)
    // Adds score based on the destroyed enemy type(Currently all enemies give 1 point)
    public void addScore(int scoreToAdd)
    {
        playerScore += scoreToAdd;

        scoreText.text = "PUNKTID: " + playerScore.ToString();
    }
}
