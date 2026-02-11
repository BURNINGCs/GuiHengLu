using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;
    private GUIStyle roomNodeSelectedStyle;
    private static RoomNodeGraphSO currentRoomNodeGraph;

    private Vector2 graphOffset;
    private Vector2 graphDrag;

    private RoomNodeSO currentRoomNode = null;
    private RoomNodeTypeListSO roomNodeTypeList;

    //节点布局值
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    //连接线的值
    private const float connectingLineWidth = 3f;

    private const float connectingLineArrowSize = 6f;

    //网格空间
    private const float gridLarge = 100f;
    private const float gridSmall = 25f;

    [MenuItem("Room Node Graph Editor",menuItem ="Window/Dungeon Editor/Room Node Graph Editor")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    private void OnEnable()
    {
        //订阅检查器选择更改事件
        Selection.selectionChanged += InspectorSelectionChanged;

        //定义节点布局类型
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        //定义选中节点的样式
        roomNodeSelectedStyle = new GUIStyle();
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        roomNodeSelectedStyle.normal.textColor = Color.white;
        roomNodeSelectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeSelectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        //加载房间节点类型表
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable()
    {
        //取消订阅检查器选择更改事件
        Selection.selectionChanged -= InspectorSelectionChanged;
    }


    //如果在检视面板中双击房间节点图可脚本化对象资源，就打开房间节点图编辑器窗口
    [OnOpenAsset(0)]    //需要命名空间 UnityEditor.Callbacks
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;

        if(roomNodeGraph != null)
        {
            OpenWindow();

            currentRoomNodeGraph = roomNodeGraph;

            return true;
        }
        return false;
    }



    //在编辑器窗口绘制内容
    private void OnGUI()
    {
        //如果已选中 RoomNodeGraphSO 类型的可脚本化对象，则进行处理
        if (currentRoomNodeGraph != null)
        {
            //绘制背景网格
            DrawBackgroundGrid(gridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(gridLarge, 0.3f, Color.gray);

            //如果正在拖动，则绘制线条
            DrawDraggedLine();

            //处理事件
            ProcessEvents(Event.current);

            //绘制房间节点之间的连接
            DrawRoomConnections();

            //绘制房间节点
            DrawRoomNodes();
        }

        if(GUI.changed)
        {
            Repaint();
        }
    }

    //为房间节点图编辑器绘制背景网格
    private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
    {
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        graphOffset += graphDrag * 0.5f;

        Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0);

        for (int i = 0; i < verticalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + gridOffset, new Vector3(gridSize * i, position.height + gridSize, 0f) +
                gridOffset);
        }

        for (int j = 0; j < horizontalLineCount; j++)
        {
            Handles.DrawLine(new Vector3(-gridSize, gridSize * j, 0) + gridOffset, new Vector3(position.width + gridSize, gridSize * j, 0f) +
                gridOffset);
        }

        Handles.color = Color.white;
    }

    private void DrawDraggedLine()
    {
        if (currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            // 从节点绘制线条到线条位置
            Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition,
                currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, Color.white, null, connectingLineWidth);
        }
    }

    private void ProcessEvents(Event currentEvent)
    {
        //重置图表拖动状态
        graphDrag = Vector2.zero;

        //获取鼠标当前悬停的房间节点（若节点为空或未处于拖拽状态）
        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        //如果鼠标未悬停在房间节点上或者如果我们已经开始从一个房间节点拖动一条线，则处理图表事件
        if (currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom != null) 
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        //否则处理房间节点事件
        else
        {
            //处理房间节点事件
            currentRoomNode.ProcessEvents(currentEvent);
        }



    }

    //检查鼠标是否在一个房间节点上 - 如果是，则返回房间节点，否则返回null
    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        for (int i = currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--)
        {
            if (currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return currentRoomNodeGraph.roomNodeList[i];
            }
        }

        return null;
    }

    //处理房间节点图事件
    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch(currentEvent.type)
        {
            //处理鼠标下降事件
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;

            //处理鼠标向上事件
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;

            //处理鼠标拖动事件
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    //处理房间节点图上的鼠标按下事件（非节点上方）
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        //处理图表上的右键鼠标按下事件（显示上下文菜单）
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
        //处理图表上的鼠标左键按下事件
        else if (currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    //显示上下文菜单的方法
    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Select All Room Nodes"), false, SelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete Selected Room Node Links"), false, DeleteSelectedRoomNodeLinks);
        menu.AddItem(new GUIContent("Delete Selected Room Node"), false, DeleteSelectedRoomNodes);

        menu.ShowAsContext();
    }

    //在鼠标位置创建一个房间节点
    private void CreateRoomNode(object mousePositionObject)
    {
        //如果当前节点图为空，则首先添加入口房间节点
        if(currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            CreateRoomNode(new Vector2(200f, 200f), roomNodeTypeList.list.Find(x => x.isEntrance));
        }

        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isNone));
    }

    //在鼠标位置创建房间节点 - 重载方法，还可传入 RoomNodeType 参数
    private void CreateRoomNode(object mousePositionObject,RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        //创建房间节点可脚本化对象资源
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        //将房间节点添加到当前房间节点图的房间节点列表中
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        //设置房间节点的值
        roomNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);

        //将房间节点添加到房间节点图可脚本化对象资源数据库中
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);

        AssetDatabase.SaveAssets();

        //刷新图表节点字典
        currentRoomNodeGraph.OnValidate();
    }

    //删除选中的房间节点
    private void DeleteSelectedRoomNodes()
    {
        Queue<RoomNodeSO> roomNodeDeletionQueue = new Queue<RoomNodeSO>();

        //遍历所有节点
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && !roomNode.roomNodeType.isEntrance)
            {
                roomNodeDeletionQueue.Enqueue(roomNode);

                //遍历子房间节点 ID
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    //获取子房间节点
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(childRoomNodeID);

                    if (childRoomNode != null)
                    {
                        //从子房间节点中移除父节点 ID
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }

                //遍历父房间节点 ID
                foreach (string parentRoomNodeID in roomNode.parentRoomNodeIDList)
                {
                    //获取父节点
                    RoomNodeSO parentRoomNode = currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);

                    if (parentRoomNode != null)
                    {
                        //从父节点中移除子节点 ID
                        parentRoomNode.RemoveChildRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }
        //删除队列中的房间节点
        while (roomNodeDeletionQueue.Count > 0)
        {
            //从队列中获取房间节点
            RoomNodeSO roomNodeToDelete = roomNodeDeletionQueue.Dequeue();

            //从字典中移除节点
            currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeToDelete.id);

            //从列表中移除节点
            currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);

            //从资源数据库中移除节点
            DestroyImmediate(roomNodeToDelete, true);

            //保存资源数据库
            AssetDatabase.SaveAssets();
        }
    }
    

    //删除选中房间节点之间的链接
    private void DeleteSelectedRoomNodeLinks()
    {
        //遍历所有房间节点
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && roomNode.childRoomNodeIDList.Count > 0)
            {
                for (int i = roomNode.childRoomNodeIDList.Count - 1; i >= 0; i--)
                {
                    //获取子房间节点
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(roomNode.childRoomNodeIDList[i]);

                    //如果子房间节点处于选中状态
                    if (childRoomNode != null && childRoomNode.isSelected)
                    {
                        //从父房间节点中移除子节点 ID
                        roomNode.RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                        //从子房间节点中移除父节点 ID
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        //清除所有选中的房间节点
        ClearAllSelectedRoomNodes();
    }

    //清除所有房间节点的选中状态
    private void ClearAllSelectedRoomNodes()
    {
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if(roomNode.isSelected)
            {
                roomNode.isSelected = false;

                GUI.changed = true;
            }
        }
    }

    //选择所有房间节点
    private void SelectAllRoomNodes()
    {
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.isSelected = true;
        }
        GUI.changed = true;
    }

    //处理鼠标向上事件
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        //如果释放右键且当前正在拖动一条线
        if (currentEvent.button == 1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            //检查是否悬停在房间节点上
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);

            if(roomNode != null)
            {
                //如果是，则在可添加的情况下将其设为父房间节点的子节点
                if(currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
                {
                    //在子房间节点中设置父 ID
                    roomNode.AddParentRoomNodeIDToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
                }
            }

            ClearLineDrag();
        }
    }

    //处理鼠标拖动事件
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        //处理鼠标右键按下拖动事件 - draw line
        if (currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
        //处理左键拖动事件 - 拖动节点图表
        else if(currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent.delta);
        }
    }

    //处理鼠标右键拖动事件 - draw line
    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        if (currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            DragConnectingLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    //处理鼠标左键拖动事件 - 拖动房间节点图表
    private void ProcessLeftMouseDragEvent(Vector2 dragDelta)
    {
        graphDrag = dragDelta;

        for (int i = 0; i < currentRoomNodeGraph.roomNodeList.Count; i++) 
        {
            currentRoomNodeGraph.roomNodeList[i].DragNode(dragDelta);
        }

        GUI.changed = true;
    }

    //从房间节点拖动连接线
    private void DragConnectingLine(Vector2 delta)
    {
        currentRoomNodeGraph.linePosition += delta;
    }

    //清除从房间节点发起的线条拖拽
    private void ClearLineDrag()
    {
        currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    //在图表窗口中绘制房间节点之间的连接
    private void DrawRoomConnections()
    {
        //遍历所有房间节点
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.childRoomNodeIDList.Count > 0) 
            {
                //遍历子房间节点
                foreach(string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    //从字典中获取子房间节点
                    if(currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomNodeID))
                    {
                        DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);
                        GUI.changed = true;
                    }
                }
            }
        }
    }

    //在父房间节点和子房间节点之间绘制连接线
    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        //获取线条的起点和终点位置
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

        //计算中点
        Vector2 midPosition = (endPosition + startPosition) / 2f;

        //从线条的起点到终点的向量
        Vector2 direction = endPosition - startPosition;

        //计算从中点出发的标准化垂直位置
        Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;

        //计算箭头的中点偏移位置
        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;

        //绘制箭头
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, connectingLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, connectingLineWidth);

        //绘制线条
        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectingLineWidth);
        GUI.changed = true;
    }

    //绘制房间节点在图形窗口中
    private void DrawRoomNodes()
    {
        //循环浏览所有的房间节点以及画出它们
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if(roomNode.isSelected)
            {
                roomNode.Draw(roomNodeSelectedStyle);
            }
            else
            {
                roomNode.Draw(roomNodeStyle);
            }
        }

        GUI.changed = true;
    }

    //检查器中的选择发生变化
    private void InspectorSelectionChanged()
    {
        RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            currentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
        }
    }
}
