using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class TestDraw : MonoBehaviour
{
    public ComputeShader shader;
    public TreeMesh tmpMeshScript;
    RenderTexture textureBuffer;
    [Range(0, 1)]
    public float thrashhold = 0.85f;
    public float dotChangeThrashhold = 0.05f;

    int drawKernel;
    int clearKernel;

    public CatmullTree tmpCatmullTree;

    private Mesh _mesh;
    private float screenScaleX;
    private float screenScaleY;
    private int radius = 5;
    private int extendetRadius = 5;
    private List<Vector3> points = new List<Vector3>();
    private ComputeBuffer point;
    private Camera cam;

    private Vector3 prevPointDir = Vector3.zero;
    private Vector3 prevAddedPoint = Vector3.zero;
    private float prevDirDot = 1;
    private Vector3 prevPointpos = Vector3.zero;
    private Vector3 lasteddidPointPos = Vector3.zero;

    private float mindistBetweenPoints = 0.1f;
    public float maxDistBetweenPoints = 1.0f;

    private bool bSkipFirst = true;
    private Ray initialMouseRay;

    private void OnDisable()
    {
        point.Release();
    }
    private void Awake()
    {
        cam = Camera.main;
        cam.depthTextureMode = DepthTextureMode.Depth;
    }
    public void Start()
    {
        point = new ComputeBuffer(1, sizeof(float) * 3);
        textureBuffer = new RenderTexture(Screen.width, Screen.height, 4);
        textureBuffer.enableRandomWrite = true;
        gameObject.GetComponent<Renderer>().material.mainTexture = textureBuffer;
        drawKernel = shader.FindKernel("CSMain");
        clearKernel = shader.FindKernel("ClearTexture");
        _mesh = gameObject.GetComponent<MeshFilter>().mesh;
        Vector3 screenMin = Camera.main.WorldToScreenPoint(_mesh.bounds.min);
        Vector3 screnMax = Camera.main.WorldToScreenPoint(_mesh.bounds.max);
        screenScaleX = screnMax.x - screenMin.x;
        screenScaleY = screnMax.y - screenMin.y;
    }

    private void LateUpdate()
    {
        if (bSkipFirst)
        {
            StartCoroutine(WaitThenDo());
        }
        else if(!Input.GetMouseButton(0))
        {
            resetTexture();
        }
    }

    private IEnumerator WaitThenDo()
    {
        yield return new WaitForSeconds(.25f);
        bSkipFirst = false;
        yield return null;
    }

    private void Update()
    {
        float dist2Cam = Vector3.Distance(Camera.main.transform.position, transform.position);
        float planeHeight = 2.0f * Mathf.Tan(0.5f * Camera.main.fieldOfView * Mathf.Deg2Rad) * dist2Cam;
        planeHeight /= 10;
        float planeWidth = planeHeight * Camera.main.aspect;
        transform.localScale = new Vector3(planeWidth, transform.localScale.y, planeHeight);
        if (Input.GetMouseButtonDown(0))
        {
            initialMouseRay = cam.ScreenPointToRay(Input.mousePosition);
        }
        if (Input.GetMouseButton(0))
        {
            shader.SetTexture(drawKernel, "Result", textureBuffer);
            shader.SetBuffer(drawKernel, "_point", point);
            int mousePosX = Screen.width - (int)Input.mousePosition.x;
            int mousePosY = Screen.height - (int)Input.mousePosition.y;
            // Vector3 mp = cam.WorldToScreenPoint(Vector3.up);
            // shader.SetInt("mouseX", (int)mp.x);
            // shader.SetInt("mouseY", (int)mp.y);
            shader.SetInt("mouseX", mousePosX);
            shader.SetInt("mouseY", mousePosY);
            shader.SetInt("radius", radius);
            shader.SetInt("checkDist", extendetRadius);
            shader.Dispatch(drawKernel, Screen.width / 8, Screen.height / 8, 1);
            TryAddPoint();
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            resetTexture();
        }
        if (Input.GetMouseButtonUp(0))
        {
            prevPointpos = prevPointDir = prevAddedPoint = Vector3.zero;
            // tmpCatmullTree.ResetLastPoint();
        }
    }

    private bool IsPosOutside(Vector3 pos, Vector3 min, Vector3 max, bool bUseZ = true)
    {
        if (pos.x > max.x || pos.x < min.x) return true;
        if (pos.y > max.y || pos.y < min.y) return true;
        if (!bUseZ) return false;
        if (pos.z > max.z || pos.z < min.z) return true;
        return false;
    }

    public void resetTexture()
    {
        cam = Camera.main;
        shader.SetTexture(clearKernel, "Result", textureBuffer);
        shader.SetTextureFromGlobal(clearKernel, "depth", "_CameraDepthTexture");
        shader.SetInt("mouseX", Screen.width);
        shader.SetInt("mouseY", Screen.height);
        shader.Dispatch(clearKernel, Screen.width / 8, Screen.height / 8, 1);
    }
    
    private void TryAddPoint()
    {
        Vector3 pos, conPos;
        SplineNode conNode;
        bool add = prevPointDir.Equals(Vector3.zero);
        if (!ExtractPoint(out pos, out conPos, out conNode))
            return;
        if (Vector3.Distance(pos, prevAddedPoint) < mindistBetweenPoints)
            return;
        SplineNode newConNode = null;
        if (add)
        {
            // this is temporary and will be replaced with the distance from the insertion point
            prevPointDir = Vector3.up;
            // will be replaced with prev point
            prevPointpos = pos - prevPointDir;
            lasteddidPointPos = pos;

            points.Add(pos);
            newConNode = tmpCatmullTree.AddPoint(conPos, conNode);
            Debug.Log(conPos.x + ":" + conPos.y + ":" + conPos.z + ":|:" + newConNode.point.x + ":" + newConNode.point.y + ":" + newConNode.point.z);
            //tmpCatmullTree.testPoints.Add(pos);
        }
        else
        {
            // in here check for three conditions to add a point: angle to last point, direction 
            // change and distance.
            float dot = Vector3.Dot(prevPointDir.normalized, (pos - prevPointpos).normalized);
            float dot2 = Vector3.Dot(prevAddedPoint.normalized, (pos - prevAddedPoint).normalized);
            // direction to last point
            if (dot < thrashhold && dot != 0)
            {
                add = true;
            }
            // distance
            else if (Vector3.Distance(pos, lasteddidPointPos) > maxDistBetweenPoints)
            {
                add = true;
            }
            // change in direction
            else
            {
                if (dot2 < prevDirDot)
                {
                    prevDirDot = dot2;
                }
                else if (dot2 - prevDirDot > dotChangeThrashhold)
                {
                    add = true;
                }
            }
        }
        if (add)
        {
            // reset all that needs resetting;
            prevDirDot = 1;

            // TODO currently not working
            points[points.Count - 1] = pos;
            if(newConNode == null)
                tmpCatmullTree.MoveLastPoint(pos);

            //tmpCatmullTree.testPoints[tmpCatmullTree.testPoints.Count - 1] = pos;
            //points.Add(conPos);
            points.Add(pos);
            //SplineNode tmp = tmpCatmullTree.AddPoint(conPos);
            
            if(newConNode == null)
                tmpCatmullTree.AddPoint(pos);
            else
                tmpCatmullTree.AddPoint(pos, newConNode);

            prevAddedPoint = pos;
            //tmpCatmullTree.testPoints.Add(pos);
            prevPointDir = (pos - prevPointpos).normalized;
            lasteddidPointPos = pos;
            //tmpMeshScript.GenerateMeshSpline();
            tmpMeshScript.GenerateMeshSplineByShader();
        }
        else
        {
            points[points.Count - 1] = pos;
            tmpCatmullTree.MoveLastPoint(pos);
            //tmpCatmullTree.testPoints[tmpCatmullTree.testPoints.Count - 1] = pos;
        }
        prevPointpos = pos;
    }

    private bool ExtractPoint(out Vector3 _point, out Vector3 connectionJoint, out SplineNode connectionNode)
    {
        _point = Vector3.zero;
        connectionJoint = Vector3.zero;
        connectionNode = null;

        Vector3[] pArray = new Vector3[1];
        point.GetData(pArray);
        Vector3 p = pArray[0];
        if (p.z == 0) return false;
        Ray mouseRay = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 mousePoint = mouseRay.origin + mouseRay.direction.normalized * p.z;
        SplineNode closestPoint;
        if (!tmpCatmullTree.TryGetClosestPoint(mousePoint, out closestPoint))
            return false;

        connectionNode = closestPoint;

        

        Plane camPlane, pointPlane;
        camPlane = new Plane(cam.transform.forward, Vector3.zero);
        Vector3 camPlanePointFromClosestPoint =
            camPlane.ClosestPointOnPlane(connectionNode.point);
        Vector3 newInNormal = camPlanePointFromClosestPoint - connectionNode.point;

        // this check exists to mitigate points beeing generated at the tip of the camera
        if (newInNormal.magnitude < 0.00001f)
            pointPlane = camPlane;
        else
            pointPlane = new Plane(newInNormal.normalized, newInNormal.magnitude);

        float distMouse2Plane;
        pointPlane.Raycast(mouseRay, out distMouse2Plane);

        _point = mouseRay.origin + mouseRay.direction * distMouse2Plane;



        float dist1, dist2;
        SplineNode p2;
        Vector3 p3;
        p2 = closestPoint.GetNextNode();
        if (p2 == null)
        {
            // I need to figure out a way to still add to the last node 
            return true;
        }
        if (p2.GetNumberOfConnections() > 0)
        {
            p3 = p2.GetNextNode().point;
        }
        else
        {
            p3 = p2.point + p2.point - connectionNode.point;
        }
        dist1 = Vector3.Distance(closestPoint.point, mousePoint);
        dist2 = Vector3.Distance(p2.point, mousePoint);
        float t = dist1 / (dist1 + dist2);
        connectionJoint = CatmullRom.GetPoint(connectionNode.point, p2.point, p3, t);
        return true;
        
        // DEPRICATED
        //// TMP Debug Messaage Debug.Log(Vector3.Distance(cam.transform.position, Vector3.up) + " : " + p.z);
        //Vector3 dir = mouseRay.direction;
        //Plane camPlane = new Plane(cam.transform.forward, 0);

        //Vector3 startPoint = initialMouseRay.origin + initialMouseRay.direction * p.z;


        //// TODO this is not at all performant, because i call the same function later to get to 
        //// the same node. figure out a way to fix it 
        //SplineNode tmpNode;
        //tmpCatmullTree.TryGetClosestPoint(startPoint, out tmpNode);
        //float tmpDist = Vector3.Distance(tmpNode.point, mouseRay.origin);
        //startPoint = tmpNode.point;


        //Vector3 planePoint = camPlane.ClosestPointOnPlane(startPoint);
        //float distPlane2Plane = Vector3.Distance(startPoint, planePoint);
        //float dist1 = Vector3.Distance(camPos, startPoint);
        //float dist2 = Vector3.Distance(camPos, planePoint);
        //distPlane2Plane = dist1 < dist2 ? distPlane2Plane : -distPlane2Plane;
        //Plane plane = new Plane(cam.transform.forward, distPlane2Plane);
        //float dist2Point;
        //plane.Raycast(mouseRay, out dist2Point);
        //Vector3 vector2plane = mouseRay.direction * dist2Point;
        //_point = mouseRay.origin + vector2plane;
        //return true;
    }

    public void DrawPlane(Vector3 position, Vector3 normal, float size = 1)
    {
        Vector3 v3;
        if (normal.normalized != Vector3.forward)
            v3 = Vector3.Cross(normal, Vector3.forward).normalized * normal.magnitude * size;
        else
            v3 = Vector3.Cross(normal, Vector3.up).normalized * normal.magnitude * size;
        var corner0 = position + v3;
        var corner2 = position - v3;
        var q = Quaternion.AngleAxis(90.0f, normal);
        v3 = q * v3;
        var corner1 = position + v3;
        var corner3 = position - v3;
        Debug.DrawLine(corner0, corner2, Color.green);
        Debug.DrawLine(corner1, corner3, Color.green);
        Debug.DrawLine(corner0, corner1, Color.green);
        Debug.DrawLine(corner1, corner2, Color.green);
        Debug.DrawLine(corner2, corner3, Color.green);
        Debug.DrawLine(corner3, corner0, Color.green);
        Debug.DrawRay(position, normal, Color.red);
    }

}
