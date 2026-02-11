using System;
using UnityEngine;

public class Node : IComparable<Node>
{
    public Vector2Int gridPosition;
    public int gCost = 0; //与起始节点的距离
    public int hCost = 0; //与结束节点的距离
    public Node parentNode;

    public Node(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition;

        parentNode = null;
    }

    public int FCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        //若当前实例的 F 值（FCost）小于待比较节点（nodeToCompare）的 F 值，比较结果将小于 0
        //若当前实例的 F 值大于待比较节点的 F 值，比较结果将大于 0
        //若两者 F 值相等，比较结果将等于 0

        int compare = FCost.CompareTo(nodeToCompare.FCost);

        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }

        return compare;
    }
}
