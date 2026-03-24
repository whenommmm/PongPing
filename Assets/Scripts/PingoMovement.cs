using UnityEngine;
using UnityEngine.InputSystem;

public class PingoMovement : MonoBehaviour
{
    public float maxSpeed = 12f;
    public float rampDuration = 30f;  
    public float verticalSpeed = 4f;    
    public float startSpeed = 6f;    
    public float maxBounceAngle = 60f;   

    public TrailRenderer ballTrail;
    public Color startTrailColor = Color.cyan;
    public Color maxSpeedTrailColor = Color.red;

    private float horizontalSpeed;
    private Vector2 velocity;
    private float topBoundary;
    private float bottomBoundary;
    private float ballHalfSize;
    private float gameStartTime;

    void Start()
    {
        float halfHeight = Camera.main.orthographicSize;
        ballHalfSize = GetComponent<SpriteRenderer>().bounds.extents.y;

        topBoundary = halfHeight - ballHalfSize;
        bottomBoundary = -halfHeight + ballHalfSize;

        gameStartTime = Time.time;
        horizontalSpeed = startSpeed;

        // start at center and move left
        transform.position = Vector2.zero;
        velocity = new Vector2(-horizontalSpeed, 0f);
    }

    void Update()
    {
        // speed up over time
        float elapsed = Mathf.Clamp(Time.time - gameStartTime, 0f, rampDuration);
        float speedPercentage = elapsed / rampDuration;
        horizontalSpeed = Mathf.Lerp(startSpeed, maxSpeed, speedPercentage);

        if (ballTrail != null)
        {
            // change trail color as we go faster
            Color currentColor = Color.Lerp(startTrailColor, maxSpeedTrailColor, speedPercentage);
            ballTrail.startColor = currentColor;
            ballTrail.endColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0f); 

            ballTrail.time = Mathf.Lerp(0.15f, 0.45f, speedPercentage);
        }

     
        var kb = Keyboard.current;
        float verticalInput = 0f;
        if      (kb.upArrowKey.isPressed   || kb.wKey.isPressed) verticalInput =  1f;
        else if (kb.downArrowKey.isPressed || kb.sKey.isPressed) verticalInput = -1f;

        velocity.y = verticalInput * verticalSpeed;
        velocity.x = Mathf.Sign(velocity.x) * horizontalSpeed;

        transform.Translate(velocity * Time.deltaTime);

        float clampedY = Mathf.Clamp(transform.position.y, bottomBoundary, topBoundary);
        transform.position = new Vector2(transform.position.x, clampedY);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Paddle")) return;

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(5);

        // shrink the paddle when hit
        PaddleMovement paddle = collision.gameObject.GetComponent<PaddleMovement>();
        if (paddle != null)
            paddle.ShrinkPaddle();

        // figure out where on the paddle we hit
        float paddleHeight = collision.collider.bounds.size.y;
        float hitDelta = transform.position.y - collision.transform.position.y;
        float hitOffset = Mathf.Clamp(hitDelta / (paddleHeight * 0.5f), -1f, 1f);

        // bounce back
        velocity.x = -velocity.x;

        // angle the bounce depending on where it hit
        float bounceY = Mathf.Sin(maxBounceAngle * Mathf.Deg2Rad) * horizontalSpeed;

        if      (hitOffset >  0.333f) velocity.y =  bounceY;  
        else if (hitOffset < -0.333f) velocity.y = -bounceY; 
        else                          velocity.y =  0f;       

        // nudge it away so it doesnt get stuck
        transform.position = new Vector2(
            transform.position.x + Mathf.Sign(velocity.x) * 0.1f,
            transform.position.y
        );
    }
}
