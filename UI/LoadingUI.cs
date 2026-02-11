using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadingUI : MonoBehaviour
{
    [Header("对象引用")]
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TextMeshProUGUI loadingNumber; 

    private float minLoadTime = 3f;
    private AsyncOperation asyncLoad;

    private void Start()
    {
        StartCoroutine(LoadMainGameScene());
    }

    private IEnumerator LoadMainGameScene()
    {
        float timer = 0f;
        float fakeProgress = 0f;

        float phase1Time = minLoadTime * 0.8f; // 80%的时间到80%

        asyncLoad = SceneManager.LoadSceneAsync("MainGameScene");
        asyncLoad.allowSceneActivation = false;

        while (timer < phase1Time)
        {
            timer += Time.deltaTime;
            fakeProgress = Mathf.Clamp01(timer / phase1Time) * 0.8f;
            UpdateUI(fakeProgress);
            yield return null;
        }

        float phase2Time = minLoadTime * 0.2f; // 20%的时间从80%到100%
        float phase2Timer = 0f;

        while (phase2Timer < phase2Time)
        {
            phase2Timer += Time.deltaTime;
            fakeProgress = 0.8f + (Mathf.Clamp01(phase2Timer / phase2Time) * 0.2f);
            UpdateUI(fakeProgress);
            yield return null;
        }

        //显示100%
        UpdateUI(1f);

        yield return new WaitForSeconds(0.3f);

        //确保场景已加载到可激活状态
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
    }

    private void UpdateUI(float progress)
    {
        //更新进度条
        if (loadingSlider != null)
        {
            loadingSlider.value = progress;
        }

        // 更新加载文本
        if (loadingNumber != null)
        {
            loadingNumber.text = $"{(progress * 100):F0}%";
        }
    }
}