using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickAble : MonoBehaviour, IClickable
{
    private Renderer rend;
    [Header("Info")]
    public string title = "Object Name";
    [TextArea(2, 4)] public string description = "Short description...";
    public PathType pathType;
    public string partId;
    public Sprite icon;
    
    [Header("Sắp xếp Child")]
    public bool enableChildSorting = true;
    public float spacing = 1f; // Khoảng cách giữa các child
    public bool sortFromTop = true; // true: từ trên xuống, false: từ dưới lên
    public float startYOffset = 0f; // Offset Y ban đầu

    public void OnClicked()
    {
        UIController.instance.contentController.gameObject.SetActive(true);
        UIController.instance.contentController.Init(icon, title, description, pathType, partId);
        UIController.instance.joystick.gameObject.SetActive(false);
    }
    
    void SortChildrenByY()
    {
        if (transform.childCount == 0) return;
        
        // Lấy tất cả các child
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            children.Add(transform.GetChild(i));
        }
        
        // Sắp xếp các child theo vị trí Y hiện tại (từ cao xuống thấp)
        children.Sort((a, b) => 
        {
            if (sortFromTop)
                return b.localPosition.y.CompareTo(a.localPosition.y); // Cao -> thấp
            else
                return a.localPosition.y.CompareTo(b.localPosition.y); // Thấp -> cao
        });
        
        // Đặt lại vị trí Y cho các child
        float currentY = startYOffset;
        for (int i = 0; i < children.Count; i++)
        {
            Vector3 pos = children[i].localPosition;
            if (sortFromTop)
            {
                pos.y = currentY;
                currentY -= spacing;
            }
            else
            {
                pos.y = currentY;
                currentY += spacing;
            }
            children[i].localPosition = pos;
        }
    }
}

public interface IClickable
{
    void OnClicked();
}
public enum PathType
{
    nhan_so,
    dong_vat,
    thuc_vat
}