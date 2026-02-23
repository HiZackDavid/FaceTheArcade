using UnityEngine;

/// <summary>
/// Système de vision 2D de l'ennemi dans le labyrinthe.
/// Gère deux types de détection :
///   1. Line of Sight (LOS) : l'ennemi « voit » le joueur si celui-ci est dans le rayon
///      de vision ET qu'aucun mur ne bloque la ligne de vue (raycast 2D).
///   2. Aggro range : zone plus large où l'ennemi continue de poursuivre le joueur
///      même sans ligne de vue directe (par exemple quand le joueur tourne un coin).
/// Ce script est utilisé par EnemyMazeAI pour décider des transitions Wander <-> Chase.
/// </summary>
public class EnemyVision2D : MonoBehaviour
{
    // Référence au Transform du joueur (assignée dans l'Inspector ou dynamiquement)
    public Transform player;

    // === Paramètres de vision ===
    [Header("Vision")]
    public float sightRadius = 4f;      // Rayon de vision (LOS) : bloqué par les murs via raycast
    public float aggroRadius = 7f;      // Rayon d'aggro : plus grand, pas bloqué par les murs

    // === Configuration des obstacles ===
    [Header("Obstacles")]
    public LayerMask wallsMask;         // LayerMask des murs pour le raycast (seul le layer "Walls" bloque la vision)

    /// <summary>
    /// Vérifie si le joueur est dans la zone d'aggro (cercle de rayon aggroRadius).
    /// Pas de vérification d'obstacles : c'est une simple comparaison de distance.
    /// Utilisée par l'IA pour savoir si elle doit continuer la poursuite.
    /// </summary>
    public bool InAggroRange()
    {
        if (!player) return false; // Sécurité : pas de joueur assigné
        return Vector2.Distance(transform.position, player.position) <= aggroRadius;
    }

    /// <summary>
    /// Vérifie si l'ennemi a une ligne de vue directe (Line of Sight) sur le joueur.
    /// Conditions :
    ///   1. Le joueur doit être dans le rayon de vision (sightRadius)
    ///   2. Aucun mur ne doit bloquer la ligne entre l'ennemi et le joueur (Physics2D.Linecast)
    /// C'est cette méthode qui déclenche le passage en mode Chase dans l'IA.
    /// </summary>
    public bool CanSeePlayerLOS()
    {
        if (!player) return false; // Sécurité

        Vector2 a = transform.position; // Position de l'ennemi
        Vector2 b = player.position;    // Position du joueur

        // Étape 1 : vérifier la distance (pas la peine de raycast si trop loin)
        float dist = Vector2.Distance(a, b);
        if (dist > sightRadius) return false;

        // Étape 2 : lancer un raycast (linecast) entre l'ennemi et le joueur
        // Si le raycast touche un collider (= un mur), la vision est bloquée
        var hit = Physics2D.Linecast(a, b, wallsMask);
        return hit.collider == null; // null = pas de mur entre les deux = on voit le joueur
    }
}