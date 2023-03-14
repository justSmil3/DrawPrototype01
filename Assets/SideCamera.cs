using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideCamera : CameraBase
{
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.K))
        {
            transform.Rotate(transform.up, 180, Space.World);
            Debug.Log(transform.up);
            Plane plane = new Plane(transform.forward, 0);
            Vector3 point = plane.ClosestPointOnPlane(transform.position);
            transform.position = point + point - transform.position;
        }
    }

    public void Flip()
    {
        transform.Rotate(transform.up, 180, Space.World);
        Debug.Log(transform.up);
        Plane plane = new Plane(transform.forward, 0);
        Vector3 point = plane.ClosestPointOnPlane(transform.position);
        transform.position = point + point - transform.position;
    }
}
