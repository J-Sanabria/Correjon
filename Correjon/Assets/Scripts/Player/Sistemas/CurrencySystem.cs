using UnityEngine;
using UnityEngine.Events;

[System.Serializable] public class IntEvent : UnityEvent<int> { }

public class CurrencySystem : MonoBehaviour
{
    public static CurrencySystem Instance { get; private set; }

    [SerializeField] bool persistAcrossScenes = true;
    [SerializeField] string walletKey = "WALLET_COINS";

    public int Wallet { get; private set; }
    public int RunCoins { get; private set; }

    public IntEvent OnWalletChanged;
    public IntEvent OnRunCoinsChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (persistAcrossScenes) DontDestroyOnLoad(gameObject);

        Wallet = PlayerPrefs.GetInt(walletKey, 0);
        OnWalletChanged?.Invoke(Wallet);
        OnRunCoinsChanged?.Invoke(RunCoins);
    }

    public void AddCoins(int amount, bool addToWallet = true, bool addToRun = true)
    {
        if (amount <= 0) return;

        if (addToWallet)
        {
            Wallet += amount;
            PlayerPrefs.SetInt(walletKey, Wallet);
            PlayerPrefs.Save();
            OnWalletChanged?.Invoke(Wallet);
        }

        if (addToRun)
        {
            RunCoins += amount;
            OnRunCoinsChanged?.Invoke(RunCoins);
        }
    }

    public void ResetRunCoins()
    {
        RunCoins = 0;
        OnRunCoinsChanged?.Invoke(RunCoins);
    }
}
