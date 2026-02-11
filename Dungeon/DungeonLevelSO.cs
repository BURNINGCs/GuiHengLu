using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DungeonLevel_", menuName = "Scriptable Objects/Dungeon/Dungeon Level")]
public class DungeonLevelSO : ScriptableObject
{
    #region Header BASIC LEVEL DETAILS
    [Space(10)]
    [Header("基础关卡详情")]
    #endregion Header BASIC LEVEL DETAILS

    #region Tooltip
    [Tooltip("关卡名称")]
    #endregion Tooltip

    public string levelName;

    #region Header ROOM TEMPLATES FOR LEVEL

    [Space(10)]
    [Header("关卡的房间模板")]
    #endregion Header ROOM TEMPLATES FOR LEVEL

    #region Tooltip
    [Tooltip("用你希望纳入关卡的房间模板填充此列表。你需要确保：关卡的房间节点图中指定的所有房间节点类型，都有对应的房间模板被包含在内。")]
    #endregion Tooltip
    public List<RoomTemplateSO> roomTemplateList;

    #region Header ROOM NODE GRAPHS FOR LEVEL
    [Space(10)]
    [Header("关卡的房间节点图")]
    #endregion Header ROOM NODE GRAPHS FOR LEVEL

    #region Tooltip
    [Tooltip("将用于关卡的房间节点图填充到此列表中，关卡会从这些节点图中随机选取使用。")]
    #endregion Tooltip

    public List<RoomNodeGraphSO> roomNodeGraphList;

    #region Validation
#if UNITY_EDITOR
    //验证输入的可脚本化对象详情
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(levelName), levelName);
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomTemplateList), roomTemplateList))
            return;
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomNodeGraphList), roomNodeGraphList))
            return;

        //检查并确保，对于指定节点图中的所有节点类型，都已指定了对应的房间模板

        //首先检查是否已指定南北向走廊、东西向走廊和入口类型
        bool isEWCorridor = false;
        bool isNSCorridor = false;
        bool isEntrance = false;

        //遍历所有房间模板，检查是否已指定该节点类型
        foreach (RoomTemplateSO roomTemplateSO in roomTemplateList)
        {
            if (roomTemplateSO == null)
                return;

            if (roomTemplateSO.roomNodeType.isCorridorEW)
                isEWCorridor = true;

            if (roomTemplateSO.roomNodeType.isCorridorNS)
                isNSCorridor = true;

            if (roomTemplateSO.roomNodeType.isEntrance)
                isEntrance = true;
        }

        if (isEWCorridor == false)
        {
            Debug.Log("在" + this.name.ToString() + "中：未指定东西向走廊房间类型");
        }

        if (isNSCorridor == false)
        {
            Debug.Log("在" + this.name.ToString() + "中：未指定南北向走廊房间类型");
        }
        if (isEntrance == false)
        {
            Debug.Log("在" + this.name.ToString() + "中：未指定入口走廊房间类型");
        }

        //遍历所有节点图
        foreach (RoomNodeGraphSO roomNodeGraph in roomNodeGraphList)
        {
            if (roomNodeGraph == null)
                return;

            //遍历节点图中的所有节点
            foreach (RoomNodeSO roomNodeSO in roomNodeGraph.roomNodeList)
            {
                if (roomNodeSO == null)
                    return;

                //检查每个房间节点类型是否都已指定对应的房间模板

                //走廊和入口已完成检查
                if (roomNodeSO.roomNodeType.isEntrance || roomNodeSO.roomNodeType.isCorridorEW || roomNodeSO.roomNodeType.isCorridorNS ||
                    roomNodeSO.roomNodeType.isCorridor || roomNodeSO.roomNodeType.isNone)
                    continue;

                bool isRoomNodeTypeFound = false;

                //遍历所有房间模板，检查该节点类型是否已被指定
                foreach (RoomTemplateSO roomTemplateSO in roomTemplateList)
                {
                    if (roomTemplateSO == null)
                        continue;

                    if (roomTemplateSO.roomNodeType == roomNodeSO.roomNodeType)
                    {
                        isRoomNodeTypeFound = true;
                        break;
                    }
                }

                if (!isRoomNodeTypeFound)
                {
                    Debug.Log("In " + this.name.ToString() + " : No room template " + roomNodeSO.roomNodeType.name.ToString() + " found for node graph "
                        + roomNodeGraph.name.ToString());
                }
            }
        }
    }



#endif
    #endregion Validation
}
