using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[RequireComponent(typeof(InstantiatedRoom))]
public class RoomLightingControl : MonoBehaviour
{
    private InstantiatedRoom instantiatedRoom;

    private void Awake()
    {
        //加载组件
        instantiatedRoom = GetComponent<InstantiatedRoom>();
    }

    private void OnEnable()
    {
        //订阅房间变更事件
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {
        //取消订阅房间变更事件的订阅
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }

    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        //若当前是进入的房间且房间尚未点亮，则淡入房间光照
        if (roomChangedEventArgs.room == instantiatedRoom.room && !instantiatedRoom.room.isLit)
        {
            //淡入房间
            FadeInRoomLighting();

            //淡入房间门的光照
            FadeInDoors();

            instantiatedRoom.room.isLit = true;
        }
    }

    //淡入房间光照
    private void FadeInRoomLighting()
    {
        //淡入房间瓦片地图的光照
        StartCoroutine(FadeInRoomLightingRoutine(instantiatedRoom));
    }

    //房间光照淡入协程
    private IEnumerator FadeInRoomLightingRoutine(InstantiatedRoom instantiatedRoom)
    {
        //创建用于淡入的新材质
        Material material = new Material(GameResources.Instance.variableLitShader);

        instantiatedRoom.groundTilemap.GetComponent<TilemapRenderer>().material = material;
        instantiatedRoom.decoration1Tilemap.GetComponent<TilemapRenderer>().material = material;
        instantiatedRoom.decoration2Tilemap.GetComponent<TilemapRenderer>().material = material;
        instantiatedRoom.frontTilemap.GetComponent<TilemapRenderer>().material = material;
        instantiatedRoom.minimapTilemap.GetComponent<TilemapRenderer>().material = material;

        for (float i = 0.05f; i <= 1f; i += Time.deltaTime / Settings.fadeInTime)
        {
            material.SetFloat("Alpha_Slider", i);
            yield return null;
        }

        //将材质恢复为受光材质（Lit Material）
        instantiatedRoom.groundTilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
        instantiatedRoom.decoration1Tilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
        instantiatedRoom.decoration2Tilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
        instantiatedRoom.frontTilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
        instantiatedRoom.minimapTilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
    }

    //淡入门
    private void FadeInDoors()
    {
        Door[] doorArray = GetComponentsInChildren<Door>();

        foreach (Door door in doorArray)
        {
            DoorLightingControl doorLightingControl = door.GetComponentInChildren<DoorLightingControl>();

            doorLightingControl.FadeInDoor(door);
        }
    }
}
