using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeType_", menuName = "Scriptable Objects/Dungeon/Room Node Type")] 
public class RoomNodeTypeSO : ScriptableObject
{
    public string roomNodeTypeName;//房间节点类型名称

    #region Header
    [Header("此节点类型在编辑器中可见")]
    #endregion Header
    public bool displayInNodeGraphEditor = true;

    #region Header
    [Header("走廊")]
    #endregion Header
    public bool isCorridor;

    #region Header
    [Header("南北方向走廊")]
    #endregion Header
    public bool isCorridorNS;

    #region Header
    [Header("东西方向走廊")]
    #endregion Header
    public bool isCorridorEW;

    #region Header
    [Header("入口")]
    #endregion Header
    public bool isEntrance;

    #region Header
    [Header("Boss房间")]
    #endregion Header
    public bool isBossRoom;

    #region Header
    [Header("无（未分配）")]
    #endregion Header
    public bool isNone;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(roomNodeTypeName), roomNodeTypeName);
    }
#endif
    #endregion
}
