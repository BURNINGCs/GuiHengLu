using UnityEngine;
[System.Serializable]
public class Doorway 
{
    public Vector2Int position;//门洞位置
    public Orientation orientation;//方向
    public GameObject doorPrefab;//门的预制体对象
    #region Header
    [Header("从左上角开始复制的位置")]
    #endregion
    public Vector2Int doorwayStartCopyPosition;//门洞开始复制的位置
    #region Header
    [Header("门道处要复制的瓷砖宽度")]
    #endregion
    public int doorwayCopyTileWidth;
    #region Header
    [Header("门道处要复制的瓷砖高度")]
    #endregion
    public int doorwayCopyTileHeight;
    [HideInInspector]
    public bool isConnected = false;//门洞是否连接
    [HideInInspector]
    public bool isUnavailable = false;//门洞是否无法使用
}
