using UnityEngine;

public class RunnerSession : MonoBehaviour
{
    void Start()
    {
        if (CurrencySystem.Instance != null)
            CurrencySystem.Instance.ResetRunCoins();
    }
}
