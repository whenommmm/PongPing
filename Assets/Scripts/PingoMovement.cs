using UnityEngine;
using UnityEngine.InputSystem;

public class PingoMovement : MonoBehaviour
{
    [Header("Ball Settings")]
    public float startSpeed     = 6f;    // Horizontal speed at game start
    public float maxSpeed       = 12f;   // Horizontal speed cap
    public float rampDuration   = 30f;   // Seconds to go from startSpeed to maxSpeed
    public float verticalSpeed  = 4f;    // Player-controlled vertical speed
    public float maxBounceAngle = 60f;   // Max angle on edge hit

    [Header("Visual Effects")]
    public TrailRenderer ballTrail;
    public Color startTrailColor = Color.cyan;
    public Color maxSpeedTrailColor = Color.red;

    // Derived each frame from ramp
    private float horizontalSpeed;

    // Current velocity — X flips on bounce, Y is player-controlled (reset on bounce)
    private Vector2 velocity;

    // Screen boundaries (vertical only — horizontal boundary checked by GameManager)
    private float topBoundary;
    private float bottomBoundary;
    private float ballHalfSize;
    private float gameStartTime;         // Timestamp when game started

    void Start()
    {
        float halfHeight = Camera.main.orthographicSize;
        ballHalfSize     = GetComponent<SpriteRenderer>().bounds.extents.y;

        topBoundary    =  halfHeight - ballHalfSize;
        bottomBoundary = -halfHeight + ballHalfSize;

        gameStartTime    = Time.time;
        horizontalSpeed  = startSpeed;

        // Start at center, fire LEFT, no vertical movement
        transform.position = Vector2.zero;
        velocity = new Vector2(-horizontalSpeed, 0f);
    }

    void Update()
    {
        // ── Ramp horizontal speed from startSpeed to maxSpeed over rampDuration ─
        float elapsed = Mathf.Clamp(Time.time - gameStartTime, 0f, rampDuration);
        float speedPercentage = elapsed / rampDuration;
        horizontalSpeed = Mathf.Lerp(startSpeed, maxSpeed, speedPercentage);

        // ── Update Trail Renderer ───────────────────────────────────────────
        if (ballTrail != null)
        {
            // Shift color from start (cyan) to max speed (red)
            Color currentColor = Color.Lerp(startTrailColor, maxSpeedTrailColor, speedPercentage);
            ballTrail.startColor = currentColor;
            ballTrail.endColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0f); // fade out at tail

            // Increase trail length as speed increases (time parameter determines length)
            ballTrail.time = Mathf.Lerp(0.15f, 0.45f, speedPercentage);
        }

        // ── Player always controls Y ────────────────────────────────────────
        var kb = Keyboard.current;
        float verticalInput = 0f;
        if      (kb.upArrowKey.isPressed   || kb.wKey.isPressed) verticalInput =  1f;
        else if (kb.downArrowKey.isPressed || kb.sKey.isPressed) verticalInput = -1f;

        // Snap Y instantly to player input (rigid feel), or keep it at 0 when idle
        velocity.y = verticalInput * verticalSpeed;

        // X stays fixed — always moving horizontally at constant speed
        velocity.x = Mathf.Sign(velocity.x) * horizontalSpeed;

        // ── Move ball ───────────────────────────────────────────────────────
        transform.Translate(velocity * Time.deltaTime);

        // ── Clamp to vertical screen edges ──────────────────────────────────
        float clampedY = Mathf.Clamp(transform.position.y, bottomBoundary, topBoundary);
        transform.position = new Vector2(transform.position.x, clampedY);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Paddle")) return;

        // ── Add score ───────────────────────────────────────────────────────
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(5);

        // ── Shrink the paddle that was hit ──────────────────────────────────
        PaddleMovement paddle = collision.gameObject.GetComponent<PaddleMovement>();
        if (paddle != null)
            paddle.ShrinkPaddle();

        // ── Divide paddle into 3 equal zones ───────────────────────────────
        // hitOffset: -1 = bottom, 0 = center, +1 = top
        float paddleHeight = collision.collider.bounds.size.y;
        float hitDelta     = transform.position.y - collision.transform.position.y;
        float hitOffset    = Mathf.Clamp(hitDelta / (paddleHeight * 0.5f), -1f, 1f);

        // ── Flip X direction ────────────────────────────────────────────────
        velocity.x = -velocity.x;

        // ── Set Y based on zone ─────────────────────────────────────────────
        // Top third    (hitOffset >  0.333) → angled upward
        // Middle third (hitOffset ±  0.333) → straight (Y = 0)
        // Bottom third (hitOffset < -0.333) → angled downward
        float bounceY = Mathf.Sin(maxBounceAngle * Mathf.Deg2Rad) * horizontalSpeed;

        if      (hitOffset >  0.333f) velocity.y =  bounceY;  // top zone
        else if (hitOffset < -0.333f) velocity.y = -bounceY;  // bottom zone
        else                          velocity.y =  0f;        // middle zone → straight

        // ── Nudge away to prevent sticking ─────────────────────────────────
        transform.position = new Vector2(
            transform.position.x + Mathf.Sign(velocity.x) * 0.1f,
            transform.position.y
        );
    }
}
