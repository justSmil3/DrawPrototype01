using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuControlls : MonoBehaviour
{
    // this one is needed until event system exist
    public RotateCam tmpCam;
    //public GameObject menu;

    private void Awake()
    {
        tmpCam = Camera.main.GetComponent<RotateCam>();
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        menu.SetActive(!menu.activeSelf);
    //    }
    //}
    public void ChangeMouseControllSpeed(float controllSpeed)
    {
        tmpCam.mouseRotationSpeed = controllSpeed;
    }

    public void ChangeKeyboardControllSpeed(float controllSpeed)
    {
        tmpCam.keyRotationSpeed = controllSpeed;
    }
}
