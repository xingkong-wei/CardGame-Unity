using UnityEngine;

[CreateAssetMenu(fileName = "新卡牌类型", menuName = "Card/CardTypeData")]
public class CardTypeData : ScriptableObject
{
    [Header(" 基础信息 ")]
    public int id;
    public string typeName;
    public int index;
}
