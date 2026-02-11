using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class MoveItem : MonoBehaviour
{
    #region SOUND EFFECT
    [Header("音效")]
    #endregion SOUND EFFECT
    #region Tooltip
    [Tooltip("移动此物品时的音效")]
    #endregion Tooltip
    [SerializeField] private SoundEffectSO moveSoundEffect;

    [HideInInspector] public BoxCollider2D boxCollider2D;
    private Rigidbody2D rigidBody2D;
    private InstantiatedRoom instantiatedRoom;
    private Vector3 previousPosition;

    private void Awake()
    {
        //获取组件引用
        boxCollider2D = GetComponent<BoxCollider2D>();
        rigidBody2D = GetComponent<Rigidbody2D>();
        instantiatedRoom = GetComponentInParent<InstantiatedRoom>();

        //将此物品添加到可移动物品列表中
        instantiatedRoom.moveableItemsList.Add(this);
    }

    //当有物体发生碰撞接触时更新障碍物位置
    private void OnCollisionStay2D(Collision2D collision)
    {
        UpdateObstacles();
    }


    //更新障碍物位置
    private void UpdateObstacles()
    {
        //确保物品保持在房间边界内
        ConfineItemToRoomBounds();

        //更新障碍物数组中的可移动物品
        instantiatedRoom.UpdateMoveableObstacles();

        //捕获碰撞后的新位置
        previousPosition = transform.position;

        //如果正在移动（允许较小的速度）则播放音效
        if (Mathf.Abs(rigidBody2D.velocity.x) > 0.001f || Mathf.Abs(rigidBody2D.velocity.y) > 0.001f)
        {
            //每10帧播放一次移动音效
            if (moveSoundEffect != null && Time.frameCount % 10 == 0)
            {
                SoundEffectManager.Instance.PlaySoundEffect(moveSoundEffect);
            }
        }
    }

    //将物品限制在房间边界内
    private void ConfineItemToRoomBounds()
    {
        Bounds itemBounds = boxCollider2D.bounds;
        Bounds roomBounds = instantiatedRoom.roomColliderBounds;

        //如果物品被推至超出房间边界，则将物品位置设置为其先前位置
        if (itemBounds.min.x <= roomBounds.min.x ||
            itemBounds.max.x >= roomBounds.max.x ||
            itemBounds.min.y <= roomBounds.min.y ||
            itemBounds.max.y >= roomBounds.max.y)
        {
            transform.position = previousPosition;
        }

    }

}