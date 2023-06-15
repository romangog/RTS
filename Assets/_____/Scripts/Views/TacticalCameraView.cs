using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TacticalCameraView : MonoBehaviour
{
    public CinemachineVirtualCamera VirtualCamera => _virtualCamera;
    public Transform CameraFollowPoint => _cameraFollowPoint;

    [SerializeField] private CinemachineVirtualCamera _virtualCamera;

    [SerializeField] private Transform _cameraFollowPoint;
}
