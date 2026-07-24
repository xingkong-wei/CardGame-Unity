using UnityEngine;

/// <summary>
/// 调和瓶 - 每回合首次获得一种元素亲和度时，10%概率额外获得其他两种元素各1层
/// </summary>
public class ConcoctionVial : RelicBase
{
    private const float TRIGGER_CHANCE = 0.1f;

    private int triggeredThisTurn = -1;
    private bool isProcessing;

    public override void OnTurnStart()
    {
        triggeredThisTurn = -1;
    }

    public override void OnAffinityChanged(StatusType type, int currentStack)
    {
        if (isProcessing) return;

        if (type != StatusType.FireAffinity && type != StatusType.IceAffinity && type != StatusType.LightningAffinity)
            return;

        int typeIndex = (int)type;
        if (triggeredThisTurn == typeIndex)
            return;

        triggeredThisTurn = typeIndex;

        if (Random.value > TRIGGER_CHANCE)
            return;

        isProcessing = true;

        if (type != StatusType.FireAffinity)
            BuffManager.Instance.AddStatus(StatusType.FireAffinity, 1);
        if (type != StatusType.IceAffinity)
            BuffManager.Instance.AddStatus(StatusType.IceAffinity, 1);
        if (type != StatusType.LightningAffinity)
            BuffManager.Instance.AddStatus(StatusType.LightningAffinity, 1);

        isProcessing = false;
    }
}
