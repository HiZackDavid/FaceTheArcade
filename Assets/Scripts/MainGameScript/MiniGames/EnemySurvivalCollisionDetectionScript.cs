using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySurvivalCollisionDetectionScript : MonoBehaviour
{
    public event Action<EnemySurvivalCollisionDetectionScript> OnKilledByBullet;
    public event Action<EnemySurvivalCollisionDetectionScript> OnPlayerTouched;
    
    public Transform bulletPrefab;

    private void Start()
    {
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.transform.name);
        
        if (other.name.Equals(bulletPrefab.name))
        {
            //Destroy(other.gameObject);
            Destroy(gameObject);
            OnKilledByBullet?.Invoke(this);
        }
    }
    
    public void PlayerTouched()
    {
        Destroy(gameObject);
        OnPlayerTouched?.Invoke(this);
    }
    private void OnDestroy()
    {
        OnKilledByBullet = null;
        OnPlayerTouched = null;
    }
}
