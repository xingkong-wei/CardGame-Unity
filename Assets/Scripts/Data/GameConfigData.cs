using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//游戏配置表类，每个对象对应一个txt配置表
public class GameConfigData
{
    private List<Dictionary<string, string>> dataDic;//存储配置表中的所有数据

    public GameConfigData(string str)
    {
        dataDic = new List<Dictionary<string, string>>();

        //换行切割
        string[] lines = str.Split('\n');
        //第一行是存储数据类型
        string[] title = lines[0].Trim().Split('\t');//tab切割
        //从第三行开始遍历，第二行数据是解释说明
        for (int i = 2; i < lines.Length; i++)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            string[] tempArr = lines[i].Trim().Split('\t');

            // 防止数组越界
            int minLen = Mathf.Min(title.Length, tempArr.Length);
            for (int j = 0; j < minLen; j++)
            {
                dic.Add(title[j], tempArr[j]);
            }

            // 如果数据列比标题列多，记录警告
            if (tempArr.Length > title.Length)
            {
                Debug.LogWarning($"配置表第 {i + 1} 行数据列数({tempArr.Length})多于标题列数({title.Length})，部分数据将被忽略");
            }
            // 如果数据列比标题列少，记录警告
            else if (tempArr.Length < title.Length)
            {
                Debug.LogWarning($"配置表第 {i + 1} 行数据列数({tempArr.Length})少于标题列数({title.Length})，部分字段将为空");
            }

            dataDic.Add(dic);
        }
    }

    public List<Dictionary<string, string>> GetLines()
    {
        return dataDic;
    }

    public Dictionary<string, string> GetOneById(string id)
    {
        for (int i = 0; i < dataDic.Count; i++)
        {
            Dictionary<string , string> dic = dataDic[i];
            if (dic["Id"] == id)
            {
                return dic;
            }
        }
        return null;
    }
}
