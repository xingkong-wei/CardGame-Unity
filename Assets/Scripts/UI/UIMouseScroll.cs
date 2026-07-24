using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 鼠标中键滚动组件
/// </summary>
public class UIMouseScroll : MonoBehaviour, IScrollHandler
{
    [Header("滚动速度（值越小越快）")]
    public float scrollSpeed = 90f;

    [Header("是否启用")]
    public bool isEnabled = true;

    // 被绑定的 ScrollRect
    private UnityEngine.UI.ScrollRect scrollRect;

    private void Awake()
    {
        // 尝试获取 ScrollRect 组件
        scrollRect = GetComponent<UnityEngine.UI.ScrollRect>();
        if (scrollRect == null)
        {
            // 尝试在子物体中查找
            scrollRect = GetComponentInChildren<UnityEngine.UI.ScrollRect>();
        }
        if (scrollRect == null)
        {
            Debug.LogError($"UIMouseScroll: ScrollRect not found on {gameObject.name}!");
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (!isEnabled || scrollRect == null)
            return;

        // 使用鼠标中键滚动
        float delta = eventData.scrollDelta.y;
        
        // 计算新的滚动位置
        float newPosition = scrollRect.verticalNormalizedPosition + (delta / scrollSpeed);
        
        // 限制范围
        newPosition = Mathf.Clamp01(newPosition);
        
        // 应用滚动
        scrollRect.verticalNormalizedPosition = newPosition;
    }
}
