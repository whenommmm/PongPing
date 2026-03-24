using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Singleton so PingoMovement can call GameManager.Instance.AddScore()
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public GameObject      ball;           // Drag the ball GameObject here
    public GameObject      gameOverPanel;  // Drag the Game Over UI Panel here
    public TextMeshProUGUI gameOverText;   // Drag the "Game Over" Text here
    public TextMeshProUGUI scoreText;      // Drag the Score Text here

    // Left/right screen boundaries in world units
    private float leftBoundary;
    private float rightBoundary;
    private float ballHalfSize;

    private bool isGameOver = false;
    private int  score      = 0;

    // PlayerPrefs keys (shared with MainMenuManager)
    private const string PrefDifficulty = "Difficulty";
    private const string PrefHighScore  = "HighScore";

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        float halfWidth = Camera.main.orthographicSize * Camera.main.aspect;
        ballHalfSize   = ball.GetComponent<SpriteRenderer>().bounds.extents.x;

        leftBoundary  = -halfWidth - ballHalfSize;
        rightBoundary =  halfWidth + ballHalfSize;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Pin score text to top-centre so it never drifts on resize
        if (scoreText != null)
        {
            RectTransform rt = scoreText.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(1f, 1f);    
            rt.anchorMax        = new Vector2(1f, 1f);
            rt.pivot            = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-10f,-10f); // equal 10px margin from right and top
            rt.sizeDelta        = new Vector2(200f, 60f);
            scoreText.alignment = TMPro.TextAlignmentOptions.Right; // hug the right edge
        }

        UpdateScoreDisplay();
        ApplyDifficulty();
    }

    /// <summary>Reads the saved difficulty and applies speed settings to the ball.</summary>
    private void ApplyDifficulty()
    {
        int diff = PlayerPrefs.GetInt(PrefDifficulty, 1); // default Medium
        PingoMovement pingo = ball.GetComponent<PingoMovement>();
        if (pingo == null) return;

        switch (diff)
        {
            case 0: // Easy
                pingo.startSpeed    = 8f;
                pingo.maxSpeed      = 10f;
                pingo.rampDuration  = 30f;
                break;
            case 1: // Medium (defaults already set, but explicit for clarity)
                pingo.startSpeed    = 9f;
                pingo.maxSpeed      = 13f;
                pingo.rampDuration  = 30f;
                break;
            case 2: // Hard
                pingo.startSpeed    = 10f;
                pingo.maxSpeed      = 16f;
                pingo.rampDuration  = 25f;
                break;
        }
    }

    // Called by PingoMovement every time ball hits a paddle
    public void AddScore(int points)
    {
        if (isGameOver) return;
        score += points;
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    void Update()
    {
        if (isGameOver)
        {
            // Restart scene when spacebar is pressed
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

            return; // Don't check boundaries once game is over
        }

        // Check if ball has crossed left or right boundary
        if (ball.transform.position.x <= leftBoundary ||
            ball.transform.position.x >= rightBoundary)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        isGameOver = true;

       
        ball.GetComponent<PingoMovement>().enabled = false;

        
        if (scoreText != null)
            scoreText.gameObject.SetActive(false);

       
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Save high score
        int prevBest = PlayerPrefs.GetInt(PrefHighScore, 0);
        if (score > prevBest)
        {
            PlayerPrefs.SetInt(PrefHighScore, score);
            PlayerPrefs.Save();
        }

        if (gameOverText != null)
        {
            int best = PlayerPrefs.GetInt(PrefHighScore, 0);
            gameOverText.text = "Game Over\nScore: " + score + "\nBest: " + best;
        }

        Debug.Log("Game Over! Score: " + score);
    }
}
