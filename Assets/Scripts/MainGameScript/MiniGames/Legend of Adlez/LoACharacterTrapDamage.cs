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
            if (other.gameObject.layer != _trapLayer) return;
        
            characterHealthScript.TakeDamage(trapDamage);
        }
        
    }
}
