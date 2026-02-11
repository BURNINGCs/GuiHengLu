using UnityEngine;

[CreateAssetMenu(fileName = "ScrollConfig_", menuName = "Scriptable Objects/Scroll Config")]
public class ScrollConfig : ScriptableObject
{
    [Header("¿ª³¡¾íÖá")]
    public Sprite openingScrollSprite;   //¿ª³¡¾íÖá±³¾°Í¼Æ¬
    public string openingTitle = "½ğÕ×Æª";
    [TextArea(3, 10)]
    public string openingContent;        //¿ª³¡¾íÖáÕıÎÄÄÚÈİ
    public Sprite openingSealSprite;     //¿ª³¡Ó¡ÕÂÍ¼Æ¬

    [Header("Ê¤Àû¾íÖá")]
    public Sprite victoryScrollSprite;   //Ê¤Àû¾íÖá±³¾°Í¼Æ¬
    public string victoryTitle = "Õ½Ê¤Æª";
    [TextArea(3, 10)]
    public string victoryContent;        //Ê¤Àû¾íÖáÕıÎÄÄÚÈİ
    public Sprite victorySealSprite;     //Ê¤ÀûÓ¡ÕÂÍ¼Æ¬

    [Header("Ê§°Ü¾íÖá")]
    public Sprite defeatScrollSprite;    //Ê§°Ü¾íÖá±³¾°Í¼Æ¬
    public string defeatTitle = "Õ½°ÜÆª";
    [TextArea(3, 10)]
    public string defeatContent;         //Ê§°Ü¾íÖáÕıÎÄÄÚÈİ
    public Sprite defeatSealSprite;      //Ê§°ÜÓ¡ÕÂÍ¼Æ¬
}