using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private Rigidbody2D bulletRigidBody2D;
    private float bulletSpeed = 20f;        // Kuuli kiirus - Bullet speed
    private float bulletLifetime = 0.5f;    // Kuuli eluaeg(sekundites) enne, kui mänguobjekt hävitatakse - Bullet lifetime(in seconds) before it is destroyed
    void Start()
    {
        bulletRigidBody2D = GetComponent<Rigidbody2D>();

        // Hävitab kuuli mänguobjekti peale 0.5-t sekundit
        // Destroys the bullet game object after 0.5 seconds
        Destroy(gameObject, bulletLifetime);
    }
    void Update()
    {
        // Kuul saab kiirenduse vastavalt kuuli kiirusele
        bulletRigidBody2D.velocity = transform.up * bulletSpeed;
    }

    // Kui kuul puutub millegagi kokku, millel on ka Rigidbody füüsikaline komponent
    // If the bullet collides with another game object that also has a Rigidbody physics component
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Hävitab kuuli mänguobjekti, kui puutub kokku ükskõik, mis mänguobjekti sildiga, mis ei ole 'Enemy'
        // Näiteks, kui silt on 'Wall' ehk sein, siis kuuli mänguobjekt hävib
        // See on vajalik, et püstoliga vastased saaksid kilbiga vastastest läbi lasta ilma kuuli mänguobjekti hävinemiseta
        // Destroys the bullet game object, if the tag is everything, but 'Enemy'
        // Example: if the tag is 'Wall' then the bullet game object is destroyed
        // This is neccessary so that pistol enemies could shoot through shield enemies without the bullet game object being destroyed
        if (collision.gameObject.tag != "Enemy")
        {
            Destroy(gameObject);
        }
        // 'Bullet' ehk kuuli silt on ainult mängija poolt lastud kuulidel, ehk kuuli mänguobjekt hävib, kui mängija laseb 'Enemy' sildiga mänguobjekti
        // The 'Bullet' tag exists only on bullets that the player shoots. If the player shoots a game object with the 'Enemy' tag,
        // the bullet game object is destroyed
        else if (gameObject.tag == "Bullet")
        {
            Destroy(gameObject);
        }
    }
}
