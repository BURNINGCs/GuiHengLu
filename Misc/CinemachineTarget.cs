using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineTargetGroup))]
public class CinemachineTarget : MonoBehaviour
{
    private CinemachineTargetGroup cinemachineTargetGroup;

    #region Tooltip
    [Tooltip("Populate with the CursorTarget gameobject")]
    #endregion Tooltip
    [SerializeField] private Transform cursorTarget;

    private void Awake()
    {
        //加载组件
        cinemachineTargetGroup = GetComponent<CinemachineTargetGroup>();
    }

    //Start 方法在第一帧更新前调用
    void Start()
    {
        SetCinemachineTargetGroup();
    }

    //设置 Cinemachine 相机目标组
    private void SetCinemachineTargetGroup()
    {
        //为 Cinemachine 创建目标组，供 Cinemachine 相机跟随 —— 该组将包含玩家和屏幕光标
        CinemachineTargetGroup.Target cinemachineGroupTarget_player = new CinemachineTargetGroup.Target
        {
            weight = 1f,
            radius = 2.5f,
            target = GameManager.Instance.GetPlayer().transform
        };

        CinemachineTargetGroup.Target cinemachineGroupTarget_cursor = new CinemachineTargetGroup.Target
        {
            weight = 1f,
            radius = 1f,
            target = cursorTarget
        };

        CinemachineTargetGroup.Target[] cinemachineTargetArray = new CinemachineTargetGroup.Target[] { cinemachineGroupTarget_player,
        cinemachineGroupTarget_cursor};

        cinemachineTargetGroup.m_Targets = cinemachineTargetArray;
    }

    private void Update()
    {
        cursorTarget.position = HelperUtilities.GetMouseWorldPosition();
    }
}
