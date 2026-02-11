using System.Collections.Generic;
using UnityEngine;

public class ChestSpawner : MonoBehaviour
{
    [System.Serializable]
    private struct RangeByLevel
    {
        public DungeonLevelSO dungeonLevel;
        [Range(0, 100)] public int min;
        [Range(0, 100)] public int max;
    }

    #region Header CHEST PREFAB
    [Space(10)]
    [Header("宝箱预制体")]
    #endregion Header CHEST PREFAB
    #region Tooltip
    [Tooltip("填充宝箱预制体")]
    #endregion Tooltip
    [SerializeField] private GameObject chestPrefab;

    #region Header CHEST SPAWN CHANCE
    [Space(10)]
    [Header("宝箱生成概率")]
    #endregion Header CHEST SPAWN CHANCE
    #region Tooltip
    [Tooltip("生成宝箱的最小概率")]
    #endregion Tooltip
    [SerializeField][Range(0, 100)] private int chestSpawnChanceMin;
    #region Tooltip
    [Tooltip("生成宝箱的最大概率")]
    #endregion Tooltip
    [SerializeField][Range(0, 100)] private int chestSpawnChanceMax;
    #region Tooltip
    [Tooltip("按照地下城等级覆盖宝箱生成概率")]
    #endregion Tooltip
    [SerializeField] private List<RangeByLevel> chestSpawnChanceByLevelList;

    #region Header CHEST SPAWN DETAILS
    [Space(10)]
    [Header("宝箱生成详情")]
    #endregion Header CHEST SPAWN DETAILS
    [SerializeField] private ChestSpawnEvent chestSpawnEvent;
    [SerializeField] private ChestSpawnPosition chestSpawnPosition;
    #region Tooltip
    [Tooltip("要生成的物品最小数量（注意：弹药、生命值和武器每种类型最多生成1个）")]
    #endregion Tooltip
    [SerializeField][Range(0, 3)] private int numberOfItemsToSpawnMin;
    #region Tooltip
    [Tooltip("要生成的物品最大数量（注意：弹药、生命值和武器每种类型最多生成1个")]
    #endregion Tooltip
    [SerializeField][Range(0, 3)] private int numberOfItemsToSpawnMax;

    #region Header CHEST CONTENT DETAILS
    [Space(10)]
    [Header("宝箱内容详情")]
    #endregion Header CHEST CONTENT DETAILS
    #region Tooltip
    [Tooltip("每个地下城等级要生成的武器及其生成比例")]
    #endregion Tooltip
    [SerializeField] private List<SpawnableObjectsByLevel<WeaponDetailsSO>> weaponSpawnByLevelList;
    #region Tooltip
    [Tooltip("每个等级要生成的生命值范围")]
    #endregion Tooltip
    [SerializeField] private List<RangeByLevel> healthSpawnByLevelList;
    #region Tooltip
    [Tooltip("每个等级要生成的弹药范围")]
    #endregion Tooltip
    [SerializeField] private List<RangeByLevel> ammoSpawnByLevelList;

    private bool chestSpawned = false;
    private Room chestRoom;

    private void OnEnable()
    {
        //订阅房间改变事件
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;

        //订阅房间敌人被击败事件
        StaticEventHandler.OnRoomEnemiesDefeated += StaticEventHandler_OnRoomEnemiesDefeated;
    }

    private void OnDisable()
    {
        //取消订阅房间改变事件
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;

        //取消订阅房间敌人被击败事件
        StaticEventHandler.OnRoomEnemiesDefeated -= StaticEventHandler_OnRoomEnemiesDefeated;
    }

    //处理房间改变事件
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        //如果没有获取过宝箱所在的房间，则获取它
        if (chestRoom == null)
        {
            chestRoom = GetComponentInParent<InstantiatedRoom>().room;
        }

        //如果宝箱在进入房间时生成，并且当前房间是宝箱房间，则生成宝箱
        if (!chestSpawned && chestSpawnEvent == ChestSpawnEvent.onRoomEntry && chestRoom == roomChangedEventArgs.room)
        {
            SpawnChest();
        }
    }

    /// <summary>
    /// Handle room enemies defeated event
    /// </summary>
    private void StaticEventHandler_OnRoomEnemiesDefeated(RoomEnemiesDefeatedArgs roomEnemiesDefeatedArgs)
    {
        // Get the room the chest is in if we don't already have it
        if (chestRoom == null)
        {
            chestRoom = GetComponentInParent<InstantiatedRoom>().room;
        }

        // If the chest is spawned when enemies are defeated and the chest is in the room that the
        // enemies have been defeated
        if (!chestSpawned && chestSpawnEvent == ChestSpawnEvent.onEnemiesDefeated && chestRoom == roomEnemiesDefeatedArgs.room)
        {
            SpawnChest();
        }
    }

    /// <summary>
    /// Spawn the chest prefab
    /// </summary>
    private void SpawnChest()
    {
        chestSpawned = true;

        // Should chest be spawned based on specified chance? If not return.
        if (!RandomSpawnChest()) return;

        // Get Number Of Ammo,Health, & Weapon Items To Spawn (max 1 of each)
        GetItemsToSpawn(out int ammoNum, out int healthNum, out int weaponNum);

        // Instantiate chest
        GameObject chestGameObject = Instantiate(chestPrefab, this.transform);

        // Position chest
        if (chestSpawnPosition == ChestSpawnPosition.atSpawnerPosition)
        {
            chestGameObject.transform.position = this.transform.position;
        }
        else if (chestSpawnPosition == ChestSpawnPosition.atPlayerPosition)
        {
            // Get nearest spawn position to player
            Vector3 spawnPosition = HelperUtilities.GetSpawnPositionNearestToPlayer(GameManager.Instance.GetPlayer().transform.position);

            // Calculate some random variation
            Vector3 variation = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);

            chestGameObject.transform.position = spawnPosition + variation;
        }

        // Get Chest component
        Chest chest = chestGameObject.GetComponent<Chest>();

        // Initialize chest
        if (chestSpawnEvent == ChestSpawnEvent.onRoomEntry)
        {
            // Don't use materialize effect
            chest.Initialize(false, GetHealthPercentToSpawn(healthNum), GetWeaponDetailsToSpawn(weaponNum), GetAmmoPercentToSpawn(ammoNum));
        }
        else
        {
            // use materialize effect
            chest.Initialize(true, GetHealthPercentToSpawn(healthNum), GetWeaponDetailsToSpawn(weaponNum), GetAmmoPercentToSpawn(ammoNum));
        }
    }

    /// <summary>
    /// Check if a chest should be spawned based on the chest spawn chance - returns true if chest should be spawned false otherwise
    /// </summary>
    private bool RandomSpawnChest()
    {
        int chancePercent = Random.Range(chestSpawnChanceMin, chestSpawnChanceMax + 1);

        // Check if an override chance percent has been set for the current level
        foreach (RangeByLevel rangeByLevel in chestSpawnChanceByLevelList)
        {
            if (rangeByLevel.dungeonLevel == GameManager.Instance.GetCurrentDungeonLevel())
            {
                chancePercent = Random.Range(rangeByLevel.min, rangeByLevel.max + 1);
                break;
            }
        }

        // get random value between 1 and 100
        int randomPercent = Random.Range(1, 100 + 1);

        if (randomPercent <= chancePercent)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Get the number of items to spawn - max 1 of each - max 3 in total
    /// </summary>
    private void GetItemsToSpawn(out int ammo, out int health, out int weapons)
    {
        ammo = 0;
        health = 0;
        weapons = 0;

        int numberOfItemsToSpawn = Random.Range(numberOfItemsToSpawnMin, numberOfItemsToSpawnMax + 1);

        int choice;

        if (numberOfItemsToSpawn == 1)
        {
            choice = Random.Range(0, 3);
            if (choice == 0) { weapons++; return; }
            if (choice == 1) { ammo++; return; }
            if (choice == 2) { health++; return; }
            return;
        }
        else if (numberOfItemsToSpawn == 2)
        {
            choice = Random.Range(0, 3);
            if (choice == 0) { weapons++; ammo++; return; }
            if (choice == 1) { ammo++; health++; return; }
            if (choice == 2) { health++; weapons++; return; }
        }
        else if (numberOfItemsToSpawn >= 3)
        {
            weapons++;
            ammo++;
            health++;
            return;
        }
    }

    /// <summary>
    /// Get ammo percent to spawn
    /// </summary>
    private int GetAmmoPercentToSpawn(int ammoNumber)
    {
        if (ammoNumber == 0) return 0;

        // Get ammo spawn percent range for level
        foreach (RangeByLevel spawnPercentByLevel in ammoSpawnByLevelList)
        {
            if (spawnPercentByLevel.dungeonLevel == GameManager.Instance.GetCurrentDungeonLevel())
            {
                return Random.Range(spawnPercentByLevel.min, spawnPercentByLevel.max);
            }
        }

        return 0;
    }

    /// <summary>
    /// Get health percent to spawn
    /// </summary>
    private int GetHealthPercentToSpawn(int healthNumber)
    {
        if (healthNumber == 0) return 0;

        // Get ammo spawn percent range for level
        foreach (RangeByLevel spawnPercentByLevel in healthSpawnByLevelList)
        {
            if (spawnPercentByLevel.dungeonLevel == GameManager.Instance.GetCurrentDungeonLevel())
            {
                return Random.Range(spawnPercentByLevel.min, spawnPercentByLevel.max);
            }
        }

        return 0;
    }

    /// <summary>
    /// Get the weapon details to spawn - return null if no weapon is to be spawned or the player already has the weapon
    /// </summary>
    private WeaponDetailsSO GetWeaponDetailsToSpawn(int weaponNumber)
    {
        if (weaponNumber == 0) return null;

        // Create an instance of the class used to select a random item from a list based on the
        // relative 'ratios' of the items specified
        RandomSpawnableObject<WeaponDetailsSO> weaponRandom = new RandomSpawnableObject<WeaponDetailsSO>(weaponSpawnByLevelList);

        WeaponDetailsSO weaponDetails = weaponRandom.GetItem();

        return weaponDetails;
    }

    #region Validation
#if UNITY_EDITOR

    // Validate prefab details enetered
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(chestPrefab), chestPrefab);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(chestSpawnChanceMin), chestSpawnChanceMin, nameof(chestSpawnChanceMax), chestSpawnChanceMax, true);

        if (chestSpawnChanceByLevelList != null && chestSpawnChanceByLevelList.Count > 0)
        {
            HelperUtilities.ValidateCheckEnumerableValues(this, nameof(chestSpawnChanceByLevelList), chestSpawnChanceByLevelList);

            foreach (RangeByLevel rangeByLevel in chestSpawnChanceByLevelList)
            {
                HelperUtilities.ValidateCheckNullValue(this, nameof(rangeByLevel.dungeonLevel), rangeByLevel.dungeonLevel);
                HelperUtilities.ValidateCheckPositiveRange(this, nameof(rangeByLevel.min), rangeByLevel.min, nameof(rangeByLevel.max), rangeByLevel.max, true);
            }
        }

        HelperUtilities.ValidateCheckPositiveRange(this, nameof(numberOfItemsToSpawnMin), numberOfItemsToSpawnMin, nameof(numberOfItemsToSpawnMax), numberOfItemsToSpawnMax, true);

        if (weaponSpawnByLevelList != null && weaponSpawnByLevelList.Count > 0)
        {
            foreach (SpawnableObjectsByLevel<WeaponDetailsSO> weaponDetailsByLevel in weaponSpawnByLevelList)
            {
                HelperUtilities.ValidateCheckNullValue(this, nameof(weaponDetailsByLevel.dungeonLevel), weaponDetailsByLevel.dungeonLevel);

                foreach (SpawnableObjectRatio<WeaponDetailsSO> weaponRatio in weaponDetailsByLevel.spawnableObjectRatioList)
                {
                    HelperUtilities.ValidateCheckNullValue(this, nameof(weaponRatio.dungeonObject), weaponRatio.dungeonObject);

                    HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponRatio.ratio), weaponRatio.ratio, true);
                }
            }
        }

        if (healthSpawnByLevelList != null && healthSpawnByLevelList.Count > 0)
        {
            HelperUtilities.ValidateCheckEnumerableValues(this, nameof(healthSpawnByLevelList), healthSpawnByLevelList);

            foreach (RangeByLevel rangeByLevel in healthSpawnByLevelList)
            {
                HelperUtilities.ValidateCheckNullValue(this, nameof(rangeByLevel.dungeonLevel), rangeByLevel.dungeonLevel);
                HelperUtilities.ValidateCheckPositiveRange(this, nameof(rangeByLevel.min), rangeByLevel.min, nameof(rangeByLevel.max), rangeByLevel.max, true);
            }
        }

        if (ammoSpawnByLevelList != null && ammoSpawnByLevelList.Count > 0)
        {
            HelperUtilities.ValidateCheckEnumerableValues(this, nameof(ammoSpawnByLevelList), ammoSpawnByLevelList);
            foreach (RangeByLevel rangeByLevel in ammoSpawnByLevelList)
            {
                HelperUtilities.ValidateCheckNullValue(this, nameof(rangeByLevel.dungeonLevel), rangeByLevel.dungeonLevel);
                HelperUtilities.ValidateCheckPositiveRange(this, nameof(rangeByLevel.min), rangeByLevel.min, nameof(rangeByLevel.max), rangeByLevel.max, true);
            }
        }

    }

#endif

    #endregion Validation

}