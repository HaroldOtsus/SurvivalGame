using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollowPlayer : MonoBehaviour
{   
    public Transform player;             // M�ngitava karakteri asukoht - Player location

    // Nihe, Z koordinaat peab olema -10, kuna muidu kaamera ei n�e midagi
    // Offset, Z coordinate must be -10, because otherwise the camera won't see anything
    public Vector3 offset;               

    void Update()
    {
        // Leiab m�ngu aktiivse stseeni
        // Finds the active scene in game
        UnityEngine.SceneManagement.Scene currentScene = SceneManager.GetActiveScene();

        // Kui aktiivne stseen on 'GameScene' ehk m�ngu stseen, siis kaamera peab m�ngijaga kaasa liikuma
        // If the active scene is the game scene, then the camera has to follow the player
        if (currentScene.name == "GameScene")
        {
            // M�ngija asukoha leidmine
            // Finds the player's location
            Vector3 desiredPosition = player.position + offset;
            
            // Kaamera m�nguobjekti seadmine m�ngija asukoha peale
            // Sets the camera game object on the player's location
            transform.position = desiredPosition;
        }
    }
}
