using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxScroller : MonoBehaviour
{
    [Header("Movimiento")]
    public Transform target; // jugador o cámara
    [Range(0f, 1f)] public float parallaxFactor = 0.5f;

    [Header("Fondos / Biomas")]
    public Sprite[] biomes; // tus 3 sprites (o más)
    public float biomeChangeInterval = 100f; // cada cuántos metros cambia (ej. 50 o 100)

    // Internos
    private float textureUnitSizeX;
    private Vector3 lastTargetPosition;
    private float startX;
    private int currentBiomeIndex = -1;
    private SpriteRenderer rootSR;

    void Start()
    {
        if (!target) target = Camera.main.transform;

        rootSR = GetComponent<SpriteRenderer>();
        lastTargetPosition = target.position;
        startX = target.position.x;

        // Asegura clones y sprite inicial
        CreateOrRefreshCopies(rootSR);

        // Fondo inicial
        ApplyBiome(0);
    }

    void Update()
    {
        // Movimiento parallax
        Vector3 delta = target.position - lastTargetPosition;
        transform.position += new Vector3(delta.x * parallaxFactor, 0f, 0f);
        lastTargetPosition = target.position;

        // Loop infinito de tiles
        if (Mathf.Abs(target.position.x - transform.position.x) >= textureUnitSizeX)
        {
            float offset = (target.position.x - transform.position.x) % textureUnitSizeX;
            transform.position = new Vector3(target.position.x + offset, transform.position.y, transform.position.z);
        }

        // Calcular bioma actual en función de la distancia
        float dist = target.position.x - startX;

        // Indice de bioma cíclico según intervalos
        int newBiomeIndex = Mathf.FloorToInt(dist / biomeChangeInterval) % biomes.Length;
        if (newBiomeIndex < 0) newBiomeIndex += biomes.Length; // por si retrocede

        if (newBiomeIndex != currentBiomeIndex)
        {
            ApplyBiome(newBiomeIndex);
        }
    }

    // ------------------------------------------------------------
    // Cambio de bioma
    // ------------------------------------------------------------

    void ApplyBiome(int index)
    {
        if (biomes == null || biomes.Length == 0) return;

        currentBiomeIndex = Mathf.Clamp(index, 0, biomes.Length - 1);
        Sprite newSprite = biomes[currentBiomeIndex];
        if (!newSprite) return;

        SetSpriteForAll(newSprite);
        RecalculateWidthFromSprite(newSprite);
        PositionCopies();
    }

    // ------------------------------------------------------------
    // Clonación y loop
    // ------------------------------------------------------------

    void CreateOrRefreshCopies(SpriteRenderer sr)
    {
        if (transform.childCount == 0)
        {
            for (int i = -1; i <= 1; i++)
            {
                if (i == 0) continue;
                var clone = new GameObject($"{name}_clone_{(i < 0 ? "L" : "R")}");
                clone.transform.SetParent(transform);
                clone.transform.localScale = Vector3.one;
                var csr = clone.AddComponent<SpriteRenderer>();
                csr.sprite = sr.sprite;
                csr.sortingLayerID = sr.sortingLayerID;
                csr.sortingOrder = sr.sortingOrder;
            }
        }

        RecalculateWidthFromSprite(sr.sprite);
        PositionCopies();
    }

    void SetSpriteForAll(Sprite s)
    {
        if (rootSR) rootSR.sprite = s;
        for (int i = 0; i < transform.childCount; i++)
        {
            var csr = transform.GetChild(i).GetComponent<SpriteRenderer>();
            if (csr) csr.sprite = s;
        }
    }

    void RecalculateWidthFromSprite(Sprite s)
    {
        if (!s) return;
        var tex = s.texture;
        float ppu = s.pixelsPerUnit > 0 ? s.pixelsPerUnit : 100f;
        textureUnitSizeX = (tex.width / ppu) * transform.localScale.x;
        if (textureUnitSizeX <= 0f) textureUnitSizeX = 1f;
    }

    void PositionCopies()
    {
        if (transform.childCount == 0) return;

        int placed = 0;
        for (int i = 0; i < transform.childCount && placed < 2; i++)
        {
            var t = transform.GetChild(i);
            float offset = (placed == 0) ? -textureUnitSizeX : +textureUnitSizeX;
            t.position = new Vector3(transform.position.x + offset, transform.position.y, transform.position.z);
            placed++;
        }
    }

}
