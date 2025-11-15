using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float runSpeed = 4.2f;       // Velocidad inicial
    public float maxSpeed = 5.6f;       // Velocidad maxima
    public float acceleration = 0.25f;  // Incremento gradual por segundo

    private float currentSpeed;         // Velocidad real actual

    [Header("Salto")]
    public float jumpForce = 9.6f;
    public float holdForce = 5.0f;
    public float maxHoldTime = 0.22f;
    public float coyoteTime = 0.10f;
    public float jumpBufferTime = 0.10f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float coyoteCounter;
    private float bufferCounter;
    private float holdCounter;
    private bool isHoldingJump;

    [Header("Deteccion de suelo")]
    public Transform groundCheck;
    public float checkRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("Distancia")]
    public RunDistance runDistance;

    public Animator Personaje;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = runSpeed; // inicia en velocidad base
        if (runDistance != null)
            runDistance.SetSpeed(currentSpeed);
        MusicManager.Instance.PlayGameplayMusic();

    }

    void Update()
    {
        // Aumentar gradualmente la velocidad hasta el maximo
        if (currentSpeed < maxSpeed)
        {
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
        }

        // Aplicar movimiento horizontal constante
        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);

        // Actualizar velocidad en el sistema de distancia
        if (runDistance != null)
            runDistance.SetSpeed(currentSpeed);

        // Comprobar suelo
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // Control de coyote time y buffer
        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            bufferCounter = jumpBufferTime;
        else
            bufferCounter -= Time.deltaTime;

        // Salto
        if (bufferCounter > 0 && coyoteCounter > 0)
        {
            Jump();
            bufferCounter = 0;
        }

        // Salto prolongado
        if (isHoldingJump && holdCounter > 0)
        {
            rb.AddForce(Vector2.up * holdForce, ForceMode2D.Force);
            holdCounter -= Time.deltaTime;
        }

        // Fin de salto prolongado
        if (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
            isHoldingJump = false;
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        holdCounter = maxHoldTime;
        isHoldingJump = true;
        coyoteCounter = 0;
    }

    public void ResetVerticalVelocity()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    public void SetControlEnabled(bool enabledControl)
    {
        // Deshabilitar el script pausa el auto-run y la lógica de salto
        this.enabled = enabledControl;
        if (!enabledControl && rb != null)
            rb.linearVelocity = Vector2.zero;
    }

}

