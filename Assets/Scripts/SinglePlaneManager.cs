using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Keeps only the most recently detected plane visible.
/// Attach to the XROrigin GameObject alongside ARPlaneManager.
/// </summary>
[RequireComponent(typeof(ARPlaneManager))]
public class SinglePlaneManager : MonoBehaviour
{
    private ARPlaneManager _planeManager;

    void Awake()
    {
        _planeManager = GetComponent<ARPlaneManager>();
    }

    void OnEnable()
    {
        _planeManager.trackablesChanged.AddListener(OnPlanesChanged);
    }

    void OnDisable()
    {
        _planeManager.trackablesChanged.RemoveListener(OnPlanesChanged);
    }

    private void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        // Hide every tracked plane, then re-show only the most recently added one
        ARPlane newest = null;

        foreach (ARPlane plane in _planeManager.trackables)
        {
            plane.gameObject.SetActive(false);
        }

        foreach (ARPlane plane in args.added)
        {
            newest = plane;
        }

        if (newest != null)
            newest.gameObject.SetActive(true);
        else
        {
            // No new plane this frame — re-show the last tracked one
            foreach (ARPlane plane in _planeManager.trackables)
            {
                newest = plane;
            }
            if (newest != null)
                newest.gameObject.SetActive(true);
        }
    }
}
