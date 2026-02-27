using UnityEngine;

public class PaddleMovement : MonoBehaviour
{
    [Header("Paddle Settings")]
    public float speed = 4f;

    [Tooltip("Check for LEFT paddle (starts DOWN). Uncheck for RIGHT paddle (starts UP).")]
    public bool isLeftPaddle = true;

    [Header("Random Movement")]
    public float minDirectionChangeTime = 0.5f;  // Min seconds before random direction flip
    public float maxDirectionChangeTime = 2.0f;  // Max seconds before random direction flip

    [Header("Ball Bias")]
    [Range(0f, 1f)]
    [Tooltip("How much the paddle slows down when near the ball's Y. 0 = no bias, 1 = stops at ball.")]
    public float slowdownBias = 0.3f;

    [Tooltip("Distance from ball Y at which slowdown starts.")]
    public float slowdownRange = 1.5f;

    // References & state
    private Transform ball;
    private float topBoundary;
    private float bottomBoundary;
    private int   direction;               // 1 = up, -1 = down
    private float nextDirectionChangeTime; // When to next randomly flip

    // Shrink state
    private float spriteNativeHeight;      // Paddle sprite height at scale (1,1,1)
    public  float shrinkPerHit  = 2f;      // World units removed per hit (1 top + 1 bottom)
    public  float minHeight     = 1.5f;    // Paddle never shrinks below this (world units)

    void Start()
    {
        float halfScreenHeight = Camera.main.orthographicSize;

        // Get the sprite's native (unscaled) height so we can calculate scale correctly
        spriteNativeHeight = GetComponent<SpriteRenderer>().sprite.bounds.size.y;

        // Scale paddle to cover full screen with 0.5 unit margin on top and bottom
        float fullScreenHeight = (halfScreenHeight * 2f) - 1f; // −0.5 top, −0.5 bottom
        float startScaleY      = fullScreenHeight / spriteNativeHeight;
        transform.localScale   = new Vector3(transform.localScale.x, startScaleY, transform.localScale.z);

        // Start centered, initial direction based on which paddle
        transform.position = new Vector2(transform.position.x, 0f);
        direction          = isLeftPaddle ? -1 : 1;

        ScheduleNextDirectionChange();

        // Find ball
        GameObject ballObj = GameObject.FindGameObjectWithTag("Ball");
        if (ballObj != null) ball = ballObj.transform;
        else Debug.LogWarning("PaddleMovement: No GameObject tagged 'Ball' found.");
    }

    // Called by PingoMovement on every paddle collision
    public void ShrinkPaddle()
    {
        float currentHeight = GetComponent<SpriteRenderer>().bounds.size.y;
        if (currentHeight <= minHeight) return;  // Already at minimum size

        // Reduce height by shrinkPerHit world units (clamp to minHeight)
        float newHeight  = Mathf.Max(currentHeight - shrinkPerHit, minHeight);
        float newScaleY  = newHeight / spriteNativeHeight;
        transform.localScale = new Vector3(transform.localScale.x, newScaleY, transform.localScale.z);
    }

    void Update()
    {
        // Recalculate boundaries dynamically since paddle height changes after each shrink
        float halfScreenHeight = Camera.main.orthographicSize;
        float paddleHalfHeight = GetComponent<SpriteRenderer>().bounds.extents.y;
        topBoundary    =  halfScreenHeight - paddleHalfHeight;
        bottomBoundary = -halfScreenHeight + paddleHalfHeight;

        // ── 1. Randomly flip direction at random intervals ───────────────────
        if (Time.time >= nextDirectionChangeTime)
        {
            direction *= -1;
            ScheduleNextDirectionChange();
        }

        // ── 2. Calculate speed with slowdown bias near ball's Y ──────────────
        float currentSpeed = speed;
        if (ball != null)
        {
            float distToBall = Mathf.Abs(transform.position.y - ball.position.y);

            // The closer we are to ball's Y, the more we slow down
            if (distToBall < slowdownRange)
            {
                float slowdownFactor = distToBall / slowdownRange; // 0 at ball, 1 at edge of range
                // Lerp from reduced speed (at ball) to full speed (at range edge)
                currentSpeed = Mathf.Lerp(speed * (1f - slowdownBias), speed, slowdownFactor);
            }
        }

        // ── 3. Move paddle ───────────────────────────────────────────────────
        transform.Translate(Vector2.up * direction * currentSpeed * Time.deltaTime);

        // ── 4. Flip direction and clamp at screen boundaries ─────────────────
        if (transform.position.y >= topBoundary)
        {
            transform.position = new Vector2(transform.position.x, topBoundary);
            direction = -1;
            ScheduleNextDirectionChange();
        }
        else if (transform.position.y <= bottomBoundary)
        {
            transform.position = new Vector2(transform.position.x, bottomBoundary);
            direction = 1;
            ScheduleNextDirectionChange();
        }
    }

    private void ScheduleNextDirectionChange()
    {
        nextDirectionChangeTime = Time.time + Random.Range(minDirectionChangeTime, maxDirectionChangeTime);
    }
}
