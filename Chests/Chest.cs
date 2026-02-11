using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(MaterializeEffect))]
public class Chest : MonoBehaviour, IUseable
{
    #region Tooltip
    [Tooltip("设置材质化效果使用的颜色")]
    #endregion Tooltip
    [ColorUsage(false, true)]
    [SerializeField] private Color materializeColor;
    #region Tooltip
    [Tooltip("设置宝箱材质化所需的时间")]
    #endregion Tooltip
    [SerializeField] private float materializeTime = 3f;
    #region Tooltip
    [Tooltip("填充物品生成点transform")]
    #endregion Tooltip
    [SerializeField] private Transform itemSpawnPoint;
    private int healthPercent;
    private WeaponDetailsSO weaponDetails;
    private int ammoPercent;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private MaterializeEffect materializeEffect;
    private bool isEnabled = false;
    private ChestState chestState = ChestState.closed;
    private GameObject chestItemGameObject;
    private ChestItem chestItem;
    private TextMeshPro messageTextTMP;

    private void Awake()
    {
        //缓存组件
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        materializeEffect = GetComponent<MaterializeEffect>();
        messageTextTMP = GetComponentInChildren<TextMeshPro>();
    }

    //初始化宝箱，可选择立即显示或进行材质化
    public void Initialize(bool shouldMaterialize, int healthPercent, WeaponDetailsSO weaponDetails, int ammoPercent)
    {
        this.healthPercent = healthPercent;
        this.weaponDetails = weaponDetails;
        this.ammoPercent = ammoPercent;

        if (shouldMaterialize)
        {
            StartCoroutine(MaterializeChest());
        }
        else
        {
            EnableChest();
        }
    }

    //使宝箱材质化
    private IEnumerator MaterializeChest()
    {
        SpriteRenderer[] spriteRendererArray = new SpriteRenderer[] { spriteRenderer };

        yield return StartCoroutine(materializeEffect.MaterializeRoutine(GameResources.Instance.materializeShader, materializeColor, materializeTime, spriteRendererArray, GameResources.Instance.litMaterial));

        EnableChest();
    }

    //启用宝箱
    private void EnableChest()
    {
        //设置为可使用状态
        isEnabled = true;
    }

    //使用宝箱 - 操作将根据宝箱状态而变化
    public void UseItem()
    {
        if (!isEnabled) return;

        switch (chestState)
        {
            case ChestState.closed:
                OpenChest();
                break;

            case ChestState.healthItem:
                CollectHealthItem();
                break;

            case ChestState.ammoItem:
                CollectAmmoItem();
                break;

            case ChestState.weaponItem:
                CollectWeaponItem();
                break;

            case ChestState.empty:
                return;

            default:
                return;
        }
    }

    //首次使用时打开宝箱
    private void OpenChest()
    {
        animator.SetBool(Settings.use, true);

        //宝箱打开音效
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.chestOpen);

        //检查玩家是否已拥有该武器 - 如果是则将武器设为null
        if (weaponDetails != null)
        {
            if (GameManager.Instance.GetPlayer().IsWeaponHeldByPlayer(weaponDetails))
                weaponDetails = null;
        }

        UpdateChestState();
    }

    //根据应生成的内容和宝箱状态创建物品
    private void UpdateChestState()
    {
        if (healthPercent != 0)
        {
            chestState = ChestState.healthItem;
            InstantiateHealthItem();
        }
        else if (ammoPercent != 0)
        {
            chestState = ChestState.ammoItem;
            InstantiateAmmoItem();
        }
        else if (weaponDetails != null)
        {
            chestState = ChestState.weaponItem;
            InstantiateWeaponItem();
        }
        else
        {
            chestState = ChestState.empty;
        }
    }

    //实例化宝箱物品
    private void InstantiateItem()
    {
        chestItemGameObject = Instantiate(GameResources.Instance.chestItemPrefab, this.transform);

        chestItem = chestItemGameObject.GetComponent<ChestItem>();
    }

    //实例化玩家可收集的生命值物品
    private void InstantiateHealthItem()
    {
        InstantiateItem();

        chestItem.Initialize(GameResources.Instance.heartIcon, healthPercent.ToString() + "%", itemSpawnPoint.position, materializeColor);
    }


    //收集生命值物品并添加到玩家生命值中
    private void CollectHealthItem()
    {
        //检查物品是否存在且已完成材质化
        if (chestItem == null || !chestItem.isItemMaterialized) return;

        //为玩家增加生命值
        GameManager.Instance.GetPlayer().health.AddHealth(healthPercent);

        //播放拾取音效
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.healthPickup);

        healthPercent = 0;

        Destroy(chestItemGameObject);

        UpdateChestState();
    }

    //实例化玩家可收集的弹药物品
    private void InstantiateAmmoItem()
    {
        InstantiateItem();

        chestItem.Initialize(GameResources.Instance.bulletIcon, ammoPercent.ToString() + "%", itemSpawnPoint.position, materializeColor);
    }


    //收集弹药物品并添加到玩家当前武器的弹药中
    private void CollectAmmoItem()
    {
        //检查物品是否存在且已完成材质化
        if (chestItem == null || !chestItem.isItemMaterialized) return;

        Player player = GameManager.Instance.GetPlayer();

        //更新当前武器的弹药
        player.reloadWeaponEvent.CallReloadWeaponEvent(player.activeWeapon.GetCurrentWeapon(), ammoPercent);

        //播放拾取音效
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.ammoPickup);

        ammoPercent = 0;

        Destroy(chestItemGameObject);

        UpdateChestState();
    }

    //实例化玩家可收集的武器物品
    private void InstantiateWeaponItem()
    {
        InstantiateItem();

        chestItemGameObject.GetComponent<ChestItem>().Initialize(weaponDetails.weaponSprite, weaponDetails.weaponName, itemSpawnPoint.position, materializeColor);
    }

    //收集武器并添加到玩家的武器列表中
    private void CollectWeaponItem()
    {
        //检查物品是否存在且已完成材质化
        if (chestItem == null || !chestItem.isItemMaterialized) return;

        //如果玩家尚未拥有该武器，则添加到玩家
        if (!GameManager.Instance.GetPlayer().IsWeaponHeldByPlayer(weaponDetails))
        {
            //为玩家添加武器
            GameManager.Instance.GetPlayer().AddWeaponToPlayer(weaponDetails);

            //播放拾取音效
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.weaponPickup);
        }

        else
        {
            //显示"武器已装备"的消息
            StartCoroutine(DisplayMessage("武器\n已\n装备", 5f));

        }
        weaponDetails = null;

        Destroy(chestItemGameObject);

        UpdateChestState();
    }

    //在宝箱上方显示消息
    private IEnumerator DisplayMessage(string messageText, float messageDisplayTime)
    {
        messageTextTMP.text = messageText;

        yield return new WaitForSeconds(messageDisplayTime);

        messageTextTMP.text = "";
    }
}