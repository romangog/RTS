using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Cinemachine;
using System;

public class TacticalCameraController : ITickable
{
    private readonly TacticalCameraView _tacticalCameraView;
    private readonly Settings _settings;
    private float _currentCameraZoom;
    private float _targetCameraFlyDistance;
    private readonly CinemachineTransposer _cameraTransposer;
    public TacticalCameraController(TacticalCameraView tacticalCameraView, GameSettings settings)
    {
        _tacticalCameraView = tacticalCameraView;
        _settings = settings.TacticalCameraSettings;
        _cameraTransposer = _tacticalCameraView.VirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        _currentCameraZoom = _targetCameraFlyDistance = 1f;
    }
    public void Tick()
    {
        float moveMod = 20f;
        _tacticalCameraView.CameraFollowPoint.transform.position += new Vector3(
            Input.GetAxis("Horizontal"),
            0f,
            Input.GetAxis("Vertical")) * Time.deltaTime * moveMod * _settings.CameraMoveModFromZoomCurve.Evaluate(_targetCameraFlyDistance);
        _targetCameraFlyDistance -= Input.mouseScrollDelta.y / 10f;
        _targetCameraFlyDistance = Mathf.Clamp01(_targetCameraFlyDistance);
        _currentCameraZoom = Mathf.MoveTowards(_currentCameraZoom, _targetCameraFlyDistance, Time.deltaTime * 2f);
        _cameraTransposer.m_FollowOffset = Vector3.Lerp(_settings.FollowOffsetMinZoom, _settings.FollowOffsetMaxZoom, _currentCameraZoom);

        
    }

    [Serializable]
    public class Settings
    {
        public Vector3 FollowOffsetMinZoom;
        public Vector3 FollowOffsetMaxZoom;
        public AnimationCurve CameraMoveModFromZoomCurve;
    }
}
