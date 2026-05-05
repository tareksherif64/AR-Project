using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Displays the game over screen when all relics are resolved.
/// Attach to a persistent GameObject. Wire up the canvas group,
/// final score label, and play again button in the Inspector.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI flavourText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private float fadeDuration = 1.5f;

    private int _totalRelics;
    private int _resolvedCount;
    private bool _allPlaced;

    void Awake()
    {
        // Start hidden
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        flavourText.text = "The rift is sealed. The relics have returned home.";
    }

    void OnEnable()
    {
        TapToPlace.OnAllRelicsPlaced += HandleAllPlaced;
        RelicInteractable.OnAnyRelicResolved += HandleRelicResolved;
    }

    void OnDisable()
    {
        TapToPlace.OnAllRelicsPlaced -= HandleAllPlaced;
        RelicInteractable.OnAnyRelicResolved -= HandleRelicResolved;
    }

    void Start()
    {
        playAgainButton.onClick.AddListener(RestartScene);
    }

    private void HandleAllPlaced()
    {
        _allPlaced = true;
        CheckGameOver();
    }

    private void HandleRelicResolved()
    {
        _resolvedCount++;
        CheckGameOver();
    }

    private void CheckGameOver()
    {
        // Game over only when all relics have been placed AND all resolved
        if (!_allPlaced) return;
        if (_resolvedCount < 3) return;

        StartCoroutine(ShowGameOver());
    }

    private IEnumerator ShowGameOver()
    {
        finalScoreText.text = $"Final Score\n{ScoreManager.Instance.Score}";

        // Fade in
        float elapsed = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
