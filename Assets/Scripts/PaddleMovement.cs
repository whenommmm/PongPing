using UnityEngine;

public class PaddleMovement : MonoBehaviour
{
    public float speed = 4f;
    public bool isLeftPaddle = true;
    public float minDirectionChangeTime = 0.5f;  
    public float maxDirectionChangeTime = 2.0f; 
    public float slowdownBias = 0.3f;
    public float slowdownRange = 1.5f;

    private Transform ball;
    private float topBoundary;
    private float bottomBoundary;
    private int direction;              
    private float nextDirectionChangeTime; 

    private float spriteNativeHeight;      
    public float shrinkPerHit = 2f;     
    public float minHeight = 1.5f;   

    void Start()
    {
        // scale paddle to fit the screen
        float halfScreenHeight = Camera.main.orthographicSize;
        spriteNativeHeight = GetComponent<SpriteRenderer>().sprite.bounds.size.y;
        
        float fullScreenHeight = (halfScreenHeight * 2f) - 1f;
        float startScaleY = fullScreenHeight / spriteNativeHeight;
        transform.localScale = new Vector3(transform.localScale.x, startScaleY, transform.localScale.z);

        transform.position = new Vector2(transform.position.x, 0f);
        direction = isLeftPaddle ? -1 : 1;

        ScheduleNextDirectionChange();

        GameObject ballObj = GameObject.FindGameObjectWithTag("Ball");
        if (ballObj != null) ball = ballObj.transform;
    }

    public void ShrinkPaddle()
    {
        float currentHeight = GetComponent<SpriteRenderer>().bounds.size.y;
        if (currentHeight <= minHeight) return; 

        // make it smaller but not too small
        float newHeight = Mathf.Max(currentHeight - shrinkPerHit, minHeight);
        float newScaleY = newHeight / spriteNativeHeight;
        transform.localScale = new Vector3(transform.localScale.x, newScaleY, transform.localScale.z);
    }

    void Update()
    {
        // recalculate edges since it can shrink
        float halfScreenHeight = Camera.main.orthographicSize;
        float paddleHalfHeight = GetComponent<SpriteRenderer>().bounds.extents.y;
        topBoundary = halfScreenHeight - paddleHalfHeight;
        bottomBoundary = -halfScreenHeight + paddleHalfHeight;

        // randomly flip direction
        if (Time.time >= nextDirectionChangeTime)
        {
            direction *= -1;
            ScheduleNextDirectionChange();
        }

        float currentSpeed = speed;
        if (ball != null)
        {
            float distToBall = Mathf.Abs(transform.position.y - ball.position.y);

            // slow down if we are getting close to the ball
            if (distToBall < slowdownRange)
            {
                float slowdownFactor = distToBall / slowdownRange; 
                currentSpeed = Mathf.Lerp(speed * (1f - slowdownBias), speed, slowdownFactor);
            }
        }

        transform.Translate(Vector2.up * direction * currentSpeed * Time.deltaTime);

        // bounce off top and bottom walls
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
