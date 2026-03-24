using UnityEngine;

public class PlayerSurvivalShootScript : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform playerGraphics;
    private float _shootTimer;
    public AudioSource shootAudioSource;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _shootTimer -= Time.deltaTime;
        if (Input.GetMouseButton(0) && _shootTimer <= 0f)
        {
            ShootBullet();
            _shootTimer = 0.15f;
        }
    }
    
    private void ShootBullet()
    {  
        var bullet = Instantiate(bulletPrefab, transform.position , playerGraphics.rotation);
        bullet.name = bulletPrefab.name;
        bullet.GetComponent<Rigidbody2D>().AddForce(bullet.transform.up * 0.50f , ForceMode2D.Impulse);
        shootAudioSource.Play();
    }
}
