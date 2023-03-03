using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(LineRenderer))]
public class CatmullTree : MonoBehaviour
{
    public List<Vector3> testPoints = new List<Vector3>();
    [Min(1)]
    public int resolution = 1;
    [Min(1)]
    public int circularResolution = 1;
    [Range(0, 1)]
    public float a = 0.55f;

    public SplineNode tree;
    public SplineNode lastAddedNode;
    LineRenderer debugLine;

    //debug code 
    private void OnDrawGizmos()
    {
        if (tree == null)
            return;
        SplineNode[] wholeTree = tree.GetTree();
        for(int i = 0; i < wholeTree.Length; i++)
        {
            Gizmos.DrawSphere(wholeTree[i].point, .03f);

        }
    }

    private void Awake()
    {
        debugLine = GetComponent<LineRenderer>();
        SetupTree();
    }

    private void SetupTree()
    {
        tree = new SplineNode(Vector3.zero);
        SplineNode s2 = new SplineNode(Vector3.up * 1);
        tree.SetNext(s2);
        SplineNode s3 = new SplineNode(Vector3.up * 2);
        s2.SetNext(s3);
        SplineNode s4 = new SplineNode(Vector3.up * 3);
        s3.SetNext(s4);
        SplineNode s5 = new SplineNode(Vector3.up * 4);
        s4.SetNext(s5);
        SplineNode s6 = new SplineNode(Vector3.up * 5);
        s5.SetNext(s6);
    }


    private void SetNumberOfPoints()
    {
        debugLine.positionCount = resolution * (testPoints.Count - 1);
    }

    private void IterateOverObjects()
    {
        int positionCount = testPoints.Count;
        Vector3 p0 = testPoints[0] +
                    (testPoints[0] - testPoints[1]);
        SetPositions(p0, testPoints[0], testPoints[1], testPoints[2], 0);
        for(int i = 1; i < positionCount - 2; i++) 
        {
            SetPositions(testPoints[i - 1], testPoints[i], testPoints[i + 1], testPoints[i + 2], resolution * i);
        }
        Vector3 p3 = testPoints[positionCount - 1] +
                    (testPoints[positionCount - 1] - testPoints[positionCount - 2]);
        SetPositions(testPoints[positionCount - 3],
                     testPoints[positionCount - 2],
                     testPoints[positionCount - 1],
                     p3, resolution * (positionCount - 2));
    }

    private void SetPositions(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int startIdx)
    {
        for(int i = 0; i < resolution; i++)
        {
            Vector3 newPosition = CatmullRom.GetPoint(p0, p1, p2, p3, (float)i / (float)resolution, a);
            debugLine.SetPosition(startIdx + i, newPosition);
        }
    }

    public SplineNode AddPoint(Vector3 position)
    {
        SplineNode prev;
        if (!TryGetClosestPoint(position, out prev))
            return null;
        return AddPoint(position, prev);
    }

    public SplineNode AddPoint(Vector3 pos, SplineNode node)
    {
        SplineNode newNode = new SplineNode(pos);
        node.SetNext(newNode);
        lastAddedNode = newNode;
        return newNode;
    }

    public void MoveLastPoint(Vector3 pos)
    {
        if (lastAddedNode == null) return;
        lastAddedNode.point = pos;
    }

    public void ResetLastPoint()
    {
        lastAddedNode = null;
    }

    private void GetVertexPositions(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        for (int i = 0; i < resolution; i++)
        {
            Vector3 newPosition = CatmullRom.GetPoint(p0, p1, p2, p3, (float)i / (float)resolution, a);
        }
    }

    public bool TryGetClosestPoint(Vector3 point, out SplineNode closestPoint)
    {
        GetSmallestDistance(point, out closestPoint);
        return (closestPoint != null);
    }


    private float GetSmallestDistance(Vector3 pos, out SplineNode result, SplineNode node = null, SplineNode prev = null)
    {
        if (node == null)
            node = tree;

        float dist = GetDistToPoint(pos, node, prev);
        result = node;

        for(int i = 0; i < node.GetNumberOfConnections(); i++)
        {
            SplineNode tmpRes;
            float tmpDist = GetSmallestDistance(pos, out tmpRes, node.GetNextNode(i), node);

            if(tmpDist < dist)
            {
                result = tmpRes;
                dist = tmpDist;
            }
        }

        return dist;
    }
        
    private float GetDistToPoint(Vector3 pos, SplineNode node, SplineNode prev = null)
    {
        Vector3 dir = Vector3.zero;
        if (node.GetNumberOfConnections() <= 0)
        {
            if(prev == null)
                return float.PositiveInfinity;
            dir = (node.point - prev.point).normalized;
        }
        else
        {
            dir = (node.GetNextNode().point - node.point).normalized;
        }
        Vector3 dir2 = pos - node.point;
        bool bDir = IsDirectionCorrect(dir, dir2.normalized);
        float result = bDir ? dir2.magnitude : float.PositiveInfinity;
        return result;
    }

    private bool IsDirectionCorrect(Vector3 dir1, Vector3 dir2)
    {
        float dot = Vector3.Dot(dir1, dir2);
        return dot >= 0;
    }

    private bool IsDirectionCorrect(Vector3 start, Vector3 end1, Vector3 end2)
    {
        Vector3 dist0 = end1 - start;
        Vector3 dist1 = end2 - start;
        float dot = Vector3.Dot(dist0, dist1);
        return dot >= 0;
    }

    // insert is missing index based insert
    // TODO do add and insert into different functinons
    public void AddPoint(SplineNode preNode, Vector3 pos, bool bInsert = false)
    {
        SplineNode newNode = new SplineNode(pos);
        if(bInsert)
        {
            preNode.SetNext(newNode);
            return;
        }
        // TODO this all after this can be moved to the insert point function once working on it 
        // TODO fix the conflict situation that this is not always on index 0
        SplineNode nextNode = preNode.GetNextNode();
        preNode.ReplaceNext(newNode);
        newNode.SetNext(nextNode);
    }
    public void InsertPoint()
    {
        throw new NotImplementedException();
    }
}
