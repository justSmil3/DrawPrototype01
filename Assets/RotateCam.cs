using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCam : CameraBase
{
    public float keyRotationSpeed = 10.0f;
    public float mouseRotationSpeed = 100.0f;
    public float zoomSpeed = 10.0f;
    [Range(0, 1)]
    private float upRotationThrashhold = .99999f;

    private Vector3 pivotPoint = Vector3.zero;
    public Vector3 PivotPoint
    {
        private get { return pivotPoint; }
        set 
        { 
            pivotPoint = value;
            transform.LookAt(pivotPoint);
        }
    }

    private static string kVertical = "Vertical";
    private static string kHorizontal = "Horizontal"; 
    private static string kMouseX = "Mouse X";
    private static string kMouseY = "Mouse Y";
    private static string kZoom = "Mouse ScrollWheel";
    protected virtual void Awake()
    {
        PivotPoint = Vector3.up * 3;
    }

    protected override void Update()
    {
        float inputVertical = Input.GetAxis(kVertical);
        float inputHorizontal = Input.GetAxis(kHorizontal);
        float inputZoom = Input.GetAxis(kZoom);

        float rotationSpeed = keyRotationSpeed;

        if (Input.GetKey(KeyCode.Mouse1))
        {
            inputHorizontal = Mathf.Clamp(inputHorizontal - Input.GetAxis(kMouseX), -1, 1);
            inputVertical = Mathf.Clamp(inputVertical - Input.GetAxis(kMouseY), -1, 1);
            rotationSpeed = mouseRotationSpeed;
        }

        bool moved = inputVertical != 0.0f || inputHorizontal != 0.0f || inputZoom != 0.0f;
        bool canRotateUp = true;
        float pivotDot = Vector3.Dot(Vector3.up, (transform.position - pivotPoint).normalized);
        float verticalValue = 1 / Mathf.Abs(inputVertical) * inputVertical;
        canRotateUp = pivotDot * verticalValue <= upRotationThrashhold;

        if (moved)
        {
            transform.RotateAround(pivotPoint, new Vector3(0.0f, 1.0f, 0.0f), Time.fixedDeltaTime * -inputHorizontal * rotationSpeed);
            if(canRotateUp)
            {
                transform.RotateAround(pivotPoint, transform.right, Time.fixedDeltaTime * inputVertical * rotationSpeed);
            }
            transform.LookAt(pivotPoint);
            transform.position += Time.fixedDeltaTime * transform.forward * zoomSpeed * inputZoom * Vector3.Distance(transform.position, pivotPoint);
        }
    }
}
