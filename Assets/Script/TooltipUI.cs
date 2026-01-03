using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TooltipUI : MonoBehaviour
{

    [Header("Refs")]
    public RectTransform panel;
    public TMP_Text textTooltip;
    public Image backgroundImage;
    public Transform target;


    [Header("Behavior")]
    public Vector3 offset = new Vector3(0, 20,0); // Offset từ vị trí chuột (X: ngang, Y: lên trên)
    public float followSpeed = 10f; // Tốc độ di chuyển tooltip theo chuột (không dùng nữa)

    private Canvas canvas;
    private CanvasGroup cg;
    private IClickable currentHovered;
    private Transform targetWorldObject; // Object trong world để hiển thị tooltip tại vị trí của nó
    
    // Hệ thống quản lý nhiều tooltip instances cho child objects
    private List<ChildTooltipData> activeChildTooltips = new List<ChildTooltipData>();
    
    [System.Serializable]
    private class ChildTooltipData
    {
        public RectTransform panel;
        public TMP_Text text;
        public Transform targetObject;
        public CanvasGroup canvasGroup;
    }

    public void Init()
    {
        canvas = GetComponentInParent<Canvas>();
        cg = panel.GetComponent<CanvasGroup>();
        if (!cg) { cg = panel.gameObject.AddComponent<CanvasGroup>(); }
        
        // Đảm bảo panel được setup đúng cho Screen Space Overlay
        if (panel != null && canvas != null)
        {
            RectTransform panelRect = panel;
            // Đảm bảo panel không bị constraint bởi Layout Group
            LayoutGroup layoutGroup = panelRect.GetComponent<LayoutGroup>();
            if (layoutGroup != null)
            {
                layoutGroup.enabled = false; // Tắt layout group để có thể tự do di chuyển
            }
            
            // Đảm bảo anchor ở center để dùng localPosition
            if (panelRect.anchorMin != panelRect.anchorMax || 
                panelRect.anchorMin != new Vector2(0.5f, 0.5f))
            {
                // Set anchor về center nếu chưa đúng
                panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                panelRect.anchoredPosition = Vector2.zero;
            }
        }
        
        Hide();
    }

    void Update()
    {
        if (panel.gameObject.activeSelf)
        {
            if (targetWorldObject != null)
            {
                // Cập nhật vị trí dựa trên object (dùng UpdatePositionAtObject thay vì UpdatePositionAtWorldObject)
                UpdatePositionAtObject(targetWorldObject);
            }
            else
            {
                UpdatePosition();
            }
        }
        
        // Cập nhật vị trí của tất cả child tooltips
        UpdateAllChildTooltips();
    }
    void LateUpdate()
    {
        if (target == null || !gameObject.activeSelf) return;

        // Chuyển vị trí từ 3D sang 2D màn hình
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position);

        // Cập nhật vị trí của Tooltip UI
        transform.position = screenPos + offset;

        // Ẩn nếu bộ phận nằm sau lưng Camera (tránh lỗi hiển thị đè)
        GetComponent<CanvasGroup>().alpha = (screenPos.z > 0) ? 1 : 0;
    }

    public void Show(string text, IClickable clickable = null)
    {
        if (string.IsNullOrEmpty(text)) return;

        currentHovered = clickable;
        if (textTooltip) textTooltip.text = text;

        // Đảm bảo panel được setup đúng trước khi hiển thị
        // Force rebuild layout để có kích thước chính xác
        Canvas.ForceUpdateCanvases();
        
        cg.alpha = 1f;
        cg.blocksRaycasts = false; // Không chặn raycast
        cg.interactable = false;
        panel.gameObject.SetActive(true);

        UpdatePosition();
    }

    public void Hide()
    {
        currentHovered = null;
        targetWorldObject = null;
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
        panel.gameObject.SetActive(false);
        
        // Không ẩn child tooltips ở đây - chúng sẽ được quản lý riêng
        // Chỉ ẩn khi gọi HideAllChildTooltips() hoặc khi cần thiết
    }
    
    /// <summary>
    /// Ẩn tất cả tooltips (cả chính và child tooltips)
    /// </summary>
    public void HideAll()
    {
        Hide();
        HideAllChildTooltips();
    }
    
    
    
    
    
    /// <summary>
    /// Cập nhật vị trí của tất cả child tooltips
    /// </summary>
    void UpdateAllChildTooltips()
    {
        for (int i = activeChildTooltips.Count - 1; i >= 0; i--)
        {
            ChildTooltipData data = activeChildTooltips[i];
            
            // Kiểm tra nếu object đã bị destroy
            if (data.targetObject == null || data.panel == null)
            {
                if (data.panel != null) Destroy(data.panel.gameObject);
                activeChildTooltips.RemoveAt(i);
                continue;
            }
            
            UpdateChildTooltipPosition(data);
        }
    }
    
    /// <summary>
    /// Cập nhật vị trí của một child tooltip
    /// </summary>
    void UpdateChildTooltipPosition(ChildTooltipData data)
    {
        if (data.panel == null || data.targetObject == null) return;
        
        // Lấy camera
        Camera cam = canvas.worldCamera;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        
        // Chuyển đổi world position sang screen position
        Vector3 worldPos = data.targetObject.position;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
        
        // Nếu object ở phía sau camera, ẩn tooltip
        if (screenPos.z < 0)
        {
            data.panel.gameObject.SetActive(false);
            return;
        }
        
        data.panel.gameObject.SetActive(true);
        
        // Force update để đảm bảo kích thước chính xác
        Canvas.ForceUpdateCanvases();
        
        float tooltipHeight = data.panel.rect.height;
        float tooltipWidth = data.panel.rect.width;
        
        // Tính toán vị trí tooltip trên màn hình
        Vector2 tooltipScreenPos = new Vector2(
            screenPos.x + offset.x,
            screenPos.y + offset.y + tooltipHeight
        );
        
        // Chuyển đổi từ screen space sang canvas local space
        Vector2 localPoint;
        RectTransform canvasRect = canvas.transform as RectTransform;
        
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            tooltipScreenPos,
            cam,
            out localPoint))
        {
            // Điều chỉnh dựa trên pivot của panel
            Vector2 pivot = data.panel.pivot;
            localPoint.x -= (pivot.x - 0.5f) * tooltipWidth;
            localPoint.y -= (pivot.y - 0.5f) * tooltipHeight;
            
            // Set vị trí
            data.panel.localPosition = new Vector3(localPoint.x, localPoint.y, 0);
        }
    }
    
    /// <summary>
    /// Ẩn tất cả child tooltips
    /// </summary>
    void HideAllChildTooltips()
    {
        foreach (var tooltipData in activeChildTooltips)
        {
            if (tooltipData.panel != null)
            {
                Destroy(tooltipData.panel.gameObject);
            }
        }
        activeChildTooltips.Clear();
    }
    
    

    void UpdatePosition()
    {
        if (!canvas || !panel) return;

        Vector2 mousePos = Input.mousePosition;
        RectTransform panelRect = panel;
        
        // Force update để đảm bảo kích thước chính xác
        Canvas.ForceUpdateCanvases();
        
        float tooltipHeight = panelRect.rect.height;
        float tooltipWidth = panelRect.rect.width;
        
        // Tính toán vị trí tooltip trên màn hình: chuột + offset (bên trên chuột)
        Vector2 tooltipScreenPos = new Vector2(
            mousePos.x + offset.x,
            mousePos.y + offset.y + tooltipHeight
        );
        
        // Với Screen Space Overlay, dùng RectTransformUtility để chuyển đổi
        // Đây là cách Unity khuyến nghị và hoạt động tốt nhất
        Vector2 localPoint;
        RectTransform canvasRect = canvas.transform as RectTransform;
        
        // Chuyển đổi từ screen space sang canvas local space
        // Với Screen Space Overlay, camera = null
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            tooltipScreenPos,
            null, // Screen Space Overlay không cần camera
            out localPoint))
        {
            // Điều chỉnh dựa trên pivot của panel
            Vector2 pivot = panelRect.pivot;
            localPoint.x -= (pivot.x - 0.5f) * tooltipWidth;
            localPoint.y -= (pivot.y - 0.5f) * tooltipHeight;
            
            // Set vị trí
            panelRect.localPosition = new Vector3(localPoint.x, localPoint.y, 0);
        }
        else
        {
            // Fallback: tính toán thủ công nếu API thất bại
            RectTransform canvasRectTransform = canvasRect;
            float canvasWidth = canvasRectTransform.rect.width;
            float canvasHeight = canvasRectTransform.rect.height;
            
            if (canvasWidth <= 0) canvasWidth = Screen.width;
            if (canvasHeight <= 0) canvasHeight = Screen.height;
            
            Vector2 calculatedPos = new Vector2(
                (tooltipScreenPos.x / Screen.width - 0.5f) * canvasWidth,
                (tooltipScreenPos.y / Screen.height - 0.5f) * canvasHeight
            );
            
            // Điều chỉnh pivot
            Vector2 pivot = panelRect.pivot;
            calculatedPos.x -= (pivot.x - 0.5f) * tooltipWidth;
            calculatedPos.y -= (pivot.y - 0.5f) * tooltipHeight;
            
            panelRect.localPosition = new Vector3(calculatedPos.x, calculatedPos.y, 0);
        }
        
        // Đảm bảo tooltip không ra ngoài màn hình (tùy chọn)
        // Có thể bỏ qua phần này nếu muốn tooltip luôn theo chuột
    }
    public void Show(ClickAble gameObject, IClickable clickable = null)
    {
        if (gameObject == null) return;

        currentHovered = clickable;
        if (textTooltip) textTooltip.text = gameObject.title;

        // Lưu lại target object để cập nhật vị trí mỗi frame
        targetWorldObject = gameObject.transform;

        // Đảm bảo panel được setup đúng trước khi hiển thị
        // Force rebuild layout để có kích thước chính xác
        Canvas.ForceUpdateCanvases();

        cg.alpha = 1f;
        cg.blocksRaycasts = false; // Không chặn raycast
        cg.interactable = false;
        panel.gameObject.SetActive(true);

        UpdatePositionAtObject(gameObject.transform);
    }
    /// <summary>
    /// Cập nhật vị trí tooltip dựa trên vị trí của một object thay vì chuột
    /// </summary>
    /// <param name="targetObject">Transform của object cần hiển thị tooltip tại vị trí của nó</param>
    void UpdatePositionAtObject(Transform targetObject)
    {
        if (!canvas || !panel || targetObject == null) return;

        // Lấy camera (có thể là Camera.main hoặc camera từ canvas)
        Camera cam = canvas.worldCamera;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        // Chuyển đổi world position sang screen position
        Vector3 worldPos = targetObject.position;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
        
        // Nếu object ở phía sau camera, không cập nhật vị trí
        if (screenPos.z < 0)
        {
            return;
        }

        RectTransform panelRect = panel;
        
        // Force update để đảm bảo kích thước chính xác
        Canvas.ForceUpdateCanvases();
        
        float tooltipHeight = panelRect.rect.height;
        float tooltipWidth = panelRect.rect.width;
        
        // Tính toán vị trí tooltip trên màn hình: vị trí object + offset (bên trên object)
        Vector2 tooltipScreenPos = new Vector2(
            screenPos.x + offset.x,
            screenPos.y + offset.y + tooltipHeight
        );
        
        // Với Screen Space Overlay, dùng RectTransformUtility để chuyển đổi
        // Đây là cách Unity khuyến nghị và hoạt động tốt nhất
        Vector2 localPoint;
        RectTransform canvasRect = canvas.transform as RectTransform;
        
        // Chuyển đổi từ screen space sang canvas local space
        // Với Screen Space Overlay, camera = null; với các loại canvas khác, cần camera
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            tooltipScreenPos,
            cam, // null cho Screen Space Overlay, camera cho các trường hợp khác
            out localPoint))
        {
            // Điều chỉnh dựa trên pivot của panel
            Vector2 pivot = panelRect.pivot;
            localPoint.x -= (pivot.x - 0.5f) * tooltipWidth;
            localPoint.y -= (pivot.y - 0.5f) * tooltipHeight;
            
            // Set vị trí
            panelRect.localPosition = new Vector3(localPoint.x, localPoint.y, 0);
        }
        else
        {
            // Fallback: tính toán thủ công nếu API thất bại
            RectTransform canvasRectTransform = canvasRect;
            float canvasWidth = canvasRectTransform.rect.width;
            float canvasHeight = canvasRectTransform.rect.height;
            
            if (canvasWidth <= 0) canvasWidth = Screen.width;
            if (canvasHeight <= 0) canvasHeight = Screen.height;
            
            Vector2 calculatedPos = new Vector2(
                (tooltipScreenPos.x / Screen.width - 0.5f) * canvasWidth,
                (tooltipScreenPos.y / Screen.height - 0.5f) * canvasHeight
            );
            
            // Điều chỉnh pivot
            Vector2 pivot = panelRect.pivot;
            calculatedPos.x -= (pivot.x - 0.5f) * tooltipWidth;
            calculatedPos.y -= (pivot.y - 0.5f) * tooltipHeight;
            
            panelRect.localPosition = new Vector3(calculatedPos.x, calculatedPos.y, 0);
        }
        
        // Đảm bảo tooltip không ra ngoài màn hình (tùy chọn)
        // Có thể bỏ qua phần này nếu muốn tooltip luôn theo object
    }

    public bool IsShowing(IClickable clickable)
    {
        return panel.gameObject.activeSelf && currentHovered == clickable;
    }

}

