using UnityEngine;

public class HealPickup : MonoBehaviour
{
    [Header("Config")]
    public int healAmount = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var lives = other.GetComponent<LivesSystem>();
        if (lives == null) return;

        // solo curar si no esta al maximo
        if (lives.CurrentLives < lives.MaxLives)
        {
            lives.AddLife(healAmount);
        }

        Destroy(gameObject);
    }
}
