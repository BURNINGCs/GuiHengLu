using UnityEngine;

public class DisplayHighScoresUI : MonoBehaviour
{
    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("对象引用")]
    #endregion Header OBJECT REFERENCES
    #region Tooltip
    [Tooltip("填充子物体 Content 游戏对象的 Transform 组件")]
    #endregion Tooltip
    [SerializeField] private Transform contentAnchorTransform;

    private void Start()
    {
        DisplayScores();
    }

    //显示分数
    private void DisplayScores()
    {
        HighScores highScores = HighScoreManager.Instance.GetHighScores();
        GameObject scoreGameobject;

        //遍历分数
        int rank = 0;
        foreach (Score score in highScores.scoreList)
        {
            rank++;

            //实例化分数显示游戏对象
            scoreGameobject = Instantiate(GameResources.Instance.scorePrefab, contentAnchorTransform);

            ScorePrefab scorePrefab = scoreGameobject.GetComponent<ScorePrefab>();

            //填充数据
            scorePrefab.rankTMP.text = rank.ToString();
            scorePrefab.nameTMP.text = score.playerName;
            scorePrefab.levelTMP.text = score.levelDescription;
            scorePrefab.scoreTMP.text = score.playerScore.ToString("###,###0");
        }


        //实例化分数显示游戏对象
        scoreGameobject = Instantiate(GameResources.Instance.scorePrefab, contentAnchorTransform);
    }
}
