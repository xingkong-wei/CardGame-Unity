/// <summary>
/// 大法师之眼 - 所有元素亲和度上限从10层提升至15层
/// </summary>
public class ArchmagusEye : RelicBase
{
    public override int ModifyAffinityMaxStack()
    {
        return 15;
    }
}
