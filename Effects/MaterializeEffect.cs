using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterializeEffect : MonoBehaviour
{
    //显形特效协程 —— 用于实现敌人显形的特殊视觉效果
    public IEnumerator MaterializeRoutine(Shader materializeShader, Color materializeColor, float materializeTime,
        SpriteRenderer[] spriteRendererArray, Material normalMaterial)
    {
        Material materializeMaterial = new Material(materializeShader);

        materializeMaterial.SetColor("_EmissionColor", materializeColor);

        //在精灵渲染器中设置显形材质
        foreach (SpriteRenderer spriteRenderer in spriteRendererArray)
        {
            spriteRenderer.material = materializeMaterial;
        }

        float dissolveAmount = 0f;

        //使敌人显形
        while (dissolveAmount < 1f)
        {
            dissolveAmount += Time.deltaTime / materializeTime;

            materializeMaterial.SetFloat("_DissolveAmount", dissolveAmount);

            yield return null;
        }

        //在精灵渲染器中设置标准材质
        foreach (SpriteRenderer spriteRenderer in spriteRendererArray)
        {
            spriteRenderer.material = normalMaterial;
        }
    }
}
