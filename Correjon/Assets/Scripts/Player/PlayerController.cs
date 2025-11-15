using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float runSpeed = 4.2f;
    public float maxSpeed = 5.6f;
    public float acceleration = 0.25f;

    private float currentSpeed;

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

    [Header("Animacion")]
    public Animator Personaje;

    private readonly int hashIsGrounded = Animator.StringToHash("IsGrounded");

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!Personaje) Personaje = GetComponent<Animator>();

        currentSpeed = runSpeed;
        if (runDistance != null)
            runDistance.SetSpeed(currentSpeed);

        MusicManager.Instance.PlayGameplayMusic();
    }

    void Update()
    {
        if (currentSpeed < maxSpeed)
        {
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
        }

        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);

        if (runDistance != null)
            runDistance.SetSpeed(currentSpeed);

        // detección de suelo
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        if (Personaje)
            Personaje.SetBool(hashIsGrounded, isGrounded);

        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0) ||
            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            bufferCounter = jumpBufferTime;
        else
            bufferCounter -= Time.deltaTime;

        if (bufferCounter > 0 && coyoteCounter > 0)
        {
            Jump();
            bufferCounter = 0;
        }

        if (isHoldingJump && holdCounter > 0)
        {
            rb.AddForce(Vector2.up * holdForce, ForceMode2D.Force);
            holdCounter -= Time.deltaTime;
        }

        if (Input.GetMouseButtonUp(0) ||
            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
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
        this.enabled = enabledControl;
        if (!enabledControl && rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
    }
}
