using System.Collections.Generic;
using UnityEngine;

public class SpawnTest : MonoBehaviour
{
    private List<SpawnableObjectsByLevel<EnemyDetailsSO>> testLevelSpawnList;
    private RandomSpawnableObject<EnemyDetailsSO> randomEnemyHelperClass;
    private List<GameObject> instantiatedEnemyList = new List<GameObject>();

    private void OnEnable()
    {
        //订阅房间变更事件
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {
        //取消订阅房间变更事件
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }

    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        //销毁所有已生成的敌人
        if (instantiatedEnemyList != null && instantiatedEnemyList.Count > 0)
        {
            foreach (GameObject enemy in instantiatedEnemyList)
            {
                Destroy(enemy);
            }
        }

        RoomTemplateSO roomTemplate = DungeonBuilder.Instance.GetRoomTemplate(roomChangedEventArgs.room.templateID);

        if (roomTemplate != null)
        {
            testLevelSpawnList = roomTemplate.enemiesByLevelList;

            //创建随机可生成对象辅助类
            randomEnemyHelperClass = new RandomSpawnableObject<EnemyDetailsSO>(testLevelSpawnList);
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            EnemyDetailsSO enemyDetails = randomEnemyHelperClass.GetItem();

            if (enemyDetails != null)
            {
                instantiatedEnemyList.Add(Instantiate(enemyDetails.enemyPrefab, HelperUtilities.GetSpawnPositionNearestToPlayer
                    (HelperUtilities.GetMouseWorldPosition()), Quaternion.identity));
            }
        }
    }
}
