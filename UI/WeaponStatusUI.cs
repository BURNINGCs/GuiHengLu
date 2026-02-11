using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponStatusUI : MonoBehaviour
{
    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("对象引用")]
    #endregion Header OBJECT REFERENCES
    #region Tooltip
    [Tooltip("用子物体 “WeaponImage” 游戏对象上的 Image 组件填充此处")]
    #endregion Tooltip
    [SerializeField] private Image weaponImage;
    #region Tooltip
    [Tooltip("用子物体 “AmmoHolder” 游戏对象上的 Transform 组件填充此处")]
    #endregion Tooltip
    [SerializeField] private Transform ammoHolderTransform;
    #region Tooltip
    [Tooltip("用子物体 “ReloadText” 游戏对象上的 TextMeshPro-Text 组件填充此处")]
    #endregion Tooltip
    [SerializeField] private TextMeshProUGUI reloadText;
    #region Tooltip
    [Tooltip("用子物体 “AmmoRemainingText” 游戏对象上的 TextMeshPro-Text 组件填充此处")]
    #endregion Tooltip
    [SerializeField] private TextMeshProUGUI ammoRemainingText;
    #region Tooltip
    [Tooltip("用子物体 “WeaponNameText” 游戏对象上的 TextMeshPro-Text 组件填充此处")]
    #endregion Tooltip
    [SerializeField] private TextMeshProUGUI weaponNameText;
    #region Tooltip
    [Tooltip("用子物体 “ReloadBar” 游戏对象的 RectTransform 组件填充此处")]
    #endregion Tooltip
    [SerializeField] private Transform reloadBar;
    #region Tooltip
    [Tooltip("用子物体 “BarImage” 游戏对象的 Image 组件填充此处")]
    #endregion Tooltip
    [SerializeField] private Image barImage;

    private Player player;
    private List<GameObject> ammoIconList = new List<GameObject>();
    private Coroutine reloadWeaponCoroutine;
    private Coroutine blinkingReloadTextCoroutine;

    private void Awake()
    {
        //获取玩家
        player = GameManager.Instance.GetPlayer();
    }

    private void OnEnable()
    {
        //订阅设置激活武器事件
        player.setActiveWeaponEvent.OnSetActiveWeapon += SetActiveWeaponEvent_OnSetActiveWeapon;

        //订阅武器开火事件
        player.weaponFiredEvent.OnWeaponFired += WeaponFiredEvent_OnWeaponFired;

        //订阅重新装填武器事件
        player.reloadWeaponEvent.OnReloadWeapon += ReloadWeaponEvent_OnWeaponReload;

        //订阅武器装填完成事件
        player.weaponReloadedEvent.OnWeaponReloaded += WeaponReloadedEvent_OnWeaponReloaded;
    }

    private void OnDisable()
    {
        //取消订阅设置激活武器事件
        player.setActiveWeaponEvent.OnSetActiveWeapon -= SetActiveWeaponEvent_OnSetActiveWeapon;

        //取消订阅武器开火事件
        player.weaponFiredEvent.OnWeaponFired -= WeaponFiredEvent_OnWeaponFired;

        //取消订阅重新装填武器事件
        player.reloadWeaponEvent.OnReloadWeapon -= ReloadWeaponEvent_OnWeaponReload;

        //取消订阅武器装填完成事件
        player.weaponReloadedEvent.OnWeaponReloaded -= WeaponReloadedEvent_OnWeaponReloaded;
    }

    private void Start()
    {
        //更新 UI 上的激活武器状态
        SetActiveWeapon(player.activeWeapon.GetCurrentWeapon());
    }

    //在 UI 上处理设置激活武器事件
    private void SetActiveWeaponEvent_OnSetActiveWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        SetActiveWeapon(setActiveWeaponEventArgs.weapon);
    }

    //在 UI 上处理武器开火事件
    private void WeaponFiredEvent_OnWeaponFired(WeaponFiredEvent weaponFiredEvent, WeaponFiredEventArgs weaponFiredEventArgs)
    {
        WeaponFired(weaponFiredEventArgs.weapon);
    }

    //武器开火后更新 UI
    private void WeaponFired(Weapon weapon)
    {
        UpdateAmmoText(weapon);
        UpdateAmmoLoadedIcons(weapon);
        UpdateReloadText(weapon);
    }

    //在 UI 上处理武器装填事件
    private void ReloadWeaponEvent_OnWeaponReload(ReloadWeaponEvent reloadWeaponEvent, ReloadWeaponEventArgs reloadWeaponEventArgs)
    {
        UpdateWeaponReloadBar(reloadWeaponEventArgs.weapon);
    }

    //在 UI 上处理武器装填完成事件
    private void WeaponReloadedEvent_OnWeaponReloaded(WeaponReloadedEvent weaponReloadedEvent, WeaponReloadedEventArgs weaponReloadedEventArgs)
    {
        WeaponReloaded(weaponReloadedEventArgs.weapon);
    }

    //武器已装填完成 —— 若为当前武器则更新 UI
    private void WeaponReloaded(Weapon weapon)
    {
        //若装填完成的武器是当前武器
        if (player.activeWeapon.GetCurrentWeapon() == weapon)
        {
            UpdateReloadText(weapon);
            UpdateAmmoText(weapon);
            UpdateAmmoLoadedIcons(weapon);
            ResetWeaponReloadBar();
        }
    }

    //在 UI 上设置激活武器
    private void SetActiveWeapon(Weapon weapon)
    {
        UpdateActiveWeaponImage(weapon.weaponDetails);
        UpdateActiveWeaponName(weapon);
        UpdateAmmoText(weapon);
        UpdateAmmoLoadedIcons(weapon);

        //若待设置的武器仍在装填中，则更新装填进度条
        if (weapon.isWeaponReloading)
        {
            UpdateWeaponReloadBar(weapon);
        }
        else
        {
            ResetWeaponReloadBar();
        }

        UpdateReloadText(weapon);
    }

    //填充激活武器的图标
    private void UpdateActiveWeaponImage(WeaponDetailsSO weaponDetails)
    {
        weaponImage.sprite = weaponDetails.weaponSprite;
    }

    //填充激活武器的名称
    private void UpdateActiveWeaponName(Weapon weapon)
    {
        weaponNameText.text = "(" + weapon.weaponListPosition + ") " + weapon.weaponDetails.weaponName.ToUpper();
    }

    //更新 UI 上的剩余弹药文本
    private void UpdateAmmoText(Weapon weapon)
    {
        if (weapon.weaponDetails.hasInfiniteAmmo)
        {
            ammoRemainingText.text = "无限能量";
        }
        else
        {
            ammoRemainingText.text = weapon.weaponRemainingAmmo.ToString() + " /" + weapon.weaponDetails.weaponAmmoCapacity.ToString();
        }
    }

    //更新 UI 上的弹夹弹药图标
    private void UpdateAmmoLoadedIcons(Weapon weapon)
    {
        ClearAmmoLoadedIcons();

        for (int i = 0; i < weapon.weaponClipRemainingAmmo; i++)
        {
            //实例化弹药图标预制体
            GameObject ammoIcon = Instantiate(GameResources.Instance.ammoIconPrefab, ammoHolderTransform);

            ammoIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, Settings.uiAmmoIconSpacing * i);

            ammoIconList.Add(ammoIcon);
        }
    }

    //清空弹药图标
    private void ClearAmmoLoadedIcons()
    {
        //遍历图标游戏对象并销毁
        foreach (GameObject ammoIcon in ammoIconList)
        {
            Destroy(ammoIcon);
        }

        ammoIconList.Clear();
    }

    //装填武器 —— 更新 UI 上的装填进度条
    private void UpdateWeaponReloadBar(Weapon weapon)
    {
        if (weapon.weaponDetails.hasInfiniteClipCapacity)
            return;

        StopReloadWeaponCoroutine();
        UpdateReloadText(weapon);

        reloadWeaponCoroutine = StartCoroutine(UpdateWeaponReloadBarRoutine(weapon));
    }

    //装填进度条动画协程
    private IEnumerator UpdateWeaponReloadBarRoutine(Weapon currentWeapon)
    {
        //将装填进度条设为红色
        barImage.color = Color.red;

        //播放武器装填进度条动画
        while (currentWeapon.isWeaponReloading)
        {
            //更新装填进度条
            float barFill = currentWeapon.weaponReloadTimer / currentWeapon.weaponDetails.weaponReloadTime;

            //更新进度条填充量
            reloadBar.transform.localScale = new Vector3(barFill, 1f, 1f);

            yield return null;
        }
    }

    //在 UI 上初始化武器装填进度条
    private void ResetWeaponReloadBar()
    {
        StopReloadWeaponCoroutine();

        //将进度条颜色设为绿色
        barImage.color = Color.green;

        //将进度条缩放比例设为 1
        reloadBar.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    //停止更新武器装填进度条的协程
    private void StopReloadWeaponCoroutine()
    {
        //停止 UI 上所有活跃的武器装填进度条
        if (reloadWeaponCoroutine != null)
        {
            StopCoroutine(reloadWeaponCoroutine);
        }
    }

    //更新闪烁的武器装填文本
    private void UpdateReloadText(Weapon weapon)
    {
        if ((!weapon.weaponDetails.hasInfiniteClipCapacity) && (weapon.weaponClipRemainingAmmo <= 0 || weapon.isWeaponReloading))
        {
            //将装填进度条设为红色
            barImage.color = Color.red;

            StopBlinkingReloadTextCoroutine();

            blinkingReloadTextCoroutine = StartCoroutine(StartBlinkingReloadTextRoutine());
        }
        else
        {
            StopBlinkingReloadText();
        }
    }

    //启动装填文本闪烁协程
    private IEnumerator StartBlinkingReloadTextRoutine()
    {
        while (true)
        {
            reloadText.text = "能量冷却";
            yield return new WaitForSeconds(0.3f);
            reloadText.text = "";
            yield return new WaitForSeconds(0.3f);
        }
    }

    //停止装填文本闪烁
    private void StopBlinkingReloadText()
    {
        StopBlinkingReloadTextCoroutine();

        reloadText.text = "";
    }

    //停止装填文本闪烁协程
    private void StopBlinkingReloadTextCoroutine()
    {
        if (blinkingReloadTextCoroutine != null)
        {
            StopCoroutine(blinkingReloadTextCoroutine);
        }
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponImage), weaponImage);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoHolderTransform), ammoHolderTransform);
        HelperUtilities.ValidateCheckNullValue(this, nameof(reloadText), reloadText);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoRemainingText), ammoRemainingText);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponNameText), weaponNameText);
        HelperUtilities.ValidateCheckNullValue(this, nameof(reloadBar), reloadBar);
        HelperUtilities.ValidateCheckNullValue(this, nameof(barImage), barImage);
    }

#endif
    #endregion Validation
}
