using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float runSpeed = 4.2f;          // Velocidad base (m/s)
    public float maxSpeed = 5.6f;          // Velocidad maxima
    public float acceleration = 0.05f;     // Aceleracion por nivel o distancia

    [Header("Salto")]
    public float jumpForce = 9.6f;         // Fuerza base del salto
    public float holdForce = 5.0f;         // Fuerza adicional mientras se mantiene el toque
    public float maxHoldTime = 0.22f;      // Tiempo maximo de salto prolongado
    public float coyoteTime = 0.10f;       // Tiempo de gracia despues de dejar el suelo
    public float jumpBufferTime = 0.10f;   // Tiempo de buffer antes de aterrizar

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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Auto-run con aceleracion progresiva
        if (rb.linearVelocity.x < maxSpeed)
            rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, runSpeed, acceleration), rb.linearVelocity.y);

        // Comprobar suelo
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // Contadores de coyote time y buffer
        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            bufferCounter = jumpBufferTime;
        else
            bufferCounter -= Time.deltaTime;

        // Iniciar salto si se cumplen condiciones
        if (bufferCounter > 0 && coyoteCounter > 0)
        {
            Jump();
            bufferCounter = 0;
        }

        // Mantener toque para salto prolongado
        if (isHoldingJump)
        {
            if (holdCounter > 0)
            {
                rb.AddForce(Vector2.up * holdForce, ForceMode2D.Force);
                holdCounter -= Time.deltaTime;
            }
        }

        // Detectar toque largo
        if (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
            isHoldingJump = false;
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Reinicia velocidad Y
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        holdCounter = maxHoldTime;
        isHoldingJump = true;
        coyoteCounter = 0;
    }
}
