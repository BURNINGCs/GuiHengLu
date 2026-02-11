using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BeginScene : MonoBehaviour
{
    void Update()
    {
        //点击鼠标或按任意键都能进入MainMenuScene
        if (Input.GetMouseButtonDown(0) || Input.anyKeyDown)
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}
