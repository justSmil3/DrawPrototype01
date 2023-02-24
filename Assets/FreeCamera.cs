using System.Collections.Generic;
using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    public TestDraw draw;
    public float m_LookSpeedController = 120f;
    public float m_LookSpeedMouse = 10.0f;
    public float m_MoveSpeed = 10.0f;
    public float m_MoveSpeedIncrement = 2.5f;
    public float m_Turbo = 10.0f;

    private static string kMouseX = "Mouse X";
    private static string kMouseY = "Mouse Y";
    private static string kVertical = "Vertical";
    private static string kHorizontal = "Horizontal";

    //private static string kYAxis = "YAxis";
    private static string kSpeedAxis = "Speed Axis";


    void Update()
    {

        float inputRotateAxisX = 0.0f;
        float inputRotateAxisY = 0.0f;


        float inputVertical = Input.GetAxis(kVertical);
        inputVertical += Input.GetAxis("Mouse ScrollWheel") * m_LookSpeedMouse * 4;
        float inputHorizontal = Input.GetAxis(kHorizontal);
        float inputYAxis = 0.0f;

        if (Input.GetMouseButton(1) && !Input.GetKey(KeyCode.LeftControl))
        {
            inputRotateAxisX = Input.GetAxis(kMouseX) * m_LookSpeedMouse;
            inputRotateAxisY = Input.GetAxis(kMouseY) * m_LookSpeedMouse;
        }
        else if (Input.GetMouseButton(2))
        {
            inputHorizontal = Input.GetAxis(kMouseX) * m_LookSpeedMouse;
            inputYAxis = Input.GetAxis(kMouseY) * m_LookSpeedMouse;
        }
        //float inputChangeSpeed = Input.GetAxis(kSpeedAxis);
        //if (inputChangeSpeed != 0.0f)
        //{
        //    m_MoveSpeed += inputChangeSpeed * m_MoveSpeedIncrement;
        //    if (m_MoveSpeed < m_MoveSpeedIncrement) m_MoveSpeed = m_MoveSpeedIncrement;
        //}


        bool moved = inputRotateAxisX != 0.0f || inputRotateAxisY != 0.0f || inputVertical != 0.0f || inputHorizontal != 0.0f|| inputYAxis != 0.0f;
        if (moved)
        {
            float rotationX = transform.localEulerAngles.x;
            float newRotationY = transform.localEulerAngles.y + inputRotateAxisX;

            // Weird clamping code due to weird Euler angle mapping...
            float newRotationX = (rotationX - inputRotateAxisY);
            if (rotationX <= 90.0f && newRotationX >= 0.0f)
                newRotationX = Mathf.Clamp(newRotationX, 0.0f, 90.0f);
            if (rotationX >= 270.0f)
                newRotationX = Mathf.Clamp(newRotationX, 270.0f, 360.0f);

            transform.localRotation = Quaternion.Euler(newRotationX, newRotationY, transform.localEulerAngles.z);

            float moveSpeed = Time.deltaTime * m_MoveSpeed;
            if (Input.GetMouseButton(1))
                moveSpeed *= Input.GetKey(KeyCode.LeftShift) ? m_Turbo : 1.0f;
            else
                moveSpeed *= Input.GetAxis("Fire1") > 0.0f ? m_Turbo : 1.0f;
            transform.position += transform.forward * moveSpeed * inputVertical;
            transform.position += transform.right * moveSpeed * inputHorizontal;
            transform.position += transform.up * moveSpeed * inputYAxis;
        }
    }
}