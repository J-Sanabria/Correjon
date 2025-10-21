// CurrencySystem.cs
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class CurrencySystem : MonoBehaviour
{
    public static CurrencySystem Instance { get; private set; }

    [SerializeField] bool persistAcrossScenes = true;

    public int Wallet => SaveManager.Data.wallet; // persistente
    public int RunCoins { get; private set; }     // por carrera

    public UnityEvent<int> OnWalletChanged = new UnityEvent<int>();
    public UnityEvent<int> OnRunCoinsChanged = new UnityEvent<int>();

    bool bankedThisRun = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (persistAcrossScenes) DontDestroyOnLoad(gameObject);

        // Asegura que el save esté cargado
        if (SaveManager.Data == null || SaveManager.Data.wallet == 0 && SaveManager.Data.journalLeaves == 0)
            SaveManager.Load();

        OnWalletChanged.Invoke(Wallet);
        OnRunCoinsChanged.Invoke(RunCoins);
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // Cada carrera empieza limpia (esto evita romper "Reintentar")
        RunCoins = 0;
        bankedThisRun = false;
        OnRunCoinsChanged.Invoke(RunCoins);
    }

    public void AddRunCoins(int amount)
    {
        if (amount <= 0) return;
        RunCoins += amount;
        OnRunCoinsChanged.Invoke(RunCoins);
    }

    // Abonar las monedas de la carrera al total (solo una vez por carrera)
    public void BankRunCoinsOnce()
    {
        if (bankedThisRun) return;
        if (RunCoins > 0)
        {
            SaveManager.Data.wallet += RunCoins;
            SaveManager.Save();
            OnWalletChanged.Invoke(Wallet);
            RunCoins = 0;
            OnRunCoinsChanged.Invoke(RunCoins);
        }
        bankedThisRun = true;
    }

    public bool Spend(int cost)
    {
        if (cost < 0 || cost > Wallet) return false;
        SaveManager.Data.wallet -= cost;
        SaveManager.Save();
        OnWalletChanged.Invoke(Wallet);
        return true;
    }
}
