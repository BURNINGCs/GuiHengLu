using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenCursor : MonoBehaviour
{
    private void Awake()
    {
        //关闭硬件光标
        Cursor.visible = false;
    }

    private void Update()
    {
        transform.position = Input.mousePosition;
    }
}
