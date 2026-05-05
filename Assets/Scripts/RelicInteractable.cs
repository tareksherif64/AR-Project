using System;
using UnityEngine;

/// <summary>
/// Attach to each relic prefab. Handles Guardian interaction and resolved state.
/// Requires a Collider on the relic so raycasts can hit it.
/// </summary>
public class RelicInteractable : MonoBehaviour
{
    public static event Action OnAnyRelicResolved;

    public bool IsResolved { get; private set; }

    public void Interact()
    {
        if (IsResolved) return;
        if (!CharacterManager.Instance.IsGuardian) return;

        PuzzleManager.Instance.StartPuzzle(this);
    }

    public void ResolveRelic()
    {
        IsResolved = true;
        OnAnyRelicResolved?.Invoke();
        Destroy(gameObject);
    }
}
