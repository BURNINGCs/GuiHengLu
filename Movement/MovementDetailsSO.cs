using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="MovementDetails_",menuName ="Scriptable Objects/Movement/MovementDetails")]
public class MovementDetailsSO : ScriptableObject
{
    #region Header MOVEMENT DETAILS
    [Space(10)]
    [Header("移动详情")]
    #endregion Header
    #region Tooltip
    [Tooltip("最小移动速度。GetMoveSpeed 方法会计算一个介于最小值和最大值之间的随机值")]
    #endregion Tooltip
    public float minMoveSpeed = 8f;
    #region Tooltip
    [Tooltip("最大移动速度。GetMoveSpeed 方法会计算一个介于最小值和最大值之间的随机值")]
    #endregion Tooltip
    public float maxMoveSpeed = 8f;

    //获取一个介于最小值和最大值之间的随机移动速度
    public float GetMoveSpeed()
    {
        if (minMoveSpeed == maxMoveSpeed) 
        {
            return minMoveSpeed;
        }
        else
        {
            return Random.Range(minMoveSpeed, maxMoveSpeed);
        }
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(minMoveSpeed), minMoveSpeed, nameof(maxMoveSpeed), maxMoveSpeed, false);
    }

#endif
    #endregion Validation
}
