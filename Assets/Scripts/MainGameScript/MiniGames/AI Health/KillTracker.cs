using UnityEngine;

public class KillTracker : MonoBehaviour
{
    public int kills;
    public void RegisterKill() => kills++;
    public bool CanLaser(int need) => kills >= need;
    public void Consume(int need) => kills = Mathf.Max(0, kills - need);
}