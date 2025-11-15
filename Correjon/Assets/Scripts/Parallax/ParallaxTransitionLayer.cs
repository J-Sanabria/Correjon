using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxTransitionLayer : MonoBehaviour
{
    [Header("Referencias")]
    public RunDistance runDistance;     // mide la distancia de la carrera
    public Transform cam;               // cámara
    public Sprite transitionSprite;     // sprite de transición

    [Header("Configuración")]
    public float intervalMeters = 100f;     // cada cuántos metros cambia el fondo
    public float appearBeforeMeters = 30f;  // metros antes del cambio donde aparece
    public float speed = 15f;               // velocidad lateral de la banda
    public float posY = 0f;                 // altura en mundo
    public float zOffset = -0.1f;           // delante del fondo
    public float offsetOutsideCamera = 1.2f; // cuántos anchos de cámara fuera aparece

    private SpriteRenderer sr;
    private float camWidth;

    // “Agenda” de cambios
    private float nextChangeDistance;       // siguiente punto donde cambia bioma
    private float nextTransitionDistance;   // distancia donde debe aparecer la banda
    private bool transitionSpawnedThisCycle = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = transitionSprite;
        sr.enabled = false;

        if (!cam) cam = Camera.main.transform;
        if (!runDistance) runDistance = FindAnyObjectByType<RunDistance>();

        if (!cam || !runDistance)
        {
            Debug.LogWarning("ParallaxTransitionLayer: faltan referencias (cam o runDistance).");
            enabled = false;
            return;
        }

        // ancho visible de cámara
        float camHeight = 2f * Camera.main.orthographicSize;
        camWidth = camHeight * Camera.main.aspect;

        if (intervalMeters <= 0f) intervalMeters = 100f;
        if (appearBeforeMeters < 0f) appearBeforeMeters = 0f;
        if (appearBeforeMeters > intervalMeters) appearBeforeMeters = intervalMeters * 0.9f;

        // primer cambio de fondo
        nextChangeDistance = intervalMeters;                     // p.ej. 100 m
        nextTransitionDistance = nextChangeDistance - appearBeforeMeters; // p.ej. 70 m
    }

    void Update()
    {
        if (!runDistance) return;

        float dist = runDistance.Distance;

        // 1) ¿Toca lanzar la banda de esta “vuelta”?
        if (!transitionSpawnedThisCycle && dist >= nextTransitionDistance)
        {
            StartCoroutine(SlideOnce());
            transitionSpawnedThisCycle = true;
        }

        // 2) ¿Ya pasó el punto de cambio de bioma de este ciclo?
        if (dist >= nextChangeDistance)
        {
            // Preparamos el siguiente intervalo
            nextChangeDistance += intervalMeters;            // siguiente cambio: +100
            nextTransitionDistance = nextChangeDistance - appearBeforeMeters;
            transitionSpawnedThisCycle = false;              // listo para la próxima banda
        }
    }

    private IEnumerator SlideOnce()
    {
        // Posición inicial: a la derecha fuera de cámara
        float startX = cam.position.x + camWidth * offsetOutsideCamera;
        float endLimitX = cam.position.x - camWidth * offsetOutsideCamera;

        Vector3 pos = new Vector3(startX, posY, cam.position.z + zOffset);
        transform.position = pos;
        sr.enabled = true;

        // Mientras no salga por completo por la izquierda (según borde de cámara)
        while (transform.position.x > endLimitX)
        {
            transform.position += Vector3.left * speed * Time.deltaTime;
            yield return null;
        }

        sr.enabled = false;
    }
}
