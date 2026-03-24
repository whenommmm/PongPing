using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject ball;          
    public GameObject gameOverPanel;  
    public TextMeshProUGUI gameOverText;  
    public TextMeshProUGUI scoreText;     

    private float leftBoundary;
    private float rightBoundary;
    private float ballHalfSize;

    private bool isGameOver = false;
    private int score = 0;

    private const string PrefDifficulty = "Difficulty";
    private const string PrefHighScore = "HighScore";

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // figure out screen edges
        float halfWidth = Camera.main.orthographicSize * Camera.main.aspect;
        ballHalfSize = ball.GetComponent<SpriteRenderer>().bounds.extents.x;

        leftBoundary = -halfWidth - ballHalfSize;
        rightBoundary = halfWidth + ballHalfSize;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // pin score to top right corner
        if (scoreText != null)
        {
            RectTransform rt = scoreText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);    
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-10f, -10f); 
            rt.sizeDelta = new Vector2(200f, 60f);
            scoreText.alignment = TMPro.TextAlignmentOptions.Right;
        }

        UpdateScoreDisplay();
        ApplyDifficulty();
    }

    private void ApplyDifficulty()
    {
        // check what they chose in the menu
        int diff = PlayerPrefs.GetInt(PrefDifficulty, 1);
        PingoMovement pingo = ball.GetComponent<PingoMovement>();
        if (pingo == null) return;

        switch (diff)
        {
            case 0: 
                pingo.startSpeed = 8f;
                pingo.maxSpeed = 10f;
                pingo.rampDuration = 30f;
                break;
            case 1: 
                pingo.startSpeed = 9f;
                pingo.maxSpeed = 13f;
                pingo.rampDuration = 30f;
                break;
            case 2: 
                pingo.startSpeed = 10f;
                pingo.maxSpeed = 16f;
                pingo.rampDuration = 25f;
                break;
        }
    }

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
            // restart if they hit space
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

            return;
        }

        // see if ball slipped past us
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

        // check for new high score
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
    }
}
