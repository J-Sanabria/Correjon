using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public int coinAmount = 1;

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

        var currency = CurrencySystem.Instance;
        if (currency == null) return;

        currency.AddCoins(coinAmount, addToWallet: true, addToRun: true);

        collected = true;
        if (hitbox) hitbox.enabled = false;
        if (animator && !string.IsNullOrEmpty(collectTrigger))
            animator.SetTrigger(collectTrigger);
        Destroy(gameObject);
    }

    void OnBecameInvisible()
    {
        if (collected) Destroy(gameObject);
    }
}
