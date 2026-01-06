using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupInfoUI : MonoBehaviour
{
    public static PopupInfoUI Instance { get; private set; }

    [Header("Refs")]
    public RectTransform panel;
    public Image iconImage;
    public TMP_Text titleText;
    public TMP_Text descText;

    [Header("Behavior")]
    public bool appearNearMouse = true;
    public Vector2 offset = new Vector2(16, -16);

    Canvas canvas;
    CanvasGroup cg;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        canvas = GetComponentInParent<Canvas>();
        cg = panel.GetComponent<CanvasGroup>();
        if (!cg) { cg = panel.gameObject.AddComponent<CanvasGroup>(); }
        HideImmediate();
    }

    public void Show(string title, string desc, Sprite icon = null)
    {
        if (titleText) titleText.text = title;
        if (descText) descText.text = desc;

        if (iconImage)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(icon != null);
        }

        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;
        panel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
        panel.gameObject.SetActive(false);
    }

    void HideImmediate()
    {
        panel.gameObject.SetActive(false);
        if (cg) { cg.alpha = 0f; cg.blocksRaycasts = false; cg.interactable = false; }
    }

    void Update()
    {

        if (panel.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            Hide();
    }

}
