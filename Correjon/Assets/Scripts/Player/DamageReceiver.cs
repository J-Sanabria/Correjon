using System.Collections;
using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    [Header("Refs")]
    public LivesSystem lives;

    [Header("Config")]
    public float invulnerabilityTime = 0.6f;

    private bool isInvulnerable;
    private SpriteRenderer[] rends;

    void Awake()
    {
        rends = GetComponentsInChildren<SpriteRenderer>(true);
    }

    public void TakeHit(int amount = 1)
    {
        if (isInvulnerable || lives == null) return;

        lives.LoseLife();
        StartCoroutine(InvulnerabilityRoutine());
    }

    IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;

        // titileo visual
        float elapsed = 0f;
        float period = 0.1f;
        while (elapsed < invulnerabilityTime)
        {
            elapsed += period;
            SetAlpha(0.35f);
            yield return new WaitForSeconds(period * 0.5f);
            SetAlpha(1f);
            yield return new WaitForSeconds(period * 0.5f);
        }
        SetAlpha(1f);

        isInvulnerable = false;
    }

    void SetAlpha(float a)
    {
        if (rends == null) return;
        for (int i = 0; i < rends.Length; i++)
        {
            var c = rends[i].color; c.a = a; rends[i].color = c;
        }
    }
}
