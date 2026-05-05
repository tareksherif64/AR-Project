using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Singleton. Manages the sequence memory puzzle with a countdown timer.
/// Attach to a persistent GameObject. Wire up all UI fields in the Inspector.
/// </summary>
public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [Header("Puzzle UI")]
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private Button[] sequenceButtons;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Settings")]
    [SerializeField] private int sequenceLength = 3;
    [SerializeField] private float highlightDuration = 0.5f;
    [SerializeField] private float highlightGap = 0.25f;
    [SerializeField] private float timerDuration = 30f;
    [SerializeField] private int timeBonusPoints = 50;

    private RelicInteractable _currentRelic;
    private List<int> _sequence = new List<int>();
    private int _playerIndex;
    private bool _acceptingInput;
    private int _attemptCount;

    private float _timeRemaining;
    private bool _timerRunning;
    private bool _puzzleSolved;

    private Color[] _buttonColors;
    private Color _highlightColor = Color.white;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        puzzlePanel.SetActive(false);
    }

    void Start()
    {
        _buttonColors = new Color[sequenceButtons.Length];
        for (int i = 0; i < sequenceButtons.Length; i++)
        {
            int index = i;
            _buttonColors[i] = sequenceButtons[i].image.color;
            sequenceButtons[i].onClick.AddListener(() => OnPlayerTap(index));
        }
    }

    void Update()
    {
        if (!_timerRunning) return;

        _timeRemaining -= Time.deltaTime;
        timerText.text = $"{Mathf.CeilToInt(_timeRemaining)}s";

        // Turn red in the last 10 seconds
        timerText.color = _timeRemaining <= 10f ? Color.red : Color.white;

        if (_timeRemaining <= 0f)
        {
            _timerRunning = false; // stop Update from firing this again
            StartCoroutine(OnTimerExpired());
        }
    }

    public void StartPuzzle(RelicInteractable relic)
    {
        _currentRelic = relic;
        _sequence.Clear();
        _attemptCount = 0;
        _puzzleSolved = false;
        _timeRemaining = timerDuration;
        _timerRunning = true;

        for (int i = 0; i < sequenceLength; i++)
            _sequence.Add(Random.Range(0, sequenceButtons.Length));

        puzzlePanel.SetActive(true);
        SetButtonsInteractable(false);
        instructionText.text = "Watch the sequence...";

        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        yield return new WaitForSeconds(0.5f);

        foreach (int index in _sequence)
        {
            yield return StartCoroutine(FlashButton(index));
            yield return new WaitForSeconds(highlightGap);
        }

        _playerIndex = 0;
        _attemptCount++;
        _acceptingInput = true;
        SetButtonsInteractable(true);
        instructionText.text = "Repeat the sequence!";
    }

    private IEnumerator FlashButton(int index)
    {
        sequenceButtons[index].image.color = _highlightColor;
        yield return new WaitForSeconds(highlightDuration);
        sequenceButtons[index].image.color = _buttonColors[index];
    }

    private void OnPlayerTap(int index)
    {
        if (!_acceptingInput) return;

        StartCoroutine(FlashButton(index));

        if (index == _sequence[_playerIndex])
        {
            _playerIndex++;
            if (_playerIndex >= _sequence.Count)
                StartCoroutine(OnSuccess());
        }
        else
        {
            StartCoroutine(OnWrongInput());
        }
    }

    private IEnumerator OnSuccess()
    {
        _puzzleSolved = true;
        _timerRunning = false;
        _acceptingInput = false;
        SetButtonsInteractable(false);

        bool timeBonus = _timeRemaining > 0f;
        instructionText.text = timeBonus ? "Relic restored! +Time Bonus!" : "Relic restored!";

        yield return new WaitForSeconds(1f);
        puzzlePanel.SetActive(false);

        ScoreManager.Instance.AwardPoints(_attemptCount, timeBonus ? timeBonusPoints : 0);
        _currentRelic.ResolveRelic();
        _currentRelic = null;
    }

    private IEnumerator OnWrongInput()
    {
        _acceptingInput = false;
        SetButtonsInteractable(false);
        instructionText.text = "Wrong! Watch again...";
        yield return new WaitForSeconds(0.8f);
        _playerIndex = 0;
        StartCoroutine(PlaySequence());
    }

    private IEnumerator OnTimerExpired()
    {
        if (_puzzleSolved) yield break;

        _acceptingInput = false;
        SetButtonsInteractable(false);
        instructionText.text = "Time's up!";

        yield return new WaitForSeconds(1f);
        puzzlePanel.SetActive(false);
        _currentRelic = null;
    }

    private void SetButtonsInteractable(bool state)
    {
        foreach (Button b in sequenceButtons)
            b.interactable = state;
    }
}
