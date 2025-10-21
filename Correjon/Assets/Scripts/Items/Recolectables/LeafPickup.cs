// LeafPickup.cs
using UnityEngine;

public class LeafPickup : MonoBehaviour
{
    public int amount = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        JournalSystem.Instance?.AddLeaf(amount);
        Destroy(gameObject);
    }
}
