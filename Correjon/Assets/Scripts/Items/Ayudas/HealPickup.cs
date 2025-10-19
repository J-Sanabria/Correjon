using UnityEngine;

public class HealPickup : MonoBehaviour
{
    public int healAmount = 1;
    public bool destroyIfNoHeal = true;

    [Header("Anim (opcional)")]
    public Animator animator;
    public string collectTrigger = "Collect";

    Collider2D hitbox;
    bool collected;

    void Awake()
    {
        hitbox = GetComponent<Collider2D>();
        if (!animator) animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;   // opcional

        var lives = LivesSystem.Instance;
        if (lives == null) return;                 // asegúrate de tener Game System en escena

        bool canHeal = lives.CurrentLives < lives.MaxLives;
        if (canHeal)
        {
            lives.AddLife(healAmount);
            BeginCollected();
        }
        else if (destroyIfNoHeal)
        {
            BeginCollected();
        }
    }

    void BeginCollected()
    {
        collected = true;
        if (hitbox) hitbox.enabled = false;
        if (animator && !string.IsNullOrEmpty(collectTrigger))
            animator.SetTrigger(collectTrigger);
        // No se destruye aquí: esperamos a salir de cámara.
    }

    void OnBecameInvisible()
    {
        if (collected) Destroy(gameObject);
    }
}
