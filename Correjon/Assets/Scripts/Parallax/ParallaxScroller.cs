using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxScroller : MonoBehaviour
{
    [Header("Configuración")]
    public Transform target;
    [Range(0f, 1f)] public float parallaxFactor = 0.5f;

    private float textureUnitSizeX;
    private Vector3 lastTargetPosition;

    private void Start()
    {
        if (target == null)
            target = Camera.main.transform;

        lastTargetPosition = target.position;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Sprite sprite = sr.sprite;
        Texture2D texture = sprite.texture;
        textureUnitSizeX = (texture.width / sprite.pixelsPerUnit) * transform.localScale.x;

        // Crea clones automáticos a los lados para cubrir todo el ancho
        CreateBackgroundCopies(sr);
    }

    private void Update()
    {
        Vector3 deltaMovement = target.position - lastTargetPosition;
        transform.position += new Vector3(deltaMovement.x * parallaxFactor, 0, 0);
        lastTargetPosition = target.position;

        // Mantiene el loop infinito
        if (Mathf.Abs(target.position.x - transform.position.x) >= textureUnitSizeX)
        {
            float offset = (target.position.x - transform.position.x) % textureUnitSizeX;
            transform.position = new Vector3(target.position.x + offset, transform.position.y, transform.position.z);
        }
    }

    private void CreateBackgroundCopies(SpriteRenderer sr)
    {
        // Verifica si ya tiene clones
        if (transform.childCount > 0) return;

        // Instancia dos copias a los lados (izquierda y derecha)
        for (int i = -1; i <= 1; i++)
        {
            if (i == 0) continue;
            GameObject clone = new GameObject($"{name}_clone_{i}");
            clone.transform.SetParent(transform);
            clone.transform.localScale = Vector3.one;
            clone.transform.position = transform.position + new Vector3(textureUnitSizeX * i, 0, 0);

            SpriteRenderer cloneSR = clone.AddComponent<SpriteRenderer>();
            cloneSR.sprite = sr.sprite;
            cloneSR.sortingLayerID = sr.sortingLayerID;
            cloneSR.sortingOrder = sr.sortingOrder;
        }
    }
}
