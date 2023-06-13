using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CamerasController : MonoBehaviour
{
    public VirtualCameraController TacticalControlCamera => _tacticalControlCamera;

    [SerializeField] private VirtualCameraController _tacticalControlCamera;

    public VirtualCameraController CurrentCamera => _currentCamera;

    private VirtualCameraController _currentCamera;

    public void ChooseCamera(VirtualCameraController camera)
    {
        if (camera == _currentCamera) return;

        if (_currentCamera != null)
            _currentCamera.DecPriority();
        camera.gameObject.SetActive(true);
        camera.IncrPriority();
        _currentCamera = camera;
    }
}
