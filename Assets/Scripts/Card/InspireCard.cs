using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 激励卡 - 给予自身力量Buff
/// </summary>
public class InspireCard : CardItem
{
    protected override void OnCardUsed()
    {
        base.OnCardUsed();

        int strengthGain = data != null ? GetArg0() : 1;
        
        // 添加力量Buff
        BuffManager.Instance.AddStatus(StatusType.Strength, strengthGain, -1);

        UIManager.Instance.ShowTip($"激励 +{strengthGain} 力量", new Color(0.8f, 0.4f, 0f));
        
        Vector3 pos = Camera.main.transform.position;
        pos.y = 0;
        PlayEffect(pos);
    }
}
