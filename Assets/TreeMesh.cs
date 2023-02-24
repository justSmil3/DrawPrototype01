using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TreeMesh : MonoBehaviour
{
    public CatmullTree tree;
    public float maxRadius = 3;
    public float verteciemult = 5;
    public float ra = .1f;
    private Mesh mesh;

    private float resolution;
    private Vector3[] Tvertecies;
    
    // private Vector2[] uvs;
    // private int[] triangles;


    private void OnDrawGizmos()
    {
        if (Tvertecies != null)
            for (int i = 0; i < Tvertecies.Length; i++)
            {
                Gizmos.DrawSphere(Tvertecies[i], 0.01f);
            }
    }

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        resolution = Mathf.Sqrt(verteciemult);
    }

    bool once = true;
    private void FixedUpdate()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh; 
        GenerateMeshSpline();
    }

    private void GenerateMeshSpline(SplineNode current = null, SplineNode prev = null, float vigor = 1, int maxLength = -1)
    {
        if (current == null)
        {
            current = tree.tree;
            maxLength = current.GetBranchSize();
        }
        int numberOfConnections = current.GetNumberOfConnections();
        if (numberOfConnections <= 0)
            return;

        maxLength = current.GetBranchSize() > maxLength ? current.GetBranchSize() : maxLength;

        float tmpVigor = vigor * ((current.GetBranchSize() - 1) / (float)maxLength);
        vigor *= (current.GetBranchSize() / (float)maxLength);

        for (int i = 0; i < numberOfConnections; i++)
        {
            //maxLength = i > 0 ? current.GetBranchSize() : maxLength;

            SplineNode nextNode = current.GetNextNode(i);
            Vector3[] points;
            bool first = GenerateSplinePoints(prev, current, nextNode, out points, i);
            float radius1 = vigor * ra;
            float radius2 = tmpVigor * ra;
            GenerateMeshSegment(radius1,
                                radius2,
                                points[0],
                                points[1],
                                points[2],
                                points[3],
                                first);
            GenerateMeshSpline(nextNode, current, vigor, maxLength);
        }
    }

    private bool GenerateSplinePoints(SplineNode prev, SplineNode current, SplineNode next, out Vector3[] points, int i)
    {
        bool result = true;
        points = new Vector3[4];
        if(prev == null)
        {
            result = false;
            points[0] = current.point + (current.point - next.point);
        }
        else
        {
            points[0] = prev.point;
        }
        if(next.GetNumberOfConnections() <= 0)
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

    private void GenerateMeshSegment(float radius1,
                                     float radius2,
                                     Vector3 p0,
                                     Vector3 p1,
                                     Vector3 p2,
                                     Vector3 p3,
                                     bool addTiangels = true)
    {
        Vector3 p0p1 = p1 - p0;
        Vector3 p1p2 = p2 - p1;
        Vector3 p2p3 = p3 - p2;
        Vector3 p0p3 = p0p1 + p1p2 + p2p3;
        float length = p0p3.magnitude;



        int width = (int)(ra * verteciemult);
        int heigth = System.Math.Max(2, (int)(length * resolution * ra));
        //int numtriangles = width * (heigth - 1) * 6;
        int numvertecies = (int)(width * heigth);
        Vector3[] vertecies = new Vector3[numvertecies];
        Vector2[] uvs = new Vector2[numvertecies];
        int[] triangles;

        vertecies = FillVertexArray(vertecies, ref uvs, width, heigth, radius1, radius2, p0, p1, p2, p3);
        Tvertecies = vertecies;
        triangles = FillTriangleArray(width, heigth, mesh.vertexCount);

        List<Vector3> newVertecies = new List<Vector3>();
        List<Vector2> newUVs = new List<Vector2>();
        List<int> newTriangles = new List<int>();

        newVertecies.AddRange(mesh.vertices);
        newUVs.AddRange(mesh.uv);
        newUVs.AddRange(uvs);
        newTriangles.AddRange(mesh.triangles);
        // startindex is related to branchindex, test what it actually does
        // TODO left of here 
        if (addTiangels)
        {
            for (int i = 0; i < width; i++)
            {
                int ul = newVertecies.Count + width + i;
                int ur = newVertecies.Count + width + ((i + 1) % width);
                int dl = newVertecies.Count - width + i;
                int dr = newVertecies.Count - width + ((i + 1) % width);

                newTriangles.Add(ur);
                newTriangles.Add(ul);
                newTriangles.Add(dl);

                newTriangles.Add(ur);
                newTriangles.Add(dl);
                newTriangles.Add(dr);
            }
        }
        newVertecies.AddRange(vertecies);
        newTriangles.AddRange(triangles);

        mesh.vertices = newVertecies.ToArray();
        // Tvertecies = mesh.vertices;
        mesh.uv = newUVs.ToArray();
        mesh.triangles = newTriangles.ToArray();
    }

    Vector3[] FillVertexArray(Vector3[] v,
                              ref Vector2[] vuw,
                              int width,
                              int height,
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
            Vector3 dir = CatmullRom.GetDerivative(p0, p1, p2, p3, t / (height - 1));
            Vector3 W = Vector3.Cross(Vector3.right, dir);
            float radius = Mathf.Lerp(radius1, radius2, t / (height - 1));
            W = W.normalized * radius;
            Vector3 centerPoint = CatmullRom.GetPoint(p0, p1, p2, p3, t / (height - 1));
            float angleDivider = rt;

            W = Quaternion.AngleAxis(360 * (rt / ((float)width - 1)), dir.normalized) * W;
            Vector3 point = centerPoint + W;
            v[i] = point;
            vuw[i] = new Vector2(rt, t);
        }
        return v;
    }

    int[] FillTriangleArray(int width, int height, int startIdx = 0)
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
