using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class HighScoreManager : SingletonMonobehaviour<HighScoreManager>
{
    private HighScores highScores = new HighScores();

    protected override void Awake()
    {
        base.Awake();

        LoadScores();
    }

    //从本地加载分数
    private void LoadScores()
    {
        BinaryFormatter bf = new BinaryFormatter();

        if (File.Exists(Application.persistentDataPath + "/DungeonGunnerHighScores.dat"))
        {
            ClearScoreList();

            FileStream file = File.OpenRead(Application.persistentDataPath + "/DungeonGunnerHighScores.dat");

            highScores = (HighScores)bf.Deserialize(file);

            file.Close();

        }
    }

    //清除所有分数
    private void ClearScoreList()
    {
        highScores.scoreList.Clear();
    }

    //添加分数到分数列表
    public void AddScore(Score score, int rank)
    {
        highScores.scoreList.Insert(rank - 1, score);//插入

        //限制保存的最大分数的数量
        if (highScores.scoreList.Count > Settings.numberOfHighScoresToSave)
        {
            highScores.scoreList.RemoveAt(Settings.numberOfHighScoresToSave);
        }

        SaveScores();
    }

    //保存分数到本地
    private void SaveScores()
    {
        BinaryFormatter bf = new BinaryFormatter();

        FileStream file = File.Create(Application.persistentDataPath + "/DungeonGunnerHighScores.dat");

        bf.Serialize(file, highScores);

        file.Close();
    }

    //获取高分记录
    public HighScores GetHighScores()
    {
        return highScores;
    }

    //返回玩家分数相对于其他高分的排名（如果分数不高于高分列表中的任何分数，则返回0）
    public int GetRank(long playerScore)
    {
        //如果列表中当前没有分数 - 那么这个分数必须排名第1 - 然后返回
        if (highScores.scoreList.Count == 0) return 1;

        int index = 0;

        //遍历列表中的分数以找到此分数的排名
        for (int i = 0; i < highScores.scoreList.Count; i++)
        {
            index++;

            if (playerScore >= highScores.scoreList[i].playerScore)
            {
                return index;
            }
        }

        if (highScores.scoreList.Count < Settings.numberOfHighScoresToSave)
            return (index + 1);

        return 0;
    }
}