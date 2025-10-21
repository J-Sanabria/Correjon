// CoinPickup.cs
using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public int amount = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        CurrencySystem.Instance?.AddRunCoins(amount);
        Destroy(gameObject);
    }
}
