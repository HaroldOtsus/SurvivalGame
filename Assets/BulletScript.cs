using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private Rigidbody2D bulletRigidBody2D;
    private float bulletSpeed = 20f;        // Kuuli kiirus - Bullet speed
    private float bulletLifetime = 0.5f;    // Kuuli eluaeg(sekundites) enne, kui m�nguobjekt h�vitatakse - Bullet lifetime(in seconds) before it is destroyed
    void Start()
    {
        bulletRigidBody2D = GetComponent<Rigidbody2D>();

        // H�vitab kuuli m�nguobjekti peale 0.5-t sekundit
        // Destroys the bullet game object after 0.5 seconds
        Destroy(gameObject, bulletLifetime);
    }
    void Update()
    {
        // Kuul saab kiirenduse vastavalt kuuli kiirusele
        bulletRigidBody2D.velocity = transform.up * bulletSpeed;
    }

    // Kui kuul puutub millegagi kokku, millel on ka Rigidbody f��sikaline komponent
    // If the bullet collides with another game object that also has a Rigidbody physics component
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // H�vitab kuuli m�nguobjekti, kui puutub kokku �ksk�ik, mis m�nguobjekti sildiga, mis ei ole 'Enemy'
        // N�iteks, kui silt on 'Wall' ehk sein, siis kuuli m�nguobjekt h�vib
        // See on vajalik, et p�stoliga vastased saaksid kilbiga vastastest l�bi lasta ilma kuuli m�nguobjekti h�vinemiseta
        // Destroys the bullet game object, if the tag is everything, but 'Enemy'
        // Example: if the tag is 'Wall' then the bullet game object is destroyed
        // This is neccessary so that pistol enemies could shoot through shield enemies without the bullet game object being destroyed
        if (collision.gameObject.tag != "Enemy")
        {
            Destroy(gameObject);
        }
        // 'Bullet' ehk kuuli silt on ainult m�ngija poolt lastud kuulidel, ehk kuuli m�nguobjekt h�vib, kui m�ngija laseb 'Enemy' sildiga m�nguobjekti
        // The 'Bullet' tag exists only on bullets that the player shoots. If the player shoots a game object with the 'Enemy' tag,
        // the bullet game object is destroyed
        else if (gameObject.tag == "Bullet")
        {
            Destroy(gameObject);
        }
    }
}
