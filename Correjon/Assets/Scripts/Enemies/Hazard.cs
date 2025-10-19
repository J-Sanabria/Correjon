using UnityEngine;

public class Hazard : MonoBehaviour
{
    [Header("Config")]
    public int damage = 1;
    public float lifetimeAfterPassed = 5f; // seguridad por si queda atras mucho tiempo

    private Transform target; // opcional, para saber cuando quedo atras
    private float spawnTime;

    public void Init(Transform followTarget)
    {
        target = followTarget;
        spawnTime = Time.time;
    }

    void Update()
    {
        // limpieza sencilla si quedo muy atras del target o por tiempo
        if (target != null && transform.position.x < target.position.x - 30f) Destroy(gameObject);
        if (Time.time - spawnTime > 30f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var dr = other.GetComponent<DamageReceiver>();
        if (dr != null) dr.TakeHit(damage);

        Destroy(gameObject);
    }
}
