using UnityEngine;
using TMPro;

/// <summary>
/// Singleton. Tracks and displays the player's score.
/// Attach to a persistent GameObject. Wire up ScoreLabel in the Inspector.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI scoreLabel;

    public int Score { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        UpdateLabel();
    }

    /// <summary>
    /// Called by PuzzleManager on relic resolved.
    /// points = max(25, 100 - (attempts - 1) * 25) + timeBonus
    /// </summary>
    public void AwardPoints(int attempts, int timeBonus = 0)
    {
        int points = Mathf.Max(25, 100 - (attempts - 1) * 25) + timeBonus;
        Score += points;
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        scoreLabel.text = $"Score: {Score}";
    }
}
