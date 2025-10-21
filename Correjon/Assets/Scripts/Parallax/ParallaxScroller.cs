using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxScroller : MonoBehaviour
{
    [System.Serializable]
    public class Biome
    {
        public Sprite sprite;          // imagen del bioma
        public float changeAtDistance; // distancia (m) a partir de la cual usar este bioma
    }

    [Header("Movimiento")]
    public Transform target;                 // jugador o cámara
    [Range(0f, 1f)] public float parallaxFactor = 0.5f;

    [Header("Biomas (orden ascendente por distancia)")]
    public Biome[] biomes;

    // internos
    float textureUnitSizeX;
    Vector3 lastTargetPosition;
    float startX;
    int currentBiomeIndex = -1;

    SpriteRenderer rootSR;

    void Start()
    {
        if (!target) target = Camera.main.transform;

        rootSR = GetComponent<SpriteRenderer>();
        lastTargetPosition = target.position;
        startX = target.position.x;

        // Asegura clones y sprite inicial
        CreateOrRefreshCopies(rootSR);

        // Forzar bioma inicial según distancia 0
        UpdateBiomeByDistance(0f, force: true);
    }

    void Update()
    {
        // Parallax
        Vector3 delta = target.position - lastTargetPosition;
        transform.position += new Vector3(delta.x * parallaxFactor, 0f, 0f);
        lastTargetPosition = target.position;

        // Loop infinito
        if (Mathf.Abs(target.position.x - transform.position.x) >= textureUnitSizeX)
        {
            float offset = (target.position.x - transform.position.x) % textureUnitSizeX;
            transform.position = new Vector3(target.position.x + offset, transform.position.y, transform.position.z);
        }

        // Cambio de bioma por distancia recorrida
        float dist = target.position.x - startX;
        UpdateBiomeByDistance(dist, force: false);
    }

    // ----- Biomas -----

    void UpdateBiomeByDistance(float distance, bool force)
    {
        if (biomes == null || biomes.Length == 0) return;

        // Busca el bioma más alto cuyo changeAtDistance <= distance
        int index = 0;
        for (int i = 0; i < biomes.Length; i++)
        {
            if (distance >= biomes[i].changeAtDistance) index = i;
            else break;
        }

        if (index != currentBiomeIndex || force)
        {
            ApplyBiome(index);
        }
    }

    void ApplyBiome(int index)
    {
        currentBiomeIndex = Mathf.Clamp(index, 0, biomes.Length - 1);
        var b = biomes[currentBiomeIndex];
        if (!b.sprite) return;

        // Cambia el sprite en root y clones
        SetSpriteForAll(b.sprite);

        // Recalcula ancho y reubica clones
        RecalculateWidthFromSprite(b.sprite);
        PositionCopies();
    }

    // ----- Tiles/copias -----

    void CreateOrRefreshCopies(SpriteRenderer sr)
    {
        // Si no hay clones, créalos (uno a la izq y otro a la der)
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

        // Calcular ancho y posicionar
        RecalculateWidthFromSprite(sr.sprite);
        PositionCopies();
    }

    void SetSpriteForAll(Sprite s)
    {
        // Root
        if (rootSR) rootSR.sprite = s;

        // Clones
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
        // Coloca clones exactamente a un ancho a cada lado
        // Asume 2 hijos: L y R (en cualquier orden)
        if (transform.childCount == 0) return;

        // Busca o crea dos posiciones
        int count = Mathf.Min(transform.childCount, 2);
        if (count == 1)
        {
            // Crea otro si solo hay uno por algún motivo
            var extra = new GameObject($"{name}_clone_extra");
            extra.transform.SetParent(transform);
            extra.AddComponent<SpriteRenderer>();
            count = 2;
        }

        // Ordenar por nombre para consistencia (opcional)
        // y posicionar -width y +width
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

