/// <summary>
/// 储存瓶 - 战斗开始时选择一种元素，获得5层该元素亲和度
/// </summary>
public class StorageVial : RelicBase
{
    public override void OnBattleStart()
    {
        ElementSelectUI selectUI = UIManager.Instance.ShowUI<ElementSelectUI>("ElementSelectUI") as ElementSelectUI;
        if (selectUI != null)
            selectUI.ShowSelectForCodex(OnElementSelected);
    }

    private void OnElementSelected(StatusType type)
    {
        BuffManager.Instance.AddStatus(type, 5);
    }
}
