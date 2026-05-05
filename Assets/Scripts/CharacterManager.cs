using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the active character state (Explorer / Guardian).
/// Attach to a persistent UI GameObject in the scene.
/// Wire up the Toggle Button and Label in the Inspector.
/// </summary>
public class CharacterManager : MonoBehaviour
{
    public enum CharacterState { Explorer, Guardian }

    public static CharacterManager Instance { get; private set; }
    public CharacterState CurrentState { get; private set; } = CharacterState.Explorer;

    [SerializeField] private Button toggleButton;
    [SerializeField] private TextMeshProUGUI characterLabel;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        toggleButton.onClick.AddListener(ToggleCharacter);
        RefreshUI();
    }

    private void ToggleCharacter()
    {
        CurrentState = CurrentState == CharacterState.Explorer
            ? CharacterState.Guardian
            : CharacterState.Explorer;

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (CurrentState == CharacterState.Explorer)
        {
            characterLabel.text = "Explorer";
            characterLabel.color = new Color(0.4f, 0.8f, 1f); // light blue
        }
        else
        {
            characterLabel.text = "Guardian";
            characterLabel.color = new Color(1f, 0.75f, 0.2f); // amber
        }
    }

    public bool IsExplorer => CurrentState == CharacterState.Explorer;
    public bool IsGuardian => CurrentState == CharacterState.Guardian;
}
