using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class DoorLightingControl : MonoBehaviour
{
    private bool isLit = false;
    private Door door;

    private void Awake()
    {
        //获取组件
        door = GetComponentInParent<Door>();
    }

    //门淡入
    public void FadeInDoor(Door door)
    {
        //创建用于淡入的新材质
        Material material = new Material(GameResources.Instance.variableLitShader);

        if (!isLit)
        {
            SpriteRenderer[] spriteRendererArray = GetComponentsInParent<SpriteRenderer>();

            foreach (SpriteRenderer spriteRenderer in spriteRendererArray)
            {
                StartCoroutine(FadeInDoorRoutine(spriteRenderer, material));
            }

            isLit = true;
        }
    }

    //门淡入协程
    private IEnumerator FadeInDoorRoutine(SpriteRenderer spriteRenderer, Material material)
    {
        spriteRenderer.material = material;

        for (float i = 0.05f; i <= 1f; i += Time.deltaTime / Settings.fadeInTime)
        {
            material.SetFloat("Alpha_Slider", i);
            yield return null;
        }

        spriteRenderer.material = GameResources.Instance.litMaterial;
    }

    //若被触发，则将门淡入
    private void OnTriggerEnter2D(Collider2D collision)
    {
        FadeInDoor(door);
    }
}
