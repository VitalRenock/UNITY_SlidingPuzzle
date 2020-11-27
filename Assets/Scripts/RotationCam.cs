using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationCam : MonoBehaviour
{

    public  float rotationSpeed = 0.1f;
    Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }

    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed);
        _camera.orthographicSize = Mathf.Sin(Time.time) * 5;
    }
}
