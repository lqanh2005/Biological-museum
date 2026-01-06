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
    public Vector3 offset = new Vector3(0, 20,0);
    public float followSpeed = 10f;

    private Canvas canvas;
    private CanvasGroup cg;
    private IClickable currentHovered;
    private Transform targetWorldObject;


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


        if (panel != null && canvas != null)
        {
            RectTransform panelRect = panel;

            LayoutGroup layoutGroup = panelRect.GetComponent<LayoutGroup>();
            if (layoutGroup != null)
            {
                layoutGroup.enabled = false;
            }

            if (panelRect.anchorMin != panelRect.anchorMax || 
                panelRect.anchorMin != new Vector2(0.5f, 0.5f))
            {

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
                UpdatePositionAtObject(targetWorldObject);
            }
            else
            {
                UpdatePosition();
            }
        }
        UpdateAllChildTooltips();
    }
    void LateUpdate()
    {
        if (target == null || !gameObject.activeSelf) return;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position);
        transform.position = screenPos + offset;
        GetComponent<CanvasGroup>().alpha = (screenPos.z > 0) ? 1 : 0;
    }

    public void Show(string text, IClickable clickable = null)
    {
        if (string.IsNullOrEmpty(text)) return;

        currentHovered = clickable;
        if (textTooltip) textTooltip.text = text;

        Canvas.ForceUpdateCanvases();

        cg.alpha = 1f;
        cg.blocksRaycasts = false;
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

    }

    public void HideAll()
    {
        Hide();
        HideAllChildTooltips();
    }

    void UpdateAllChildTooltips()
    {
        for (int i = activeChildTooltips.Count - 1; i >= 0; i--)
        {
            ChildTooltipData data = activeChildTooltips[i];


            if (data.targetObject == null || data.panel == null)
            {
                if (data.panel != null) Destroy(data.panel.gameObject);
                activeChildTooltips.RemoveAt(i);
                continue;
            }

            UpdateChildTooltipPosition(data);
        }
    }

    void UpdateChildTooltipPosition(ChildTooltipData data)
    {
        if (data.panel == null || data.targetObject == null) return;


        Camera cam = canvas.worldCamera;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;


        Vector3 worldPos = data.targetObject.position;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);


        if (screenPos.z < 0)
        {
            data.panel.gameObject.SetActive(false);
            return;
        }

        data.panel.gameObject.SetActive(true);


        Canvas.ForceUpdateCanvases();

        float tooltipHeight = data.panel.rect.height;
        float tooltipWidth = data.panel.rect.width;


        Vector2 tooltipScreenPos = new Vector2(
            screenPos.x + offset.x,
            screenPos.y + offset.y + tooltipHeight
        );

        Vector2 localPoint;
        RectTransform canvasRect = canvas.transform as RectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            tooltipScreenPos,
            cam,
            out localPoint))
        {
            Vector2 pivot = data.panel.pivot;
            localPoint.x -= (pivot.x - 0.5f) * tooltipWidth;
            localPoint.y -= (pivot.y - 0.5f) * tooltipHeight;
            data.panel.localPosition = new Vector3(localPoint.x, localPoint.y, 0);
        }
    }

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
        Canvas.ForceUpdateCanvases();

        float tooltipHeight = panelRect.rect.height;
        float tooltipWidth = panelRect.rect.width;
        Vector2 tooltipScreenPos = new Vector2(
            mousePos.x + offset.x,
            mousePos.y + offset.y + tooltipHeight
        );

        Vector2 localPoint;
        RectTransform canvasRect = canvas.transform as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            tooltipScreenPos,
            null,
            out localPoint))
        {

            Vector2 pivot = panelRect.pivot;
            localPoint.x -= (pivot.x - 0.5f) * tooltipWidth;
            localPoint.y -= (pivot.y - 0.5f) * tooltipHeight;


            panelRect.localPosition = new Vector3(localPoint.x, localPoint.y, 0);
        }
        else
        {

            RectTransform canvasRectTransform = canvasRect;
            float canvasWidth = canvasRectTransform.rect.width;
            float canvasHeight = canvasRectTransform.rect.height;

            if (canvasWidth <= 0) canvasWidth = Screen.width;
            if (canvasHeight <= 0) canvasHeight = Screen.height;

            Vector2 calculatedPos = new Vector2(
                (tooltipScreenPos.x / Screen.width - 0.5f) * canvasWidth,
                (tooltipScreenPos.y / Screen.height - 0.5f) * canvasHeight
            );


            Vector2 pivot = panelRect.pivot;
            calculatedPos.x -= (pivot.x - 0.5f) * tooltipWidth;
            calculatedPos.y -= (pivot.y - 0.5f) * tooltipHeight;

            panelRect.localPosition = new Vector3(calculatedPos.x, calculatedPos.y, 0);
        }
    }
    public void Show(ClickAble gameObject, IClickable clickable = null)
    {
        if (gameObject == null || !gameObject.isValid) return;

        currentHovered = clickable;
        if (textTooltip) textTooltip.text = gameObject.title;


        targetWorldObject = gameObject.transform;



        Canvas.ForceUpdateCanvases();

        cg.alpha = 1f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
        panel.gameObject.SetActive(true);

        UpdatePositionAtObject(gameObject.transform);
    }

    void UpdatePositionAtObject(Transform targetObject)
    {
        if (!canvas || !panel || targetObject == null) return;

        // Xác định camera chính xác
        Camera cam = canvas.worldCamera;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        // Tính toán world position chính xác của child object
        Vector3 worldPos = GetAccurateWorldPosition(targetObject);

        // Chuyển đổi world position sang screen position
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        // Kiểm tra object có ở phía trước camera không
        if (screenPos.z < 0)
        {
            panel.gameObject.SetActive(false);
            return;
        }

        panel.gameObject.SetActive(true);

        RectTransform panelRect = panel;
        Canvas.ForceUpdateCanvases();

        float tooltipHeight = panelRect.rect.height;
        float tooltipWidth = panelRect.rect.width;

        // Tính toán screen position cho tooltip (thêm offset)
        Vector2 tooltipScreenPos = new Vector2(
            screenPos.x + offset.x,
            screenPos.y + offset.y + tooltipHeight
        );

        // Chuyển đổi screen position sang local position trong canvas
        RectTransform canvasRect = canvas.transform as RectTransform;
        Vector2 localPoint;

        // Xử lý theo render mode của canvas
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // Screen Space - Overlay: không cần camera
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                tooltipScreenPos,
                null,
                out localPoint))
            {
                // Điều chỉnh theo pivot
                Vector2 pivot = panelRect.pivot;
                localPoint.x -= (pivot.x - 0.5f) * tooltipWidth;
                localPoint.y -= (pivot.y - 0.5f) * tooltipHeight;
                panelRect.localPosition = new Vector3(localPoint.x, localPoint.y, 0);
            }
            else
            {
                // Fallback: tính toán thủ công
                float canvasWidth = canvasRect.rect.width;
                float canvasHeight = canvasRect.rect.height;
                if (canvasWidth <= 0) canvasWidth = Screen.width;
                if (canvasHeight <= 0) canvasHeight = Screen.height;

                Vector2 calculatedPos = new Vector2(
                    (tooltipScreenPos.x / Screen.width - 0.5f) * canvasWidth,
                    (tooltipScreenPos.y / Screen.height - 0.5f) * canvasHeight
                );

                Vector2 pivot = panelRect.pivot;
                calculatedPos.x -= (pivot.x - 0.5f) * tooltipWidth;
                calculatedPos.y -= (pivot.y - 0.5f) * tooltipHeight;
                panelRect.localPosition = new Vector3(calculatedPos.x, calculatedPos.y, 0);
            }
        }
        else
        {
            // Screen Space - Camera hoặc World Space: cần camera
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                tooltipScreenPos,
                cam,
                out localPoint))
            {
                // Điều chỉnh theo pivot
                Vector2 pivot = panelRect.pivot;
                localPoint.x -= (pivot.x - 0.5f) * tooltipWidth;
                localPoint.y -= (pivot.y - 0.5f) * tooltipHeight;
                panelRect.localPosition = new Vector3(localPoint.x, localPoint.y, 0);
            }
        }
    }

    // Tính toán world position chính xác của object
    Vector3 GetAccurateWorldPosition(Transform targetObject)
    {
        if (targetObject == null) return Vector3.zero;

        // Thử lấy Renderer để tính toán bounds chính xác
        Renderer renderer = targetObject.GetComponent<Renderer>();
        if (renderer != null && renderer.bounds.size != Vector3.zero)
        {
            // Sử dụng top center của bounds để tooltip hiển thị phía trên object
            Bounds bounds = renderer.bounds;
            return new Vector3(
                bounds.center.x,
                bounds.max.y, // Top của object
                bounds.center.z
            );
        }

        // Nếu không có Renderer, thử tìm trong children
        Renderer[] childRenderers = targetObject.GetComponentsInChildren<Renderer>();
        if (childRenderers != null && childRenderers.Length > 0)
        {
            Bounds combinedBounds = childRenderers[0].bounds;
            for (int i = 1; i < childRenderers.Length; i++)
            {
                if (childRenderers[i].bounds.size != Vector3.zero)
                {
                    combinedBounds.Encapsulate(childRenderers[i].bounds);
                }
            }
            
            if (combinedBounds.size != Vector3.zero)
            {
                return new Vector3(
                    combinedBounds.center.x,
                    combinedBounds.max.y, // Top của combined bounds
                    combinedBounds.center.z
                );
            }
        }

        // Fallback: sử dụng position của transform
        return targetObject.position;
    }

    public bool IsShowing(IClickable clickable)
    {
        return panel.gameObject.activeSelf && currentHovered == clickable;
    }

}

