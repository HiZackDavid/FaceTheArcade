using UnityEngine;

public class MiniGameSfx : MonoBehaviour
{
    public static MiniGameSfx I { get; private set; }

    [Header("Clips")]
    [SerializeField] private AudioClip laserShootClip;
    [SerializeField] private AudioClip laserHitClip;
    [SerializeField] private AudioClip enemyTouchClip;

    [Header("Volumes")]
    [Range(0f, 1f)][SerializeField] private float laserShootVolume = 0.8f;
    [Range(0f, 1f)][SerializeField] private float laserHitVolume = 0.7f;
    [Range(0f, 1f)][SerializeField] private float enemyTouchVolume = 0.9f;

    [Header("Source")]
    [SerializeField] private AudioSource oneShotSource;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;

        if (oneShotSource == null)
            oneShotSource = GetComponent<AudioSource>();

        if (oneShotSource == null)
            oneShotSource = gameObject.AddComponent<AudioSource>();

        oneShotSource.playOnAwake = false;
        oneShotSource.loop = false;
        oneShotSource.spatialBlend = 0f; // 2D
    }

    public void PlayLaserShoot()
    {
        PlayClip(laserShootClip, laserShootVolume, 0.97f, 1.03f);
    }

    public void PlayLaserHit()
    {
        PlayClip(laserHitClip, laserHitVolume, 0.98f, 1.02f);
    }

    public void PlayEnemyTouch()
    {
        PlayClip(enemyTouchClip, enemyTouchVolume, 0.95f, 1.05f);
    }

    private void PlayClip(AudioClip clip, float volume, float minPitch = 1f, float maxPitch = 1f)
    {
        if (clip == null || oneShotSource == null) return;

        oneShotSource.pitch = Random.Range(minPitch, maxPitch);
        oneShotSource.PlayOneShot(clip, volume);
    }
}