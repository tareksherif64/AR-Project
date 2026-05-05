using System;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// Detects AR planes, raycasts on screen tap, and spawns one of three relic prefabs
/// in random order. Each prefab spawns exactly once; taps are ignored after all three
/// have been placed. Attach to the XROrigin GameObject. Assign relicPrefabs in the
/// Inspector. Requires AR Foundation 6.x (TryAddAnchorAsync API).
/// </summary>
[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARAnchorManager))]
public class TapToPlace : MonoBehaviour
{
    public static event Action OnAllRelicsPlaced;

    [SerializeField] private GameObject[] relicPrefabs = new GameObject[3];

    private ARRaycastManager _raycastManager;
    private ARAnchorManager _anchorManager;
    private static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();

    private List<GameObject> _remaining;
    private bool _isPlacing;

    void Awake()
    {
        _raycastManager = GetComponent<ARRaycastManager>();
        _anchorManager = GetComponent<ARAnchorManager>();

        // Required to use EnhancedTouch API
        EnhancedTouchSupport.Enable();

        // Copy into a list we can shuffle and draw from
        _remaining = new List<GameObject>(relicPrefabs);
        Shuffle(_remaining);
    }

    void OnDestroy()
    {
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        if (_isPlacing || _remaining.Count == 0)
            return;

        if (!TryGetTapPosition(out Vector2 touchPosition))
            return;

        if (!_raycastManager.Raycast(touchPosition, Hits, TrackableType.PlaneWithinPolygon))
            return;

        PlaceRelicAsync(Hits[0].pose).Forget();
    }

    private async Task PlaceRelicAsync(Pose pose)
    {
        _isPlacing = true;

        // Pick the next prefab from the shuffled list (guaranteed unique)
        GameObject prefab = _remaining[0];
        _remaining.RemoveAt(0);

        var result = await _anchorManager.TryAddAnchorAsync(pose);

        if (result.status.IsSuccess())
        {
            ARAnchor anchor = result.value;
            GameObject relic = Instantiate(prefab, anchor.transform);
            relic.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($"[TapToPlace] Anchor creation failed ({result.status}). Placing without anchor.");
            Instantiate(prefab, pose.position, pose.rotation);
        }

        _isPlacing = false;

        if (_remaining.Count == 0)
            OnAllRelicsPlaced?.Invoke();
    }

    private static void Shuffle(List<GameObject> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static bool TryGetTapPosition(out Vector2 touchPosition)
    {
        touchPosition = default;

#if UNITY_EDITOR
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            touchPosition = Mouse.current.position.ReadValue();
            return true;
        }
#else
        foreach (Touch touch in Touch.activeTouches)
        {
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                touchPosition = touch.screenPosition;
                return true;
            }
        }
#endif
        return false;
    }
}

// Minimal fire-and-forget extension so we don't need UniTask
internal static class TaskExtensions
{
    internal static async void Forget(this Task task)
    {
        try { await task; }
        catch (System.Exception e) { Debug.LogException(e); }
    }
}
