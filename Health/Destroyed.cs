using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DestroyedEvent))]
[DisallowMultipleComponent]
public class Destroyed : MonoBehaviour
{
    private DestroyedEvent destroyedEvent;

    private void Awake()
    {
        //加载组件
        destroyedEvent = GetComponent<DestroyedEvent>();
    }

    private void OnEnable()
    {
        //订阅销毁事件
        destroyedEvent.OnDestroyed += DestroyedEvent_OnDestroyed;
    }

    private void OnDisable()
    {
        //取消订阅销毁事件
        destroyedEvent.OnDestroyed -= DestroyedEvent_OnDestroyed;
    }

    private void DestroyedEvent_OnDestroyed(DestroyedEvent destroyedEvent, DestroyedEventArgs destroyedEventArgs)
    {
        if (destroyedEventArgs.playerDied)
        {
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        } 
    }
}
