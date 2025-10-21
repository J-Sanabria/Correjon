// JournalSystem.cs
using UnityEngine;
using UnityEngine.Events;

public class JournalSystem : MonoBehaviour
{
    public static JournalSystem Instance { get; private set; }

    [SerializeField] bool persistAcrossScenes = true;

    [Header("Desbloqueo por cantidad de hojas")]
    public int[] pagesUnlockThresholds = new int[] { 1, 3, 6, 10 };
    // Ej: con 1 hoja desbloqueas pág 0, con 3 la pág 1, etc.

    public UnityEvent<int> OnLeavesChanged = new UnityEvent<int>();
    public UnityEvent OnPagesUpdated = new UnityEvent();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (persistAcrossScenes) DontDestroyOnLoad(gameObject);

        if (SaveManager.Data == null) SaveManager.Load();
        OnLeavesChanged.Invoke(SaveManager.Data.journalLeaves);
    }

    public int TotalLeaves => SaveManager.Data.journalLeaves;

    public void AddLeaf(int amount = 1)
    {
        if (amount <= 0) return;
        SaveManager.Data.journalLeaves += amount;
        SaveManager.Save();
        OnLeavesChanged.Invoke(SaveManager.Data.journalLeaves);
        OnPagesUpdated.Invoke();
    }

    public int UnlockedPagesCount()
    {
        int leaves = SaveManager.Data.journalLeaves;
        int count = 0;
        for (int i = 0; i < pagesUnlockThresholds.Length; i++)
            if (leaves >= pagesUnlockThresholds[i]) count++;
        return count;
    }

    public bool IsPageUnlocked(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= pagesUnlockThresholds.Length) return false;
        return SaveManager.Data.journalLeaves >= pagesUnlockThresholds[pageIndex];
    }
}
