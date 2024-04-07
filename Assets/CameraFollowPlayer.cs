using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollowPlayer : MonoBehaviour
{   
    public Transform player;             // Mängitava karakteri asukoht - Player location

    // Nihe, Z koordinaat peab olema -10, kuna muidu kaamera ei näe midagi
    // Offset, Z coordinate must be -10, because otherwise the camera won't see anything
    public Vector3 offset;               

    void Update()
    {
        // Leiab mängu aktiivse stseeni
        // Finds the active scene in game
        UnityEngine.SceneManagement.Scene currentScene = SceneManager.GetActiveScene();

        // Kui aktiivne stseen on 'GameScene' ehk mängu stseen, siis kaamera peab mängijaga kaasa liikuma
        // If the active scene is the game scene, then the camera has to follow the player
        if (currentScene.name == "GameScene")
        {
            // Mängija asukoha leidmine
            // Finds the player's location
            Vector3 desiredPosition = player.position + offset;
            
            // Kaamera mänguobjekti seadmine mängija asukoha peale
            // Sets the camera game object on the player's location
            transform.position = desiredPosition;
        }
    }
}
