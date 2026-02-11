using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ActivateRooms : MonoBehaviour
{
    #region Header POPULATE WITH MINIMAP CAMERA
    [Header("填充小地图摄像头")]
    #endregion Header
    [SerializeField] private Camera miniMapCamera;

    // Start is called before the first frame update
    private void Start()
    {
        InvokeRepeating("EnableRooms", 0.5f, 0.75f);
    }

    private void EnableRooms()//根据小地图摄像机视野范围启用或禁用房间
    {
        //如果当前正在显示地下城总览地图，则不进行处理
        if (GameManager.Instance.gameState == GameState.dungeonOverviewMap)
            return;

        //遍历所有地下城房间
        foreach (KeyValuePair<string, Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;

            HelperUtilities.CameraWorldPositionBounds(out Vector2Int miniMapCameraWorldPositionLowerBounds, out Vector2Int
                miniMapCameraWorldPositionUpperBounds, miniMapCamera);

            //如果房间在小地图摄像机视野范围内，则激活房间游戏对象
            if ((room.lowerBounds.x <= miniMapCameraWorldPositionUpperBounds.x && room.lowerBounds.y <= miniMapCameraWorldPositionUpperBounds.y) &&
                (room.upperBounds.x >= miniMapCameraWorldPositionLowerBounds.x && room.upperBounds.y >= miniMapCameraWorldPositionLowerBounds.y))
            {
                room.instantiatedRoom.gameObject.SetActive(true);
            }
            else
            {
                room.instantiatedRoom.gameObject.SetActive(false);
            }
        }
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(miniMapCamera), miniMapCamera);
    }
#endif
    #endregion
}
