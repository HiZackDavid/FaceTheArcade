using UnityEngine;

public class PlayerSurvivalShootScript : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform playerGraphics;
    private float shootTimer;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        shootTimer -= Time.deltaTime;
        if (Input.GetKey(KeyCode.Space) && shootTimer <= 0f)
        {
            ShootBullet();
            shootTimer = 0.25f;
        }
    }
    
    private void ShootBullet()
    {  
        var bullet = Instantiate(bulletPrefab, transform.position , playerGraphics.rotation);
        bullet.name = bulletPrefab.name;
        bullet.GetComponent<Rigidbody2D>().AddForce(bullet.transform.up * 0.21f , ForceMode2D.Impulse);
    }
}
