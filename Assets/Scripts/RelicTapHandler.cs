using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// Attach to XROrigin. Raycasts screen taps against relic colliders
/// and calls Interact() if the Guardian taps one.
/// </summary>
public class RelicTapHandler : MonoBehaviour
{
    [SerializeField] private Camera arCamera;

    void OnEnable()  { EnhancedTouchSupport.Enable(); }
    void OnDisable() { EnhancedTouchSupport.Disable(); }

    void Update()
    {
        if (!TryGetTapPosition(out Vector2 screenPos)) return;

        Ray ray = arCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            RelicInteractable relic = hit.collider.GetComponentInParent<RelicInteractable>();
            relic?.Interact();
        }
    }

    private static bool TryGetTapPosition(out Vector2 pos)
    {
        pos = default;
#if UNITY_EDITOR
        if (UnityEngine.InputSystem.Mouse.current != null &&
            UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            pos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            return true;
        }
#else
        foreach (Touch touch in Touch.activeTouches)
        {
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                pos = touch.screenPosition;
                return true;
            }
        }
#endif
        return false;
    }
}
