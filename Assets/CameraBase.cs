using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBase : MonoBehaviour

{
    private Quaternion initialRotation;
    private Vector3 initialPosision;

    public float m_MoveSpeed = 10.0f;
    private float m_LookSpeedMouse = 1.0f;

    private static string kVertical = "Vertical";
    private static string kHorizontal = "Horizontal";


    private static string kMouseX = "Mouse X";
    private static string kMouseY = "Mouse Y";

    protected virtual void Awake()
    {
        initialRotation = transform.rotation;
        initialPosision = transform.position;
    }

    protected virtual void Update()
    {

        float inputVertical = Input.GetAxis(kVertical);
        float inputHorizontal = Input.GetAxis(kHorizontal);
        float inputUp = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ? 1 : 0;
        float inputdown = Input.GetKey(KeyCode.Space) ? 1 : 0;


        inputVertical += Input.GetAxis("Mouse ScrollWheel") * m_LookSpeedMouse * 4;
        float inputYAxis = 0.0f;

        inputYAxis = (inputdown + - 1 * inputUp) * m_LookSpeedMouse;

        if (Input.GetMouseButton(2))
        {
            inputHorizontal = -Input.GetAxis(kMouseX) * m_LookSpeedMouse;
            inputYAxis = -Input.GetAxis(kMouseY) * m_LookSpeedMouse;
        }

        bool moved = inputVertical != 0.0f || inputHorizontal != 0.0f || inputVertical != 0.0f || inputYAxis != 0.0f;
        if (moved)
        {
            float moveSpeed = Time.deltaTime * m_MoveSpeed;
            transform.position += transform.forward * moveSpeed * inputVertical;
            transform.position += transform.right * moveSpeed * inputHorizontal;
            transform.position += transform.up * moveSpeed * inputYAxis;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPosAndRot();
        }
    }

    public void ResetPosAndRot()
    {
        transform.position = initialPosision;
        transform.rotation = initialRotation;
    }
}
