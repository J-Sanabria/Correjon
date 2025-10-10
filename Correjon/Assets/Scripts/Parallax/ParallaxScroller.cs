using UnityEngine;

public class ParallaxScroller : MonoBehaviour
{
    [Header("Configuración general")]
    [Tooltip("Transform del jugador o cámara que se mueve hacia adelante.")]
    public Transform target; 
    
    [Tooltip("Velocidad base del desplazamiento (1 = igual que el jugador).")]
    [Range(0f, 1f)] 
    public float parallaxFactor = 0.5f;  

    [Tooltip("Ancho de la textura o sprite en unidades del mundo.")]
    public float textureUnitSizeX;  

    private Vector3 lastTargetPosition;

    void Start()
    {
        if (target == null)
            target = Camera.main.transform;

        lastTargetPosition = target.position;

        // Si no está definido, calcula el ancho del sprite automáticamente
        if (textureUnitSizeX == 0)
        {
            Sprite sprite = GetComponent<SpriteRenderer>().sprite;
            Texture2D texture = sprite.texture;
            textureUnitSizeX = (texture.width / sprite.pixelsPerUnit) * transform.localScale.x;
        }
    }

    void Update()
    {
        Vector3 deltaMovement = target.position - lastTargetPosition;
        transform.position += new Vector3(deltaMovement.x * parallaxFactor, 0, 0);
        lastTargetPosition = target.position;

        // Reposicionamiento para scroll infinito
        if (Mathf.Abs(target.position.x - transform.position.x) >= textureUnitSizeX)
        {
            float offset = (target.position.x - transform.position.x) % textureUnitSizeX;
            transform.position = new Vector3(target.position.x + offset, transform.position.y, transform.position.z);
        }
    }
}
