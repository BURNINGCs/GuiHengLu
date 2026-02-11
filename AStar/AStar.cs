using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    //为房间生成一条从起始网格位置到结束网格位置的路径，并将移动步骤添加到返回的栈（Stack）中。
    //若未找到路径，则返回空值
    public static Stack<Vector3> BuildPath(Room room, Vector3Int startGridPosition, Vector3Int endGridPosition)
    {
        //通过下限调整位置
        startGridPosition -= (Vector3Int)room.templateLowerBounds;
        endGridPosition -= (Vector3Int)room.templateLowerBounds;

        //创建开放列表（open list）和关闭哈希集合（closed hashset）
        List<Node> openNodeList = new List<Node>();
        HashSet<Node> closedNodeHashSet = new HashSet<Node>();

        //创建用于路径寻路的网格节点（gridnodes）
        GridNodes gridNodes = new GridNodes(room.templateUpperBounds.x - room.templateLowerBounds.x + 1, room.templateUpperBounds.y -
        room.templateLowerBounds.y + 1);

        Node startNode = gridNodes.GetGridNode(startGridPosition.x, startGridPosition.y);
        Node targetNode = gridNodes.GetGridNode(endGridPosition.x, endGridPosition.y);

        Node endPathNode = FindShortestPath(startNode, targetNode, gridNodes, openNodeList, closedNodeHashSet, room.instantiatedRoom);

        if (endPathNode != null)
        {
            return CreatePathStack(endPathNode, room);
        }

        return null;
    }

    //寻找最短路径 —— 若找到路径则返回终点节点，否则返回空值
    private static Node FindShortestPath(Node startNode, Node targetNode, GridNodes gridNodes, List<Node> openNodeList, HashSet<Node> closedNodeHashSet,
        InstantiatedRoom instantiatedRoom)
    {
        //将起始节点添加到开放列表
        openNodeList.Add(startNode);

        //循环遍历开放节点列表，直至列表为空
        while (openNodeList.Count > 0)
        {
            //对列表进行排序
            openNodeList.Sort();

            //当前节点 = 开放列表中 F 值（fCost）最低的节点
            Node currentNode = openNodeList[0];
            openNodeList.RemoveAt(0);

            //若当前节点等于目标节点，则寻路结束
            if (currentNode == targetNode)
            {
                return currentNode;
            }

            //将当前节点添加到关闭列表
            closedNodeHashSet.Add(currentNode);

            //计算当前节点每个相邻节点的 F 值
            EvaluateCurrentNodeNeighbours(currentNode, targetNode, gridNodes, openNodeList, closedNodeHashSet, instantiatedRoom);
        }

        return null;
    }

    //创建一个包含移动路径的三维向量栈（Stack<Vector3>）
    private static Stack<Vector3> CreatePathStack(Node targetNode, Room room)
    {
        Stack<Vector3> movementPathStack = new Stack<Vector3>();

        Node nextNode = targetNode;

        //获取网格单元格的中点
        Vector3 cellMidPoint = room.instantiatedRoom.grid.cellSize * 0.5f;
        cellMidPoint.z = 0f;

        while (nextNode != null)
        {
            //将网格位置转换为世界坐标
            Vector3 worldPosition = room.instantiatedRoom.grid.CellToWorld(new Vector3Int(nextNode.gridPosition.x + room.templateLowerBounds.x,
            nextNode.gridPosition.y + room.templateLowerBounds.y, 0));

            //将世界坐标设置为网格单元格的中心
            worldPosition += cellMidPoint;

            movementPathStack.Push(worldPosition);

            nextNode = nextNode.parentNode;
        }

        return movementPathStack;
    }

    //评估相邻节点
    private static void EvaluateCurrentNodeNeighbours(Node currentNode, Node targetNode, GridNodes gridNodes, List<Node> openNodeList, 
        HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        Vector2Int currentNodeGridPosition = currentNode.gridPosition;

        Node validNeighbourNode;

        //遍历所有方向（上下左右 / 斜向等）
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                validNeighbourNode = GetValidNodeNeighbour(currentNodeGridPosition.x + i, currentNodeGridPosition.y + j, gridNodes, closedNodeHashSet, instantiatedRoom);

                if (validNeighbourNode != null)
                {
                    //计算相邻节点的新 G 值（gcost）
                    int newCostToNeighbour;

                    //获取移动惩罚值
                    //不可行走的路径值为 0。默认移动惩罚值在 “设置（Settings）” 中指定，
                    //并适用于其他网格方格
                    int movementPenaltyForGridSpace = instantiatedRoom.aStarMovementPenalty[validNeighbourNode.gridPosition.x,
                        validNeighbourNode.gridPosition.y];

                    newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, validNeighbourNode) + movementPenaltyForGridSpace;

                    bool isValidNeighbourNodeInOpenList = openNodeList.Contains(validNeighbourNode);

                    if (newCostToNeighbour < validNeighbourNode.gCost || !isValidNeighbourNodeInOpenList)
                    {
                        validNeighbourNode.gCost = newCostToNeighbour;
                        validNeighbourNode.hCost = GetDistance(validNeighbourNode, targetNode);
                        validNeighbourNode.parentNode = currentNode;

                        if (!isValidNeighbourNodeInOpenList)
                        {
                            openNodeList.Add(validNeighbourNode);
                        }
                    }
                }
            }
        }
    }

    //返回节点 A 与节点 B 之间的整数距离
    private static int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        int dstY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY); //使用 10 而非 1 作为基础单位，14 是勾股定理的近似值（对应SQRT(10*10 + 10*10)）—— 用于避免使用浮点数计算

        return 14 * dstX + 10 * (dstY - dstX);
    }

    //评估位于相邻节点 X 坐标、相邻节点 Y 坐标的相邻节点，使用指定的网格节点、关闭节点哈希集合和已实例化的房间。
    //若节点无效，则返回空值
    private static Node GetValidNodeNeighbour(int neighbourNodeXPosition, int neighbourNodeYPosition, GridNodes gridNodes, HashSet<Node> closedNodeHashSet,
        InstantiatedRoom instantiatedRoom)
    {
        //若相邻节点位置超出网格范围，则返回空值
        if (neighbourNodeXPosition >= instantiatedRoom.room.templateUpperBounds.x - instantiatedRoom.room.templateLowerBounds.x || neighbourNodeXPosition < 0
            || neighbourNodeYPosition >= instantiatedRoom.room.templateUpperBounds.y - instantiatedRoom.room.templateLowerBounds.y || neighbourNodeYPosition < 0)
        {
            return null;
        }

        //获取相邻节点
        Node neighbourNode = gridNodes.GetGridNode(neighbourNodeXPosition, neighbourNodeYPosition);

        //检查该位置是否存在障碍物
        int movementPenaltyForGridSpace = instantiatedRoom.aStarMovementPenalty[neighbourNodeXPosition, neighbourNodeYPosition];

        //检查该位置是否存在可移动障碍物
        int itemObstacleForGridSpace = instantiatedRoom.aStarItemObstacles[neighbourNodeXPosition, neighbourNodeYPosition];

        //若相邻节点是障碍物，或已在关闭列表中，则跳过该节点
        if (movementPenaltyForGridSpace == 0 || itemObstacleForGridSpace == 0 || closedNodeHashSet.Contains(neighbourNode))
        {
            return null;
        }
        else
        {
            return neighbourNode;
        }
    }
    //private static Node GetValidNodeNeighbour(int neighbourNodeXPosition, int neighbourNodeYPosition, GridNodes gridNodes, HashSet<Node> closedNodeHashSet,
    //InstantiatedRoom instantiatedRoom)
    //{
    //    // 获取网格尺寸
    //    int gridWidth = instantiatedRoom.aStarMovementPenalty.GetLength(0);
    //    int gridHeight = instantiatedRoom.aStarMovementPenalty.GetLength(1);

    //    // 检查边界
    //    if (neighbourNodeXPosition < 0 || neighbourNodeXPosition >= gridWidth ||
    //        neighbourNodeYPosition < 0 || neighbourNodeYPosition >= gridHeight)
    //    {
    //        return null;
    //    }

    //    Node neighbourNode = gridNodes.GetGridNode(neighbourNodeXPosition, neighbourNodeYPosition);

    //    // 检查障碍物
    //    int movementPenalty = instantiatedRoom.aStarMovementPenalty[neighbourNodeXPosition, neighbourNodeYPosition];
    //    int itemObstacle = instantiatedRoom.aStarItemObstacles[neighbourNodeXPosition, neighbourNodeYPosition];

    //    if ((movementPenalty == 0 && itemObstacle == 0) || closedNodeHashSet.Contains(neighbourNode))
    //    {
    //        return null;
    //    }

    //    return neighbourNode;
    //}
}
