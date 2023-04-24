using System.Collections.Generic;
using UnityEngine;

public static class GlobalVar
{
    public static Vector3 NULLVEC = new Vector3(-1000, -1000, -1000);
}
public class SplineNode
{
    private List<SplineNode> next;
    public Vector3 point = Vector3.zero;

    public SplineNode()
    {
        next = new List<SplineNode>();
        point = Vector3.zero;
    }
    public SplineNode(Vector3 position)
    {
        next = new List<SplineNode>();
        point = position;
    }

    public void scalePoint(List<SplineNode> terminators, Vector3 scaler)
    {
        if (terminators.Contains(this)) return;
        point.x /= scaler.x;
        point.y /= scaler.y;
        point.z /= scaler.z;
        for (int i = 0; i < next.Count; i++)
        {
            next[i].scalePoint(terminators, scaler);
        }
    }

    public void CopyValues(SplineNode _node)
    {
        next = _node.next;
        point = _node.point;
    }


    public void LogAll(string prefix = "", int idx = 0, string postFix = " >> ")
    {
        string perfix = idx == 0 ? "__root__: " : "__" + idx.ToString() + "__: ";
        Debug.Log(prefix + perfix + point + postFix);
        for (int i = 0; i < next.Count; i++)
        {
            Debug.DrawRay(point, next[i].point - point, Color.red, 50);
            next[i].LogAll(prefix, idx + 1, postFix + " : " + i.ToString());
        }
    }

    public void DrawTree(float duration = - 1)
    {
        for (int i = 0; i < next.Count; i++)
        {
            if (duration > 0)
                Debug.DrawRay(point, next[i].point - point, Color.red, duration);
            else
                Debug.DrawRay(point, next[i].point - point, Color.red);
            next[i].DrawTree();
        }
    }

    public List<Vector3> ConvertBranch2VectorList()
    {
        List<Vector3> result = new List<Vector3>();

        result.Add(point);

        for(int i = 0; i < next.Count; i++)
        {
            result.AddRange(next[i].ConvertBranch2VectorList());
        }
        if (next.Count == 0)
            result.Add(GlobalVar.NULLVEC);

        return result;
    }

    public List<Vector3> ConvertBranch2VectorList(bool testDebug)
    {
        List<Vector3> result = new List<Vector3>();

        if(next.Count == 0)
        {
            result.Add(point);
            result.Add(GlobalVar.NULLVEC);
        }

        for (int i = 0; i < next.Count; i++)
        {
            result.Add(point);
            result.AddRange(next[i].ConvertBranch2VectorList(true));
        }

        return result;
    }

    public SplineNode[] GetTree(List<SplineNode> nodes = null)
    {
        if(nodes == null)
            nodes = new List<SplineNode>();
        nodes.Add(this);
        for(int i = 0; i < next.Count; i++)
        {
            next[i].GetTree(nodes);
        }
        return nodes.ToArray();
    }

    public SplineNode GetNextNode(int idx = 0)
    {
        if (next.Count <= idx)
            return null;
        else return next[idx];
    }

    public int GetBranchSize(int idx = 0)
    {
        return next.Count <= idx ? 1 : 1 + next[idx].GetBranchSize();
    }

    public int GetTreeSize(bool addEnds = false)
    {
        int result = 1;
        if (next.Count == 0 && addEnds) result++;
        for (int i = 0; i < next.Count; i++)
        {
            result += next[i].GetTreeSize();
        }
        return result;
    }



    public int GetNumberOfConnections()
    {
        return next.Count;
    }

    public void SetNext(SplineNode node, int idx = -1)
    {
        if (idx < 0 && idx < next.Count)
            next.Add(node);
        else
            next.Insert(idx, node);
    }

    public void SetNext(SplineNode[] stemp)
    {
        SplineNode current = this;
        for (int i = 0; i < stemp.Length; i++)
        {
            current.SetNext(stemp[i]);
            current = stemp[i];
        }
    }

    public void ReplaceNext(SplineNode node, int idx = 0)
    {
        if(idx < next.Count)
            next[idx] = node;
    }

    public void ClearNext()
    {
        next = new List<SplineNode>();
    }

    public SplineNode GetSplineNode(int idx, int branch = 0)
    {
        if (idx == 0) return this;
        idx--;
        if (next.Count <= branch) return null;
        return next[branch].GetSplineNode(idx, 0);
    }

    public SplineNode GetLastSplineNode()
    {
        if (next.Count == 0) return this;
        return next[0].GetLastSplineNode();
    }

    public SplineNode GetLastSplineNode(int startIdx)
    {
        if (next.Count <= startIdx) return this;
        return next[startIdx].GetLastSplineNode();
    }

    public void DeleteSplineTree()
    {
        for (int i = 0; i < next.Count; i++)
        {
            next[i].DeleteSplineTree();
        }
        next = new List<SplineNode>();
    }

    public void DeleteSplineTree(List<SplineNode> terminator)
    {
        if (terminator.Contains(this)) return;
        for (int i = 0; i < next.Count; i++)
        {
            next[i].DeleteSplineTree();
        }
        next = new List<SplineNode>();
    }

    public bool CheckForEndNode()
    {
        return next.Count == 0;
    }

    public int GetNumbersOfBranchesAhead()
    {
        int result = next.Count > 1 ? 1 : 0;
        for (int i = 0; i < next.Count; i++)
        {
            result += next[i].GetNumbersOfBranchesAhead();
        }
        return result;
    }

    public int GetIndexInBranch(SplineNode node, int currentindex = 0)
    {
        if (node == this) return currentindex;
        for (int i = 0; i < next.Count; i++)
        {
            currentindex = next[i].GetIndexInBranch(node, currentindex + 1);
        }
        return currentindex;
    }

    public List<float> GetVigorMultArray(int idx = 0, float maxBranchSize = 0, float startMult = 1)
    {
        if(idx == 0)
        {
            maxBranchSize = GetBranchSize()-1;
        }
        List<float> result = new List<float>();
        float vigor = ((float)GetBranchSize() - 1) / maxBranchSize * startMult;
        result.Add(vigor);
        if(next.Count > 0)
        {
            result.AddRange(next[0].GetVigorMultArray(idx+1, maxBranchSize, startMult));
        }
        else
        {
            result.Add(-1f); 
        }
        for(int i = 1; i < next.Count; i++)
        {
            result.AddRange(next[i].GetVigorMultArray(0, 0, vigor));
        }
        return result;
    }


    public List<float> GetVigorMultArrayTmp(int idx = 0, float maxBranchSize = 0, float startMult = 1)
    {
        if (idx == 0)
        {
            maxBranchSize = GetBranchSize() - 1;
        }
        List<float> result = new List<float>();
        float vigor = ((float)GetBranchSize() - 1) / maxBranchSize * startMult;
        if (next.Count > 0)
        {
            result.Add(vigor);
            result.AddRange(next[0].GetVigorMultArrayTmp(idx + 1, maxBranchSize, startMult));
        }
        else
        {
            result.Add(vigor);
            result.Add(-1f);
        }
        for (int i = 1; i < next.Count; i++)
        {
            result.Add(vigor);
            result.AddRange(next[i].GetVigorMultArrayTmp(0, 0, vigor));
        }
        return result;
    }

}