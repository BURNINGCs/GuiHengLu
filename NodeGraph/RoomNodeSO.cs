using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id;//房间节点ID
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>();//父房间节点的ID列表
    [HideInInspector] public List<string> childRoomNodeIDList = new List<string>();//子房间节点的ID列表
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;//房间节点图
    public RoomNodeTypeSO roomNodeType;//房间节点类型
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;//房间节点类型列表

    #region Editor Code

#if UNITY_EDITOR

    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;//节点是否正在被左键拖拽
    [HideInInspector] public bool isSelected = false;//节点是否被选中

    public void Initialise(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)//初始化节点
    { 
        this.rect = rect;
        this.id=Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        //加载房间节点类型列表
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public void Draw(GUIStyle nodeStyle)//使用节点样式绘制节点
    {
        GUILayout.BeginArea(rect, nodeStyle);

        EditorGUI.BeginChangeCheck();//开始检测弹出菜单选择变化的区域
        // 如果房间节点有父节点或者是入口类型，则显示标签，否则显示弹出菜单
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);// 显示一个不可更改的标签
        }
        else
        {
            //显示一个弹出菜单
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);//查找当前房间节点类型在列表中的索引
            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());//参数1：字段前的标签（可选），参数2：字段显示的选项的索引，参数3：弹出菜单所示选项的数组

            roomNodeType = roomNodeTypeList.list[selection];//更新房间节点类型

            //如果房间类型选择发生变化，可能导致子连接无效
            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor
                || !roomNodeTypeList.list[selected].isCorridor && roomNodeTypeList.list[selection].isCorridor
                || !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom)
            {
                // 如果房间节点类型已更改且它已经有子节点，则删除父子链接，需要重新验证
                if (childRoomNodeIDList.Count > 0)
                {
                    for (int i = childRoomNodeIDList.Count - 1; i >= 0; i--)
                    {
                        RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);
                        if (childRoomNode != null)
                        {
                            RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);
                            childRoomNode.RemoveParentRoomNodeIDFromRoomNode(id);
                        }
                    }
                }
            }
        }

        if (EditorGUI.EndChangeCheck())//检测到变化则标记对象需要保存
            EditorUtility.SetDirty(this);

        GUILayout.EndArea();
    }

    public string[] GetRoomNodeTypesToDisplay()//生成一个字符串数组，包含在节点图编辑器中显示的房间节点类型名称
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];//创建一个房间数组
        for (int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
            {
                roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }
        return roomArray;
    }

    public void ProcessEvents(Event currentEvent)//处理房间节点事件
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown://鼠标按下事件
                ProcessMouseDownEvent(currentEvent);
                break;

            case EventType.MouseUp://鼠标松开事件
                ProcessMouseUpEvent(currentEvent);
                break;

            case EventType.MouseDrag://鼠标拖拽事件
                ProcessMouseDragEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    //处理鼠标按下事件
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)//鼠标左键按下
        {
            ProcessLeftClickDownEvent();
        }
        if (currentEvent.button == 1)//鼠标右键按下
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }
    private void ProcessLeftClickDownEvent()
    {
        Selection.activeObject = this;//将当前节点设置为编辑器的活动选中对象

        //切换节点选择状态
        if (isSelected == true)
        {
            isSelected = false;
        }
        else 
        {
            isSelected = true;
        }
    }
    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);//传入房间节点和鼠标位置
    }
    //处理鼠标松开事件
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)//鼠标左键按下
        {
            ProcessLeftClickUpEvent();
        }
    }
    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    //处理鼠标拖拽事件
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)//鼠标左键按下
        {
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }
    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;//节点正在被拖拽

        DragNode(currentEvent.delta);//拖拽节点
        GUI.changed = true;//GUI状态已改变需要重绘
    }
    public void DragNode(Vector2 delta)//delta：鼠标被移动的程度
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    public bool AddChildRoomNodeIDToRoomNode(string childID)//向节点添加子节点ID
    {
        //子节点是否可以有效添加至父节点
        if (IsChildRoomValid(childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }
        return false;
    }
    public bool IsChildRoomValid(string childID)//子节点是否可以有效添加到父节点
    {
        bool isConnectedBossNodeAlready = false;//是否已连接Boss节点
        //节点图中是否已经有一个连接的Boss房间
        foreach (RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
        {
            if (roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
                isConnectedBossNodeAlready = true;
        }
        //子节点是Boss房间类型 && 已经有一个连接的Boss房间节点，则返回false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isBossRoom && isConnectedBossNodeAlready)
            return false;
          
        //子节点类型为none，则返回false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isNone)
            return false;

        //节点已经有这个子ID，返回false
        if (childRoomNodeIDList.Contains(childID))
            return false;

        //节点ID和子ID相同，返回false
        if (id == childID)
            return false;

        //子ID已经在父ID列表中，返回false
        if (parentRoomNodeIDList.Contains(childID))
            return false;

        //子节点已经有父节点，返回false
        if (roomNodeGraph.GetRoomNode(childID).parentRoomNodeIDList.Count > 0)
            return false;

        // 子节点是走廊且这个节点也是走廊，返回false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && roomNodeType.isCorridor)
            return false;

        //子节点不是走廊且这个节点也不是走廊，返回false
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && !roomNodeType.isCorridor)
            return false;

        // 如果添加走廊，检查这个节点是否已经达到最大允许的子走廊数
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.maxChildCorridors)
            return false;

        //子房间是入口，返回false - 入口必须始终是顶级父节点
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isEntrance)
            return false;

        //向走廊添加房间，检查这个走廊节点是否已经添加了房间
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
            return false;

        return true;
    }
    public bool AddParentRoomNodeIDToRoomNode(string parentID)//向节点添加父节点ID
    { 
        parentRoomNodeIDList.Add(parentID);
        return true;
    }
    public bool RemoveChildRoomNodeIDFromRoomNode(string childID)//从当前节点的子节点列表中移除指定的子节点ID
    {
        if (childRoomNodeIDList.Contains(childID))//如果节点包含该子ID，则移除它
        {
            childRoomNodeIDList.Remove(childID);
            return true;
        }
        return false;
    }
    public bool RemoveParentRoomNodeIDFromRoomNode(string parentID)//从当前节点的父节点列表中移除指定的父节点ID
    {
        if (parentRoomNodeIDList.Contains(parentID))//如果节点包含该父ID，则移除它
        {
            parentRoomNodeIDList.Remove(parentID);
            return true;
        }
        return false;
    }
#endif
    #endregion Editor Code
}
