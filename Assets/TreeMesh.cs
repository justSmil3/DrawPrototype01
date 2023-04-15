using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TreeMesh : MonoBehaviour
{
    public CatmullTree tree;
    public ComputeShader vertexShader;
    public float maxRadius = 3;
    public float verteciemult = 5;
    public float ra = .1f;
    public bool bUseShader = false;
    private Mesh mesh;

    private float resolution;
    private Vector3[] Tvertecies;
    public Transform camTransform;

    private int height = 2;

    // private Vector2[] uvs;
    // private int[] triangles;

    private ComputeBuffer vertexBuffer;
    private ComputeBuffer triangleBuffer;
    private ComputeBuffer positionBuffer;
    private ComputeBuffer vigorBuffer;

    private void OnDrawGizmos()
    {
        if (Tvertecies != null)
            for (int i = 0; i < Tvertecies.Length; i++)
            {
                Gizmos.DrawSphere(Tvertecies[i], 0.01f);
            }
    }

    private void OnDisable()
    {
        vertexBuffer.Dispose();
        positionBuffer.Dispose();
        vigorBuffer.Dispose();
        triangleBuffer.Dispose();
    }

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        resolution = Mathf.Sqrt(verteciemult);
        //camTransform = Camera.main.transform;
    }

    public void ChangeCam(Camera cam)
    {
        camTransform = cam.transform;
    }

    bool once = true;
    bool bUpdateMeshGen = true;

    private void Update()
    {
        bUpdateMeshGen = Input.GetMouseButton(0);
        if (Input.GetKeyDown(KeyCode.G))
            GenerateMeshSplineByShader();
        if (Input.GetKeyDown(KeyCode.X))
        {
            int width = (int)(ra * verteciemult);
            float widthMult = 1f / (float)width;
            float heightMult = 1f / (float)height;
            for (int i = 0; i < width * height; i++)
            {
                // Debug.Log("t: " + i + " | interpolation: " + Mathf.Floor(i * widthMult) * heightMult);
            }
        }
    }

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        //GenerateMeshSpline();
        GenerateMeshSplineByShader();
    }
    private void FixedUpdate()
    {
        //if (!bUpdateMeshGen) return;
        //mesh = new Mesh();
        //GetComponent<MeshFilter>().mesh = mesh;
        //var watch = System.Diagnostics.Stopwatch.StartNew();
        //GenerateMeshSpline();
        //watch.Stop();
        //Debug.Log(watch.ElapsedMilliseconds);
    }
    int run = 0;

    public void GenerateMeshSplineByShader()
    {
        int width = (int)(ra * verteciemult);
        float widthMult = 1f / (float)width;
        float heightMult = 1f / (float)height;
        float vertecieDivider = 1f / (float)(width * height);

        int posCount = tree.tree.GetTreeSize();
        List<Vector3> posList = tree.tree.ConvertBranch2VectorList(true);
        int numOfEnds = 0;
        for (int i = 1; i < posList.Count; i++)
        {
            if (posList[i].Equals(Vector3.zero))
                numOfEnds++;
        }

        posCount -= numOfEnds;
        int numOfVerts = height * width * posCount;
        int numOfTris = (numOfVerts - width) * 6;


        vertexBuffer = new ComputeBuffer(numOfVerts, sizeof(float) * 3);
        triangleBuffer = new ComputeBuffer(numOfTris, sizeof(float));
        positionBuffer = new ComputeBuffer(posList.Count, sizeof(float) * 3);
        vigorBuffer = new ComputeBuffer(posList.Count, sizeof(float));

        Vector3[] vertBufferList = new Vector3[numOfVerts];
        int[] triBufferList = new int[numOfTris];
        float[] tmpDebugFloatList = tree.tree.GetVigorMultArrayTmp().ToArray();


        vertexBuffer.SetData(vertBufferList);
        triangleBuffer.SetData(triBufferList);
        positionBuffer.SetData(posList.ToArray());
        vigorBuffer.SetData(tmpDebugFloatList);

        vertexShader.SetBuffer(0, "vertecies", vertexBuffer);
        vertexShader.SetBuffer(0, "triangles", triangleBuffer);
        vertexShader.SetBuffer(0, "positions", positionBuffer);
        vertexShader.SetBuffer(0, "vigors", vigorBuffer);
        vertexShader.SetVector("crossVec", camTransform.forward);
        //vertexShader.SetFloat("vertecieDivider", vertecieDivider);
        //vertexShader.SetFloat("widthMult", widthMult);
        vertexShader.SetFloat("widthDivider", widthMult);
        //vertexShader.SetFloat("heightMult", heightMult);
        vertexShader.SetFloat("heightDivider", heightMult);
        //vertexShader.SetInt("numOfPositions", posList.Count - numOfEnds);
        vertexShader.SetInt("numOfPositionsMax", posList.Count);
        vertexShader.SetInt("width", width);
        vertexShader.SetInt("height", height);
        vertexShader.SetInt("numVerts", numOfVerts);

        //vertexShader.Dispatch(0, numOfVerts / 32 + 32, 1, 1);
        vertexShader.Dispatch(0, posCount * height / 32 + 32, 1, 1);
        Vector3[] resultingVerts = new Vector3[numOfVerts];
        Vector3[] resultingPos = new Vector3[posList.Count];
        int[] resultingTris = new int[numOfTris];
        vertexBuffer.GetData(resultingVerts);
        triangleBuffer.GetData(resultingTris);
        //for (int i = 0; i < resultingVerts.Length; i++)
        //{
        //    Debug.Log(i + ": " + resultingVerts[i]);
        //}
        Tvertecies = resultingVerts;
        mesh.vertices = resultingVerts;
        mesh.triangles = resultingTris; 
    }


    public void GenerateMeshSpline(SplineNode current = null, SplineNode prev = null, float vigor = 1, int maxLength = -1)
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        int tmpRun = run;
        run++;
        if (current == null)
        {
            current = tree.tree;
            maxLength = current.GetBranchSize();
        }
        var watch = System.Diagnostics.Stopwatch.StartNew();
        int numberOfConnections = current.GetNumberOfConnections();
        if (numberOfConnections <= 0)
            return;
        var elapsedMs1 = watch.ElapsedMilliseconds;
        maxLength = current.GetBranchSize() > maxLength ? current.GetBranchSize() : maxLength;
        var elapsedMs2 = watch.ElapsedMilliseconds;
        for (int i = 0; i < numberOfConnections; i++)
        {
            maxLength = i > 0 ? current.GetBranchSize(i) : maxLength;

            SplineNode nextNode = current.GetNextNode(i);
            float newVigor = vigor * (nextNode.GetBranchSize() / (float)maxLength);
            Vector3[] points;
            bool first = GenerateSplinePoints(prev, current, nextNode, out points, i);
            float radius1 = vigor * ra;
            float radius2 = newVigor * ra;

            GenerateMeshSegment(radius1,
                                radius2,
                                points[0],
                                points[1],
                                points[2],
                                points[3],
                                first);
            GenerateMeshSpline(nextNode, current, newVigor, maxLength);
        }
        var elapsedMs3 = watch.ElapsedMilliseconds;
        watch.Stop();
        long e2 = elapsedMs2 - elapsedMs1;
        long e3 = elapsedMs3 - elapsedMs2;
    }

    private bool GenerateSplinePoints(SplineNode prev, SplineNode current, SplineNode next, out Vector3[] points, int i)
    {
        bool result = true;
        points = new Vector3[4];
        if (prev == null)
        {
            result = false;
            points[0] = current.point + (current.point - next.point);
        }
        else
        {
            points[0] = prev.point;
        }
        if (next.GetNumberOfConnections() <= 0)
        {
            points[3] = next.point + (next.point - current.point);
        }
        else
        {
            points[3] = next.GetNextNode().point;
        }
        if (i > 0) result = false;
        points[1] = current.point;
        points[2] = next.point;
        return result;
    }


    //private void GenerateMeshSegment(float radius1,
    //                                 float radius2,
    //                                 Vector3 p0,
    //                                 Vector3 p1,
    //                                 Vector3 p2,
    //                                 Vector3 p3,
    //                                 bool addTiangels = true)
    //{
    //    Vector3 p0p1 = p1 - p0;
    //    Vector3 p1p2 = p2 - p1;
    //    Vector3 p2p3 = p3 - p2;
    //    Vector3 p0p3 = p0p1 + p1p2 + p2p3;
    //    float length = p0p3.magnitude;



    //    int width = (int)(ra * verteciemult);
    //    int heigth = System.Math.Max(2, (int)(length * resolution * ra));
    //    //int numtriangles = width * (heigth - 1) * 6;
    //    int numvertecies = (int)(width * heigth);
    //    Vector3[] vertecies = new Vector3[numvertecies];
    //    Vector2[] uvs = new Vector2[numvertecies];
    //    int[] triangles;

    //    if(bUseShader)
    //    vertecies = FillVertexArrayByShader(vertecies, ref uvs, width, heigth, radius1, radius2, p0, p1, p2, p3);
    //    else
    //    vertecies = FillVertexArray(vertecies, ref uvs, width, heigth, radius1, radius2, p0, p1, p2, p3);
    //    Tvertecies = vertecies;
    //    triangles = FillTriangleArray(width, heigth, mesh.vertexCount);

    //    List<Vector3> newVertecies = new List<Vector3>();
    //    List<Vector2> newUVs = new List<Vector2>();
    //    List<int> newTriangles = new List<int>();

    //    newVertecies.AddRange(mesh.vertices);
    //    newUVs.AddRange(mesh.uv);
    //    newUVs.AddRange(uvs);
    //    newTriangles.AddRange(mesh.triangles);
    //    // startindex is related to branchindex, test what it actually does
    //    // TODO left of here 
    //    if (addTiangels)
    //    {
    //        for (int i = 0; i < width; i++)
    //        {
    //            int ul = newVertecies.Count + width + i;
    //            int ur = newVertecies.Count + width + ((i + 1) % width);
    //            int dl = newVertecies.Count - width + i;
    //            int dr = newVertecies.Count - width + ((i + 1) % width);

    //            newTriangles.Add(ur);
    //            newTriangles.Add(ul);
    //            newTriangles.Add(dl);

    //            newTriangles.Add(ur);
    //            newTriangles.Add(dl);
    //            newTriangles.Add(dr);
    //        }
    //    }
    //    newVertecies.AddRange(vertecies);
    //    newTriangles.AddRange(triangles);

    //    mesh.vertices = newVertecies.ToArray();
    //    // Tvertecies = mesh.vertices;
    //    mesh.uv = newUVs.ToArray();
    //    mesh.triangles = newTriangles.ToArray();
    //}

    int uvHeight = 0;

    List<int> tmpMeshTri = new List<int>();
    List<Vector2> tmpMeshUV = new List<Vector2>();

    private void GenerateMeshSegment(float radius1,
                                     float radius2,
                                     Vector3 p0,
                                     Vector3 p1,
                                     Vector3 p2,
                                     Vector3 p3,
                                     bool first = false)
    {

        Vector3 p0p1 = p1 - p0;
        Vector3 p1p2 = p2 - p1;
        Vector3 p2p3 = p3 - p2;
        Vector3 p0p3 = p0p1 + p1p2 + p2p3;
        float length = p0p3.magnitude;



        int width = (int)(ra * verteciemult);
        // int heigth = System.Math.Max(2, (int)(length * resolution * ra));
        List<Vector3> vertecies = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();


        for (int i = 0; i < height; i++)
        {
            float t = i / (float)height;
            Vector3 centerPoint = CatmullRom.GetPoint(p0, p1, p2, p3, t);
            Vector3 dir = CatmullRom.GetDerivative(p0, p1, p2, p3, t);

            Vector3 W = Vector3.Cross(camTransform.forward, dir);
            float radius = Mathf.Lerp(radius1, radius2, t);
            W = W.normalized * radius;

            int[] tmpTriangles;
            Vector3[] tmpVertecies =
                GenerateMeshRing(centerPoint, dir, W, width, vertecies.Count + mesh.vertexCount, out tmpTriangles, first);
            Vector2[] tmpUvs = GenerateUvRing(width, uvHeight);
            uvHeight++;
            vertecies.AddRange(tmpVertecies);
            triangles.AddRange(tmpTriangles);
            uvs.AddRange(tmpUvs);
            first = true;
        }

        List<Vector3> newVertecies = new List<Vector3>();
        List<int> newTriangles = new List<int>();
        List<Vector2> newUvs = new List<Vector2>();

        newVertecies.AddRange(mesh.vertices);
        newTriangles.AddRange(tmpMeshTri);
        newUvs.AddRange(tmpMeshUV);
        newVertecies.AddRange(vertecies);
        newTriangles.AddRange(triangles);
        newUvs.AddRange(uvs);
        mesh.vertices = newVertecies.ToArray();
        tmpMeshTri = newTriangles;
        tmpMeshUV = newUvs;
    }

    private Vector2[] GenerateUvRing(int num,
                                     int row)
    {
        Vector2[] res = new Vector2[num];
        for (int i = 0; i < num; i++)
        {
            res[i] = new Vector2(i, row);
        }
        return res;
    }

    private Vector3[] GenerateMeshRing(Vector3 centerPoint,
                                   Vector3 dir,
                                   Vector3 W,
                                   int num,
                                   int start,
                                   out int[] triangles,
                                   bool first = false)
    {
        // bool first = start < num;
        triangles = !first ? new int[0] : new int[num * 6];
        Vector3[] result = new Vector3[num];

        int _start = start;
        for (int i = 0; i < num; i++)
        {
            float rt = i / (float)num;

            Vector3 _W = Quaternion.AngleAxis(360 * rt, dir.normalized) * W;

            Vector3 point = centerPoint + _W;
            result[i] = point;
            if (!first) continue;

            int dl = start - num;
            int dr = (dl + 1) % num + _start - num;
            int ul = start;
            int ur = (ul + 1) % num + _start;

            int triIdx = i * 6;
            triangles[triIdx] = ur;
            triangles[triIdx + 1] = ul;
            triangles[triIdx + 2] = dl;
            triangles[triIdx + 3] = ur;
            triangles[triIdx + 4] = dl;
            triangles[triIdx + 5] = dr;
            start++;
        }
        return result;
    }

    Vector3[] FillVertexArrayByShader(Vector3[] v,
                              ref Vector2[] vuw,
                              int width,
                              float radius1,
                              float radius2,
                              Vector3 p0,
                              Vector3 p1,
                              Vector3 p2,
                              Vector3 p3)
    {

        float widthMult = 1 / (float)width;
        float heightMult = 1 / (float)height;

        Vector3 crossVec = camTransform.forward;

        vertexBuffer = new ComputeBuffer(v.Length, sizeof(float) * 3);

        vertexShader.SetBuffer(0, "vertecies", vertexBuffer);
        vertexShader.SetFloat("widthMult", widthMult);
        vertexShader.SetFloat("width", width);
        vertexShader.SetFloat("heightMult", heightMult);
        vertexShader.SetFloat("radius1", radius1);
        vertexShader.SetFloat("radius2", radius2);
        vertexShader.SetVector("p0", p0);
        vertexShader.SetVector("p1", p1);
        vertexShader.SetVector("p2", p2);
        vertexShader.SetVector("p3", p3);
        vertexShader.SetVector("crossVec", crossVec);

        vertexShader.Dispatch(0, v.Length / 32 + 32, 1, 1);

        vertexBuffer.GetData(v);
        return v;
    }


    Vector3[] FillVertexArray(Vector3[] v,
                              ref Vector2[] vuw,
                              int width,
                              float radius1,
                              float radius2,
                              Vector3 p0,
                              Vector3 p1,
                              Vector3 p2,
                              Vector3 p3)
    {

        for (int i = 0; i < v.Length; i++)
        {
            float t = (int)(i / width);
            float rt = i % width;
            Vector3 centerPoint = CatmullRom.GetPoint(p0, p1, p2, p3, t / (float)(height));
            Vector3 dir = CatmullRom.GetDerivative(p0, p1, p2, p3, t / (float)(height));
            float tmpt = t + 1;
            Vector3 tmpCenterPoint = CatmullRom.GetPoint(p0, p1, p2, p3, tmpt / (float)(height));
            // Debug.DrawRay(centerPoint, tmpCenterPoint - centerPoint, Color.red, 50);
            dir = (tmpCenterPoint - centerPoint).normalized;
            Vector3 W = Vector3.Cross(camTransform.forward, dir);
            float radius = Mathf.Lerp(radius1, radius2, t / (height - 1));
            W = W.normalized * radius;

            Vector3 W2 = Quaternion.AngleAxis(200, dir.normalized) * W;
            W = Quaternion.AngleAxis(360 * (rt / ((float)width)), dir.normalized) * W;

            Vector3 point = centerPoint + W;
            v[i] = point;
            vuw[i] = new Vector2(rt, t);
        }
        return v;
    }

    int[] FillTriangleArray(int width, int startIdx = 0)
    {
        List<int> res = new List<int>();
        for (int i = 0; i < height - 1; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int newJ = j + width * i;
                int ul = newJ + width;
                int ur = (j + 1) % width + (width * i) + width;
                int dr = (j + 1) % width + (width * i);
                int dl = newJ;

                ul += startIdx;
                ur += startIdx;
                dl += startIdx;
                dr += startIdx;

                res.Add(ur);
                res.Add(ul);
                res.Add(dl);

                res.Add(ur);
                res.Add(dl);
                res.Add(dr);
            }
        }
        return res.ToArray();
    }
}
