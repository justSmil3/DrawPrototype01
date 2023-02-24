using System.Collections.Generic;
using UnityEngine;
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
            next[i].LogAll(prefix, idx + 1, postFix + " : " + i.ToString());
        }
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


}