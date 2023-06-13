using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Billboard : MonoBehaviour
{
    private Transform _cameraTransform;

    internal void SetCameraTranform(Transform cameraTransform)
    {
        _cameraTransform = cameraTransform;
    }

    private void Update()
    {
        this.transform.forward = _cameraTransform.forward;
    }
}

