using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 虚弱打击 - 给敌人添加虚弱Debuff
/// </summary>
public class WeaknessCard : CardItem
{
    protected override void OnCardUsed()
    {
        base.OnCardUsed();

        int weakStacks = data != null ? GetArg0() : 1;
        
        // 添加虚弱Buff到敌人（通过Enemy的Hit方法传递）
        // 这里需要修改敌人的Buff系统，先简化处理
        UIManager.Instance.ShowTip($"敌人获得虚弱 -{weakStacks}", new Color(0.5f, 0.2f, 0.8f));
        
        Vector3 pos = Camera.main.transform.position;
        pos.y = 0;
        PlayEffect(pos);
    }
}
