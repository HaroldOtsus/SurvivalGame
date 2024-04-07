using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpawnManagerScript : MonoBehaviour
{
    public List<GameObject> enemyPrefabs;       // Nimekiri vastaste prefab m�ngu objektidest
    public Transform[] spawnPoints;             // Vastaste ilmutamis asukohad 
    private float spawnInterval = 3f;           // Vastaste ilmutamis intervall(sekundites) - Enemy spawn interval(in seconds)

    public int algorithmtype;

    // Laine mehaanika muutujad - Wave mechanic variables
    public float waveInterval = 40f;            // Ilmutamis lainete intervall(sekundites) - Spawn wave interval(in seconds)
    private int waveCount = 0;                  // Lainete loendur adaptiivse algoritmi jaoks - Wave counter for the adaptive algorithm
    public int enemiesPerWavePistol = 4;        // P�stoliga vastaste arv iga laine - Pistol enemy amount per wave
    public int enemiesPerWaveMelee = 4;         // L�hiv�itluse vastaste arv iga laine - Melee enemy amount per wave
    public int enemiesPerWaveShield = 2;        // Kilbiga vastaste arv iga laine - Shield enemy amount per wave
    private bool isSpawningWave = false;        // T�si, kui ilmutatakse lainena vastaseid - True if wave of enemies is currently spawning

    // D�naamilise laine mehaanika muutujad - Dynamic wave mechanic variables
    private int totalEnemiesToSpawn;                // Kogu vastaste arv keda ilmutatakse iga laine - Total enemy amount to spawn per wave
    public int enemyPistolDestroyedCount = 0;       // H�vitatud p�stoliga vastaste arv - Pistol enemies destroyed
    public int enemyMeleeDestroyedCount = 0;        // H�vitatud l�hiv�itluse vastaste arv - Melee enemies destroyed
    public int enemyShieldDestroyedCount = 0;       // H�vitatud kilbiga vastaste arv - Shield enemies destroyed
    private float enemyPistolSpawnRatio = 0.4f;     // P�stoliga vastaste ilmutamis suhe
    private float enemyMeleeSpawnRatio = 0.4f;      // L�hiv�itluse vastaste ilmutamis suhe
    private float enemyShieldSpawnRatio = 0.2f;     // Kilbiga vastaste ilmutamis suhe

    // Teksti objektid adaptiivse algoritmi jaoks, et n�idata ilmutamise protsente - Text UI elements to display spawn ratios
    public GameObject ratioText;
    public TextMeshProUGUI ratioTextPistol;
    public TextMeshProUGUI ratioTextMelee;
    public TextMeshProUGUI ratioTextShield;

    void Start()
    {
        // Salvestab m�ngija valitud algoritmi t��bi
        // Saves the player selected algorithm type
        algorithmtype = PlayerPrefs.GetInt("AlgorithmType");

        // Kui 1 v�i 3, siis ilmutatakse staatiliselt v�i d�naamiliselt vastaseid lainetena
        // If it is 1 or 3, then enemies are spawned statically or dynamically in waves
        if (algorithmtype == 1 || algorithmtype == 3)
        {
            // Esialgne laine ilmutamine, et ei peaks ootama 40 sekundit esimese laineni
            // First wave spawn, so the player wouldn't have to wait 40 seconds for the first wave
            FirstWave();

            // Algab l�putu vastaste ilmutamine
            // Endless enemy wave spawning starts
            InvokeRepeating(nameof(SpawnWaves), waveInterval, waveInterval);
        }
        // Kui 2, siis ilmutatakse vastaseid suvaliselt ilma laineteta
        // If 2, then enemies are spawned randomly without waves
        else if (algorithmtype == 2)
        {
            InvokeRepeating("SpawnEnemy", spawnInterval, spawnInterval);
        }
    }

    private void Update()
    {
        // Kui adaptiivne algoritm valitud, siis kuvab vastaste ilmutamis protsente m�ngijale
        // If adaptive algorithm is chosen, then the spawn ratio is displayed for the player
        if (algorithmtype == 3)
        {
            ratioText.SetActive(true);
            ratioTextPistol.text = " - P�STOLIGA: " + enemyPistolSpawnRatio.ToString("F2");
            ratioTextMelee.text = " - KURIKAGA: " + enemyMeleeSpawnRatio.ToString("F2");
            ratioTextShield.text = " - KILBIGA: " + enemyShieldSpawnRatio.ToString("F2");
        }
    }

    void FirstWave()
    {
        for (int i = 0; i < enemiesPerWavePistol; i++)
        {
            Invoke(nameof(SpawnEnemyPistol), i * spawnInterval);
        }
        for (int i = 0; i < enemiesPerWaveMelee; i++)
        {
            Invoke(nameof(SpawnEnemyMelee), i * spawnInterval);
        }
        for (int i = 0; i < enemiesPerWaveShield; i++)
        {
            Invoke(nameof(SpawnEnemyShield), i * spawnInterval);
        }
    }

    // Ilmutab vastaste laineid 1 ja 3 algoritmi t��bi jaoks
    // Spawns enemy waves for the 1 ja 3 algorithm types
    void SpawnWaves()
    {
        if (!isSpawningWave)
        {
            isSpawningWave = true;

            // waveCount suurem kui 1, kuna FirstWave() ei loe ja kolmandal lainel muutuks ilmutamine d�naamiliseks
            // waveCount greater than 1, because FirstWave() doesn't count and so that on the third wave the spawning would be dynamic
            if (algorithmtype == 3 && waveCount > 1)
            {
                // Arvutab kogu h�vitatud vastaste hulga
                // Calculates total enemies destroyed
                float totalDestroyedEnemies = enemyPistolDestroyedCount + enemyMeleeDestroyedCount + enemyShieldDestroyedCount;

                // Arvutab inverteeritud vastaste ilmutamis protsendid
                // Calculates inverted enemy spawn ratios
                float invertedPistolRatio = 1.0f - (float)enemyPistolDestroyedCount / totalDestroyedEnemies;
                float invertedMeleeRatio = 1.0f - (float)enemyMeleeDestroyedCount / totalDestroyedEnemies;
                float invertedShieldRatio = 1.0f - (float)enemyShieldDestroyedCount / totalDestroyedEnemies;

                float totalInvertedRatio = invertedMeleeRatio + invertedPistolRatio + invertedShieldRatio;

                // Arvutab tegeliku vastaste ilmutamis protsendid
                // Calculates spawn ratios for enemies
                enemyPistolSpawnRatio = invertedPistolRatio / totalInvertedRatio;
                enemyMeleeSpawnRatio = invertedMeleeRatio / totalInvertedRatio;
                enemyShieldSpawnRatio = invertedShieldRatio / totalInvertedRatio;

                // Ilmutab vastaste t��bid vastavalt eelnevalt arvutatud protsendiga
                // Spawns enemy types based on previously calculated spawn rates
                int enemyPistolToSpawn = Mathf.RoundToInt(enemyPistolSpawnRatio * totalEnemiesToSpawn);
                int enemyMeleeToSpawn = Mathf.RoundToInt(enemyMeleeSpawnRatio * totalEnemiesToSpawn);
                int enemyShieldToSpawn = Mathf.RoundToInt(enemyShieldSpawnRatio * totalEnemiesToSpawn);

                for (int i = 0; i < enemyPistolToSpawn; i++)
                {
                    Invoke(nameof(SpawnEnemyPistol), i * spawnInterval);
                }
                for (int i = 0; i < enemyMeleeToSpawn; i++)
                {
                    Invoke(nameof(SpawnEnemyMelee), i * spawnInterval);
                }
                for (int i = 0; i < enemyShieldToSpawn; i++)
                {
                    Invoke(nameof(SpawnEnemyShield), i * spawnInterval);
                }
            }
            // Kui staatiline ilmutamine
            // If static spawning
            else
            {
                for (int i = 0; i < enemiesPerWavePistol; i++)
                {
                    Invoke(nameof(SpawnEnemyPistol), i * spawnInterval);
                }
                for (int i = 0; i < enemiesPerWaveMelee; i++)
                {
                    Invoke(nameof(SpawnEnemyMelee), i * spawnInterval);
                }
                for (int i = 0; i < enemiesPerWaveShield; i++)
                {
                    Invoke(nameof(SpawnEnemyShield), i * spawnInterval);
                }
            }
            
            Invoke(nameof(IncrementEnemies), (enemiesPerWavePistol + enemiesPerWaveMelee + enemiesPerWaveShield) * spawnInterval);

            waveCount++;
        }
    }

    // Suurendab j�rgneva laine vastaste kogu arvu
    // Increases the enemy amount for the next wave
    void IncrementEnemies()
    {
        isSpawningWave = false;

        enemiesPerWavePistol++;
        enemiesPerWaveMelee++;

        // Kilbiga vastaste arv suureneb iga teine laine
        // Shield enemy amount increases every other wave
        if (enemiesPerWaveMelee % 2 == 0)
        {
            enemiesPerWaveShield++;
        }

        // Salvestame kogu vastaste arvu d�naamilise laine ilmutamise jaoks
        // Saves total enemy amount for the dynamic wave spawning
        totalEnemiesToSpawn = enemiesPerWavePistol + enemiesPerWaveMelee + enemiesPerWaveShield;
    }

    // Ilmutab p�stoliga vastast suvalises ilmutamis kohas
    // Spawns a pistol type enemy on a random spawnpoint
    void SpawnEnemyPistol()
    {
        int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomSpawnIndex];

        Instantiate(enemyPrefabs[0], spawnPoint.position, spawnPoint.rotation);
    }

    // Ilmutab l�hiv�itluse vastast suvalises ilmutamis kohas
    // Spawns a melee type enemy on a random spawnpoint
    void SpawnEnemyMelee()
    {
        int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomSpawnIndex];

        Instantiate(enemyPrefabs[2], spawnPoint.position, spawnPoint.rotation);
    }

    // Ilmutab kilbiga vastast suvalises ilmutamis kohas
    // Spawns a shield type enemy on a random spawnpoint
    void SpawnEnemyShield()
    {
        int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomSpawnIndex];

        Instantiate(enemyPrefabs[4], spawnPoint.position, spawnPoint.rotation);
    }

    // Ilmutab suvalist t��pi vastast suvalises ilmutamis kohas
    // Spawns a random type enemy on a random spawnpoint
    void SpawnEnemy()
    {
        int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomSpawnIndex];

        int randomEnemyPrefabIndex = Random.Range(0, enemyPrefabs.Count);
        Instantiate(enemyPrefabs[randomEnemyPrefabIndex], spawnPoint.position, spawnPoint.rotation);
    }
}
