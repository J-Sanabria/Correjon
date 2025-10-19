using UnityEngine;

public class Hazard : MonoBehaviour
{
    public int damage = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var lives = LivesSystem.Instance;
        if (lives == null) return;

        lives.LoseLife(damage);
        Destroy(gameObject);
    }
}
