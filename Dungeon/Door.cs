using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class Door : MonoBehaviour
{
    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("对象引用")]
    #endregion

    #region Tooltip
    [Tooltip("用 DoorCollider 游戏对象上的 BoxCollider2D 组件填充此处")]
    #endregion
    [SerializeField] private BoxCollider2D doorCollider;

    [HideInInspector] public bool isBossRoomDoor = false;
    private BoxCollider2D doorTrigger;
    private bool isOpen = false;
    private bool previouslyOpened = false;
    private Animator animator;

    // 添加：防卡门相关变量
    private bool isPlayerTransitioning = false;
    private Coroutine ensurePassageCoroutine;

    private void Awake()
    {
        //默认禁用门碰撞体
        doorCollider.enabled = false;

        //加载组件
        animator = GetComponent<Animator>();
        doorTrigger = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.tag == Settings.playerTag || collision.tag == Settings.playerWeapon) && !isPlayerTransitioning)
        {
            isPlayerTransitioning = true;
            OpenDoor();

            // 添加：确保玩家能通过门
            StartEnsurePassage(collision);
        }
    }

    // 新增方法：开始确保玩家通过的协程
    private void StartEnsurePassage(Collider2D playerCollider)
    {
        // 如果已经有协程在运行，先停止它
        if (ensurePassageCoroutine != null)
        {
            StopCoroutine(ensurePassageCoroutine);
        }

        ensurePassageCoroutine = StartCoroutine(EnsurePassageRoutine(playerCollider));
    }

    // 新增方法：确保玩家通过的协程
    private IEnumerator EnsurePassageRoutine(Collider2D playerCollider)
    {
        // 完全禁用所有碰撞体，确保玩家可以通过
        doorCollider.enabled = false;
        doorTrigger.enabled = false;

        // 等待足够时间让玩家通过（根据开门动画时间调整）
        yield return new WaitForSeconds(1f);

        // 重新启用触发碰撞体（但保持doorCollider禁用，因为门是开的）
        doorTrigger.enabled = true;

        // 重置状态，允许下次触发
        isPlayerTransitioning = false;

        ensurePassageCoroutine = null;
    }

    private void OnEnable()
    {
        //当父游戏对象被禁用时（当玩家离房间足够远时），动画器状态会被重置。
        //因此我们需要恢复动画器状态。
        animator.SetBool(Settings.open, isOpen);

        // 添加：重置防卡门状态
        isPlayerTransitioning = false;
    }

    private void OnDisable()
    {
        // 添加：停止所有协程
        if (ensurePassageCoroutine != null)
        {
            StopCoroutine(ensurePassageCoroutine);
            ensurePassageCoroutine = null;
        }
        isPlayerTransitioning = false;
    }

    //打开门
    public void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;
            previouslyOpened = true;
            doorCollider.enabled = false;
            doorTrigger.enabled = false;

            //在动画器中设置开启参数
            animator.SetBool(Settings.open, true);

            //播放音效
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.doorOpenCloseSoundEffect);
        }
    }

    //锁定门
    public void LockDoor()
    {
        isOpen = false;
        doorCollider.enabled = true;
        doorTrigger.enabled = false;

        // 添加：重置防卡门状态
        isPlayerTransitioning = false;
        if (ensurePassageCoroutine != null)
        {
            StopCoroutine(ensurePassageCoroutine);
            ensurePassageCoroutine = null;
        }

        //将开启参数设为 false 以关闭门
        animator.SetBool(Settings.open, false);
    }

    //解锁门
    public void UnlockDoor()
    {
        doorCollider.enabled = false;
        doorTrigger.enabled = true;

        // 添加：重置防卡门状态
        isPlayerTransitioning = false;

        if (previouslyOpened == true)
        {
            isOpen = false;
            OpenDoor();
        }
    }

    // 新增方法：强制重新启用门（在需要时调用）
    public void ForceReenableDoor()
    {
        isPlayerTransitioning = false;

        if (ensurePassageCoroutine != null)
        {
            StopCoroutine(ensurePassageCoroutine);
            ensurePassageCoroutine = null;
        }

        // 根据门的状态重新设置碰撞体
        if (isOpen)
        {
            doorCollider.enabled = false;
            doorTrigger.enabled = true;
        }
        else
        {
            doorCollider.enabled = true;
            doorTrigger.enabled = false;
        }
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(doorCollider), doorCollider);
    }
#endif
    #endregion
}