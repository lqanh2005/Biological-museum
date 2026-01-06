using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickAble : MonoBehaviour, IClickable
{
    [Header("Info")]
    public string title = "Object Name";
    [TextArea(2, 4)] public string description = "Short description...";
    public PathType pathType;
    public string partId;
    public Sprite icon;

    [Header("Sắp xếp Child")]
    public bool isValid = true;
    public float spacing = 1f;
    public bool sortFromTop = true;
    public float startYOffset = 0f;

    [Header("Blink Effect")]
    public Color blinkColor = Color.yellow;
    public float blinkSpeed = 2f; // Số lần nhấp nháy mỗi giây
    public float blinkIntensity = 1.5f; // Độ sáng khi nhấp nháy

    private Renderer rend;
    private Coroutine blinkCoroutine;
    private Material[] originalMaterials;
    private Material[] blinkMaterials;
    private bool isBlinking = false;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            rend = GetComponentInChildren<Renderer>();
        }
        
        if (rend != null)
        {
            originalMaterials = rend.materials;
            blinkMaterials = new Material[originalMaterials.Length];
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                blinkMaterials[i] = new Material(originalMaterials[i]);
            }
        }
    }

    public void OnClicked()
    {
        UIController.instance.contentController.gameObject.SetActive(true);
        UIController.instance.contentController.Init(icon, title, description, pathType, partId);
        UIController.instance.joystick.gameObject.SetActive(false);
    }

    // Bắt đầu nhấp nháy
    public void StartBlink()
    {
        if (isBlinking) return;
        if (rend == null) return;

        isBlinking = true;
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        blinkCoroutine = StartCoroutine(BlinkCoroutine());
    }

    // Dừng nhấp nháy
    public void StopBlink()
    {
        if (!isBlinking) return;

        isBlinking = false;
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        // Khôi phục material ban đầu
        if (rend != null && originalMaterials != null)
        {
            rend.materials = originalMaterials;
        }
    }

    // Coroutine để làm nhấp nháy bằng emission color
    private IEnumerator BlinkCoroutine()
    {
        while (isBlinking)
        {
            // Sử dụng PingPong để tạo hiệu ứng nhấp nháy liên tục
            float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            
            // Thay đổi emission color để tạo hiệu ứng nhấp nháy
            if (rend != null && blinkMaterials != null)
            {
                rend.materials = blinkMaterials;
                foreach (Material mat in blinkMaterials)
                {
                    if (mat != null && mat.HasProperty("_EmissionColor"))
                    {
                        Color emissionColor = Color.Lerp(Color.black, blinkColor * blinkIntensity, t);
                        mat.SetColor("_EmissionColor", emissionColor);
                        mat.EnableKeyword("_EMISSION");
                    }
                }
            }

            yield return null;
        }
    }

    void OnDestroy()
    {
        StopBlink();
        
        // Cleanup materials
        if (blinkMaterials != null)
        {
            foreach (Material mat in blinkMaterials)
            {
                if (mat != null)
                {
                    Destroy(mat);
                }
            }
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