using UnityEngine;

namespace MainGameScript.MiniGames.Legend_of_Adlez
{
    public class LoACharacterTrapDamage : MonoBehaviour
    {
        [SerializeField] private CharacterHealthScript characterHealthScript;
        [SerializeField] private float trapDamage = 20f;

        private int _trapLayer;

        void Awake()
        {
            _trapLayer = LayerMask.NameToLayer("LoATrap");
        }

        void Reset()
        {
            characterHealthScript = GetComponent<CharacterHealthScript>();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"Trigger entered with: {other.name}, layer = {LayerMask.LayerToName(other.gameObject.layer)}");
            if (other.gameObject.layer != _trapLayer) return;
        
            Debug.Log($"Taking trap damage from: {other.name}");
            characterHealthScript.TakeDamage(trapDamage);
        }
    
        private void OnTriggerExit2D(Collider2D other)
        {
            Debug.Log($"Trigger exited with: {other.name}, layer = {LayerMask.LayerToName(other.gameObject.layer)}");
        }
    }
}
