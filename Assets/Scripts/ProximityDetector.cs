using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Attach to each relic prefab. Measures distance to the AR Camera each frame.
/// Within the threshold: pulses scale. Outside: returns to normal.
/// </summary>
public class ProximityDetector : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 1.5f;

    [Header("Pulse Settings")]
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float pulseAmount = 0.15f;   // fraction of base scale to add

    private Transform _arCamera;
    private Vector3 _baseScale;
    private Renderer _renderer;
    private Color _baseColor;
    private bool _isNear;

    void Start()
    {
        _baseScale = transform.localScale;
        _renderer = GetComponentInChildren<Renderer>();

        if (_renderer != null)
            _baseColor = _renderer.material.color;

        // Find the AR Camera automatically
        ARCameraManager camManager = FindObjectOfType<ARCameraManager>();
        if (camManager != null)
            _arCamera = camManager.transform;
        else
            _arCamera = Camera.main?.transform;
    }

    void Update()
    {
        if (_arCamera == null) return;

        // Reveal Pulse is an Explorer-only ability
        bool explorerActive = CharacterManager.Instance == null
                              || CharacterManager.Instance.IsExplorer;

        float distance = Vector3.Distance(transform.position, _arCamera.position);
        _isNear = distance <= detectionRadius;

        if (_isNear && explorerActive)
            ApplyProximityEffects();
        else
            ResetEffects();
    }

    private void ApplyProximityEffects()
    {
        // Scale pulse using a sine wave
        float pulse = 1f + pulseAmount * Mathf.Sin(Time.time * pulseSpeed);
        transform.localScale = _baseScale * pulse;

        // Tint the material yellow-white to suggest a glow
        if (_renderer != null)
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // 0..1
            _renderer.material.color = Color.Lerp(_baseColor, Color.yellow, t * 0.5f);
        }
    }

    private void ResetEffects()
    {
        transform.localScale = _baseScale;

        if (_renderer != null)
            _renderer.material.color = _baseColor;
    }
}
