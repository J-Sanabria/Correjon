using UnityEngine;

public class PowerPickup : MonoBehaviour
{
    public PowerupType type;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        PowerupSystem.Instance?.ActivatePower(type);
        Destroy(gameObject);
    }
}
