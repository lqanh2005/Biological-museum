using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    public PlayerController_Stable player;

    public GameObject tooltip;


    public QuizUIController quizUIController;
    public ContentController contentController;
    public TooltipUI tooltipUI;
    public Joystick joystick;

    public Button btnChoice;
    public Button btnMove;
    public Image imgBtn;
    public Button btnPlant, btnAnimal, btnNhanSo;
    public GameObject bottomPanel;
    public Button btnNext, btnBack;
    public TMP_Text number;

    public Button btnClose;
    public GameObject panelTut;

    public Button btnClose_1;
    public GameObject panelIntro;

    // Biến để quản lý tooltip navigation
    private List<ClickAble> currentClickableList = new List<ClickAble>();
    private int currentTooltipIndex = 0;
    private bool isNavigatingTooltips = false;
    private ClickAble currentBlinkingObject = null; // Object đang nhấp nháy
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        tooltipUI.Init();
    }

    private void Start()
    {
        panelTut.SetActive(true);
        btnClose.onClick.AddListener(() =>
        {
            panelTut.SetActive(false);
        });
        panelIntro.SetActive(true);
        btnClose_1.onClick.AddListener(() =>
        {
            panelIntro.SetActive(false);
        });
        btnMove.onClick.AddListener(() =>
        {
            imgBtn.gameObject.SetActive(false);
            bottomPanel.SetActive(false);
            joystick.gameObject.SetActive(true);
            player.isPlant = false;
            player.isNhanSo = false;
            player.isAnimal = false;
        });
        btnChoice.onClick.AddListener(() =>
        {
            imgBtn.gameObject.SetActive(!imgBtn.gameObject.activeSelf);
            bottomPanel.SetActive(!bottomPanel.activeSelf);
            btnPlant.onClick.AddListener(() =>
            {
                player.isPlant = true;
                player.isNhanSo = false;
                player.isAnimal = false;
                joystick.gameObject.SetActive(false);
            });


            btnNhanSo.onClick.AddListener(() =>
            {
                player.isNhanSo = true;
                player.isPlant = false;
                player.isAnimal = false;
                joystick.gameObject.SetActive(false);
            });


            btnAnimal.onClick.AddListener(() =>
            {
                player.isAnimal = true;
                player.isPlant = false;
                player.isNhanSo = false;
                joystick.gameObject.SetActive(false);
            });
        });

        // Setup navigation buttons
        if (btnNext != null)
        {
            btnNext.onClick.AddListener(ShowNextTooltip);
        }
        if (btnBack != null)
        {
            btnBack.onClick.AddListener(ShowPreviousTooltip);
        }
    }

    // Method mới để bắt đầu navigation mode với tooltips
    public void StartTooltipNavigation(Transform parentTransform)
    {
        if (parentTransform == null || tooltipUI == null) return;

        // Ẩn tooltip hiện tại nếu có
        if (tooltipUI != null)
        {
            tooltipUI.Hide();
        }

        // Thu thập tất cả clickable objects
        currentClickableList.Clear();
        for (int i = 0; i < parentTransform.childCount; i++)
        {
            Transform child = parentTransform.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                ClickAble clickable = child.GetComponent<ClickAble>();
                if (clickable != null && clickable.isValid)
                {
                    currentClickableList.Add(clickable);
                }
            }
        }

        if (currentClickableList.Count == 0)
        {
            Debug.LogWarning("Không tìm thấy clickable objects nào!");
            return;
        }

        // Bắt đầu navigation mode
        isNavigatingTooltips = true;
        currentTooltipIndex = 0;

        // Hiển thị tooltip đầu tiên
        ShowTooltipAtIndex(0);

        // Hiển thị navigation UI
        if (btnNext != null) btnNext.gameObject.SetActive(true);
        if (btnBack != null) btnBack.gameObject.SetActive(true);
        if (number != null) number.gameObject.SetActive(true);
    }

    // Hiển thị tooltip tại index cụ thể
    private void ShowTooltipAtIndex(int index)
    {
        if (currentClickableList == null || currentClickableList.Count == 0) return;
        if (index < 0 || index >= currentClickableList.Count) return;
        if (tooltipUI == null) return;

        // Dừng nhấp nháy object cũ
        if (currentBlinkingObject != null)
        {
            currentBlinkingObject.StopBlink();
            currentBlinkingObject = null;
        }

        // Lấy clickable object tại index
        ClickAble targetClickable = currentClickableList[index];
        if (targetClickable == null || !targetClickable.isValid) return;

        // Bắt đầu nhấp nháy object mới
        targetClickable.StartBlink();
        currentBlinkingObject = targetClickable;

        // Sử dụng tooltipUI có sẵn để hiển thị
        tooltipUI.Show(targetClickable, targetClickable);

        // Cập nhật số trang
        UpdatePageNumber();

    }

    // Hiển thị tooltip tiếp theo
    public void ShowNextTooltip()
    {
        if (!isNavigatingTooltips || currentClickableList == null || currentClickableList.Count == 0) return;

        currentTooltipIndex++;
        if (currentTooltipIndex >= currentClickableList.Count)
        {
            currentTooltipIndex = 0; // Quay lại đầu
        }

        ShowTooltipAtIndex(currentTooltipIndex);
    }

    // Hiển thị tooltip trước đó
    public void ShowPreviousTooltip()
    {
        if (!isNavigatingTooltips || currentClickableList == null || currentClickableList.Count == 0) return;

        currentTooltipIndex--;
        if (currentTooltipIndex < 0)
        {
            currentTooltipIndex = currentClickableList.Count - 1; // Quay lại cuối
        }

        ShowTooltipAtIndex(currentTooltipIndex);
    }

    // Cập nhật số trang hiển thị
    private void UpdatePageNumber()
    {
        if (number != null && currentClickableList != null && currentClickableList.Count > 0)
        {
            number.text = $"{currentTooltipIndex + 1}/{currentClickableList.Count}";
        }
    }


    // Dừng navigation mode
    public void StopTooltipNavigation()
    {
        // Dừng nhấp nháy object hiện tại
        if (currentBlinkingObject != null)
        {
            currentBlinkingObject.StopBlink();
            currentBlinkingObject = null;
        }

        isNavigatingTooltips = false;
        currentClickableList.Clear();
        currentTooltipIndex = 0;

        // Ẩn tooltip
        if (tooltipUI != null)
        {
            tooltipUI.Hide();
        }

        // Ẩn navigation UI
        if (btnNext != null) btnNext.gameObject.SetActive(false);
        if (btnBack != null) btnBack.gameObject.SetActive(false);
        if (number != null) number.gameObject.SetActive(false);
    }

    public void ShowTooltipForClickable(ClickAble clickable)
    {
        if (clickable == null || !clickable.isValid) return;

        if (tooltipUI != null)
        {
            tooltipUI.Show(clickable, clickable);
        }
    }

    // Click vào object hiện tại đang được hiển thị trong tooltip navigation
    public void ClickCurrentTooltipObject()
    {
        if (!isNavigatingTooltips || currentClickableList == null || currentClickableList.Count == 0) return;
        if (currentTooltipIndex < 0 || currentTooltipIndex >= currentClickableList.Count) return;

        ClickAble currentClickable = currentClickableList[currentTooltipIndex];
        if (currentClickable != null && currentClickable.isValid)
        {
            // Dừng nhấp nháy
            if (currentBlinkingObject != null)
            {
                currentBlinkingObject.StopBlink();
                currentBlinkingObject = null;
            }

            // Ẩn tooltip
            if (tooltipUI != null)
            {
                tooltipUI.Hide();
            }

            // Click vào object hiện tại
            currentClickable.OnClicked();
        }
    }

    // Kiểm tra xem có đang ở navigation mode không
    public bool IsNavigatingTooltips()
    {
        return isNavigatingTooltips;
    }

    // Lấy ClickAble object hiện tại đang được hiển thị
    public ClickAble GetCurrentTooltipClickable()
    {
        if (!isNavigatingTooltips || currentClickableList == null || currentClickableList.Count == 0) return null;
        if (currentTooltipIndex < 0 || currentTooltipIndex >= currentClickableList.Count) return null;
        
        return currentClickableList[currentTooltipIndex];
    }

    public void HideAll()
    {
        btnChoice.gameObject.SetActive(false);
        bottomPanel.gameObject.SetActive(false);
        btnMove.gameObject.SetActive(false);
    }
    public void ShowAll()
    {
        btnChoice.gameObject.SetActive(true);
        bottomPanel.gameObject.SetActive(true);
        btnMove.gameObject.SetActive(true);
    }
}

