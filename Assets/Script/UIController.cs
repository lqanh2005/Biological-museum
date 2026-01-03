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

    // Danh sách các tooltip đang được sử dụng
    public List<GameObject> activeTooltips = new List<GameObject>();
    
    // Danh sách các tooltip đã được preload (trong pool)
    public  List<GameObject> preloadedTooltips = new List<GameObject>();

    public QuizUIController quizUIController;
    public ContentController contentController;
    public TooltipUI tooltipUI;
    public Joystick joystick;

    public Button choceBtn;
    public Image imgBtn;
    public Button btnPlant, btnAnimal, btnNhanSo;
    public GameObject targetPlant, targetAnimal, targetNhanSo;
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
        PreloadTooltips(15);
        choceBtn.onClick.AddListener(() =>
        {
            imgBtn.gameObject.SetActive(!imgBtn.gameObject.activeSelf);
            // Button Plant
            btnPlant.onClick.AddListener(() =>
            {
                player.isPlant = true;
                player.isNhanSo = false;
                player.isAnimal = false;
                if (targetPlant != null)
                {
                    player.targetPlant = targetPlant.transform;
                }
                joystick.gameObject.SetActive(false);
            });

            // Button NhanSo
            btnNhanSo.onClick.AddListener(() =>
            {
                player.isNhanSo = true;
                player.isPlant = false;
                player.isAnimal = false;
                if (targetNhanSo != null)
                {
                    player.targetNhanSo = targetNhanSo.transform;
                }
                joystick.gameObject.SetActive(false);
            });

            // Button Animal
            btnAnimal.onClick.AddListener(() =>
            {
                player.isAnimal = true;
                player.isPlant = false;
                player.isNhanSo = false;
                if (targetAnimal != null)
                {
                    player.targetAnimal = targetAnimal.transform;
                }
                joystick.gameObject.SetActive(false);
            });
        });
        
    }

    /// <summary>
    /// Preload tooltip vào pool để sẵn sàng sử dụng
    /// </summary>
    /// <param name="quantity">Số lượng tooltip cần preload</param>
    public void PreloadTooltips(int quantity)
    {
        if (tooltip == null)
        {
            Debug.LogWarning("Tooltip prefab chưa được gán!");
            return;
        }

        // Preload và lấy danh sách các tooltip đã được tạo
        GameObject[] preloaded = SimplePool2.Preload(tooltip, quantity);
        
        // Thêm vào danh sách quản lý preloaded tooltips
        foreach (GameObject tooltipInstance in preloaded)
        {
            if (tooltipInstance != null && !preloadedTooltips.Contains(tooltipInstance))
            {
                preloadedTooltips.Add(tooltipInstance);
                tooltipInstance.transform.SetParent(this.gameObject.transform); // Đặt parent để giữ cảnh quan sạch sẽ
            }
        }
    }

    /// <summary>
    /// Spawn một tooltip từ pool và thêm vào danh sách quản lý
    /// </summary>
    /// <param name="position">Vị trí spawn tooltip</param>
    /// <param name="rotation">Rotation của tooltip</param>
    /// <returns>GameObject tooltip đã được spawn</returns>
    public GameObject SpawnTooltip(Vector3 position = default, Quaternion rotation = default)
    {
        if (position == default) position = Vector3.zero;
        if (rotation == default) rotation = Quaternion.identity;

        GameObject tooltipInstance = SimplePool2.Spawn(tooltip, position, rotation);
        
        if (tooltipInstance != null && !activeTooltips.Contains(tooltipInstance))
        {
            activeTooltips.Add(tooltipInstance);
        }
        
        return tooltipInstance;
    }

    /// <summary>
    /// Trả tooltip về pool và xóa khỏi danh sách quản lý
    /// </summary>
    /// <param name="tooltipInstance">Tooltip cần trả về pool</param>
    public void DespawnTooltip(GameObject tooltipInstance)
    {
        if (tooltipInstance == null) return;

        if (activeTooltips.Contains(tooltipInstance))
        {
            activeTooltips.Remove(tooltipInstance);
        }

        SimplePool2.Despawn(tooltipInstance);
    }

    /// <summary>
    /// Trả tất cả tooltip về pool
    /// </summary>
    public void DespawnAllTooltips()
    {
        // Tạo bản sao của danh sách để tránh lỗi khi modify trong lúc iterate
        List<GameObject> tooltipsToDespawn = new List<GameObject>(activeTooltips);
        
        foreach (GameObject tooltipInstance in tooltipsToDespawn)
        {
            if (tooltipInstance != null)
            {
                SimplePool2.Despawn(tooltipInstance);
            }
        }
        
        activeTooltips.Clear();
    }

    /// <summary>
    /// Lấy số lượng tooltip đang active
    /// </summary>
    /// <returns>Số lượng tooltip đang được sử dụng</returns>
    public int GetActiveTooltipCount()
    {
        // Lọc bỏ các tooltip null (có thể bị destroy)
        activeTooltips.RemoveAll(t => t == null);
        return activeTooltips.Count;
    }

    /// <summary>
    /// Lấy danh sách tất cả tooltip đang active
    /// </summary>
    /// <returns>Danh sách tooltip đang active</returns>
    public List<GameObject> GetActiveTooltips()
    {
        // Lọc bỏ các tooltip null
        activeTooltips.RemoveAll(t => t == null);
        return new List<GameObject>(activeTooltips);
    }

    /// <summary>
    /// Kiểm tra xem tooltip có đang active không
    /// </summary>
    /// <param name="tooltipInstance">Tooltip cần kiểm tra</param>
    /// <returns>True nếu tooltip đang active</returns>
    public bool IsTooltipActive(GameObject tooltipInstance)
    {
        if (tooltipInstance == null) return false;
        return activeTooltips.Contains(tooltipInstance);
    }

    /// <summary>
    /// Lấy số lượng tooltip đã được preload
    /// </summary>
    /// <returns>Số lượng tooltip đã preload</returns>
    public int GetPreloadedTooltipCount()
    {
        // Lọc bỏ các tooltip null (có thể bị destroy)
        preloadedTooltips.RemoveAll(t => t == null);
        return preloadedTooltips.Count;
    }

    /// <summary>
    /// Lấy danh sách tất cả tooltip đã được preload
    /// </summary>
    /// <returns>Danh sách tooltip đã preload</returns>
    public List<GameObject> GetPreloadedTooltips()
    {
        // Lọc bỏ các tooltip null
        preloadedTooltips.RemoveAll(t => t == null);
        return new List<GameObject>(preloadedTooltips);
    }

    /// <summary>
    /// Kiểm tra xem tooltip có được preload không
    /// </summary>
    /// <param name="tooltipInstance">Tooltip cần kiểm tra</param>
    /// <returns>True nếu tooltip đã được preload</returns>
    public bool IsTooltipPreloaded(GameObject tooltipInstance)
    {
        if (tooltipInstance == null) return false;
        return preloadedTooltips.Contains(tooltipInstance);
    }

    // Dictionary để lưu trữ mapping giữa child object và tooltip của nó
    private Dictionary<Transform, GameObject> childTooltipMapping = new Dictionary<Transform, GameObject>();

    /// <summary>
    /// Spawn tooltip từ preloadedTooltips và set vị trí tương ứng với các child của object
    /// </summary>
    /// <param name="parentTransform">Transform của parent object chứa các child</param>
    public void ShowTooltipsForChildren(Transform parentTransform)
    {
        if (parentTransform == null) return;

        // Ẩn tất cả tooltip cũ trước
        HideAllChildTooltips();

        // Thu thập tất cả các ClickAble từ children
        List<ClickAble> clickableObjects = new List<ClickAble>();

        // Đếm số lượng child cần tooltip
        int childCount = 0;
        for (int i = 0; i < parentTransform.childCount; i++)
        {
            Transform child = parentTransform.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                childCount++;
                // Lấy ClickAble component từ child
                ClickAble clickable = child.GetComponent<ClickAble>();
                if (clickable != null)
                {
                    clickableObjects.Add(clickable);
                }
            }
        }
            ShowTooltipsForClickables(clickableObjects);
            
            // Cập nhật childTooltipMapping để quản lý tooltip
            UpdateChildTooltipMapping(clickableObjects);
        
    }

    /// <summary>
    /// Cập nhật mapping giữa child objects và tooltip instances
    /// </summary>
    private void UpdateChildTooltipMapping(List<ClickAble> clickableObjects)
    {
        if (clickableObjects == null || activeTooltips == null) return;

        childTooltipMapping.Clear();

        // Tạo mapping giữa ClickAble và tooltip instance
        for (int i = 0; i < clickableObjects.Count && i < activeTooltips.Count; i++)
        {
            ClickAble clickable = clickableObjects[i];
            GameObject tooltipInstance = activeTooltips[i];
            
            if (clickable != null && tooltipInstance != null)
            {
                childTooltipMapping[clickable.transform] = tooltipInstance;
            }
        }
    }

    /// <summary>
    /// Lấy tooltip từ preloadedTooltips hoặc spawn mới từ pool
    /// </summary>
    private GameObject GetOrSpawnTooltip(int index, Transform parent)
    {
        // Spawn từ pool (sẽ tự động lấy tooltip đã preload nếu có trong pool)
        GameObject tooltipInstance = SimplePool2.Spawn(tooltip, Vector3.zero, Quaternion.identity);
        
        if (tooltipInstance != null)
        {
            // Set parent
            tooltipInstance.transform.SetParent(parent, false);
            
            // Thêm vào activeTooltips nếu chưa có
            if (!activeTooltips.Contains(tooltipInstance))
            {
                activeTooltips.Add(tooltipInstance);
            }
        }
        
        return tooltipInstance;
    }

    /// <summary>
    /// Lấy tooltip từ preloadedTooltips (ưu tiên) hoặc spawn mới từ pool nếu không có
    /// </summary>
    /// <param name="parent">Parent transform để đặt tooltip</param>
    /// <returns>GameObject tooltip instance</returns>
    private GameObject GetOrSpawnTooltipFromPreloaded(Transform parent)
    {
        // Lọc bỏ các tooltip null trong preloadedTooltips
        preloadedTooltips.RemoveAll(t => t == null);
        
        // Tìm tooltip trong preloadedTooltips nhưng chưa được sử dụng trong activeTooltips
        // Tooltip này đã được preload và đang inactive (đã được despawn về pool)
        // Ưu tiên tái sử dụng tooltip từ preloadedTooltips trước
        foreach (GameObject preloadedTooltip in preloadedTooltips)
        {
            if (preloadedTooltip != null && 
                !activeTooltips.Contains(preloadedTooltip) && 
                !preloadedTooltip.activeSelf)
            {
                // Tooltip này chưa được sử dụng và đang inactive
                // Tái sử dụng trực tiếp từ preloadedTooltips
                preloadedTooltip.SetActive(true);
                preloadedTooltip.transform.SetParent(parent, false);
                
                // Thêm vào activeTooltips để đánh dấu đang được sử dụng
                activeTooltips.Add(preloadedTooltip);
                
                return preloadedTooltip;
            }
        }
        
        // Nếu không có tooltip nào trong preloadedTooltips có thể tái sử dụng
        // (tất cả đã được sử dụng hoặc đang active), spawn từ pool
        // SimplePool2.Spawn sẽ tự động lấy từ pool's inactive queue nếu có
        // (bao gồm cả preloaded tooltips đã được despawn)
        // hoặc tạo mới nếu không có
        GameObject tooltipInstance = SpawnTooltip();
        if (tooltipInstance != null)
        {
            tooltipInstance.transform.SetParent(parent, false);
            
            // Đảm bảo tooltip được thêm vào activeTooltips (SpawnTooltip đã thêm, nhưng kiểm tra lại)
            if (!activeTooltips.Contains(tooltipInstance))
            {
                activeTooltips.Add(tooltipInstance);
            }
        }
        
        return tooltipInstance;
    }

    /// <summary>
    /// Setup tooltip cho một object cụ thể
    /// </summary>
    private void SetupTooltipForObject(GameObject tooltipInstance, Transform targetObject, Canvas canvas, Camera cam)
    {
        if (tooltipInstance == null || targetObject == null) return;

        // Lấy các component cần thiết
        RectTransform tooltipRect = tooltipInstance.GetComponent<RectTransform>();
        TMP_Text tooltipText = tooltipInstance.GetComponentInChildren<TMP_Text>();
        CanvasGroup tooltipCG = tooltipInstance.GetComponent<CanvasGroup>();
        if (!tooltipCG) tooltipCG = tooltipInstance.AddComponent<CanvasGroup>();

        // Setup RectTransform
        if (tooltipRect != null)
        {
            tooltipRect.anchorMin = new Vector2(0.5f, 0.5f);
            tooltipRect.anchorMax = new Vector2(0.5f, 0.5f);
            tooltipRect.anchoredPosition = Vector2.zero;
        }

        // Lấy text từ ClickAble hoặc tên object
        string text = "";
        ClickAble clickable = targetObject.GetComponent<ClickAble>();
        if (clickable != null && !string.IsNullOrEmpty(clickable.title))
        {
            text = clickable.title;
        }
        else
        {
            text = targetObject.gameObject.name;
        }

        // Set text
        if (tooltipText != null)
        {
            tooltipText.text = text;
        }

        // Setup CanvasGroup
        tooltipCG.alpha = 1f;
        tooltipCG.blocksRaycasts = false;
        tooltipCG.interactable = false;

        // Kích hoạt tooltip
        tooltipInstance.SetActive(true);

        // Cập nhật vị trí
        UpdateTooltipPosition(tooltipInstance, targetObject, canvas, cam);
    }

    /// <summary>
    /// Hiển thị tooltip cho parent object (khi không có child)
    /// </summary>
    private void ShowTooltipForObject(Transform targetObject, Canvas canvas, Camera cam)
    {
        GameObject tooltipInstance = GetOrSpawnTooltip(0, canvas.transform);
        if (tooltipInstance != null)
        {
            SetupTooltipForObject(tooltipInstance, targetObject, canvas, cam);
            childTooltipMapping[targetObject] = tooltipInstance;
        }
    }

    /// <summary>
    /// Cập nhật vị trí của tooltip theo vị trí của object
    /// </summary>
    private void UpdateTooltipPosition(GameObject tooltipInstance, Transform targetObject, Canvas canvas, Camera cam)
    {
        if (tooltipInstance == null || targetObject == null || canvas == null || cam == null) return;

        RectTransform tooltipRect = tooltipInstance.GetComponent<RectTransform>();
        if (tooltipRect == null) return;

        // Chuyển đổi world position sang screen position
        Vector3 worldPos = targetObject.position;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        // Nếu object ở phía sau camera, ẩn tooltip
        if (screenPos.z < 0)
        {
            tooltipInstance.SetActive(false);
            return;
        }

        tooltipInstance.SetActive(true);

        // Force update để đảm bảo kích thước chính xác
        Canvas.ForceUpdateCanvases();

        float tooltipHeight = tooltipRect.rect.height;
        float tooltipWidth = tooltipRect.rect.width;

        // Lấy offset từ tooltipUI
        Vector3 offset = tooltipUI.offset;

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
            Vector2 pivot = tooltipRect.pivot;
            localPoint.x -= (pivot.x - 0.5f) * tooltipWidth;
            localPoint.y -= (pivot.y - 0.5f) * tooltipHeight;

            // Set vị trí
            tooltipRect.localPosition = new Vector3(localPoint.x, localPoint.y, 0);
        }
    }

    /// <summary>
    /// Cập nhật vị trí của tất cả tooltip đang hiển thị
    /// </summary>
    public void UpdateAllChildTooltipPositions()
    {
        Canvas canvas = tooltipUI.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        Camera cam = canvas.worldCamera;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        // Tạo danh sách copy để tránh lỗi khi modify trong lúc iterate
        List<Transform> keysToRemove = new List<Transform>();

        foreach (var kvp in childTooltipMapping)
        {
            Transform targetObject = kvp.Key;
            GameObject tooltipInstance = kvp.Value;

            if (targetObject == null || tooltipInstance == null)
            {
                keysToRemove.Add(kvp.Key);
                continue;
            }

            UpdateTooltipPosition(tooltipInstance, targetObject, canvas, cam);
        }

        // Xóa các entry null
        foreach (var key in keysToRemove)
        {
            childTooltipMapping.Remove(key);
        }
    }

    /// <summary>
    /// Ẩn tất cả tooltip của child objects
    /// </summary>
    public void HideAllChildTooltips()
    {
        foreach (var kvp in childTooltipMapping)
        {
            if (kvp.Value != null)
            {
                DespawnTooltip(kvp.Value);
            }
        }
        childTooltipMapping.Clear();
    }

    void LateUpdate()
    {
        // Cập nhật vị trí của tất cả tooltip mỗi frame
        if (childTooltipMapping.Count > 0)
        {
            UpdateAllChildTooltipPositions();
        }
    }

    /// <summary>
    /// Hiển thị tooltip cho danh sách các ClickAble objects bằng cách duyệt qua list TooltipUI
    /// </summary>
    /// <param name="clickableObjects">Danh sách các ClickAble objects cần hiển thị tooltip</param>
    public void ShowTooltipsForClickables(List<ClickAble> clickableObjects)
    {
        if (clickableObjects == null || clickableObjects.Count == 0) return;

        // Lấy canvas từ tooltipUI
        Canvas canvas = tooltipUI.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        // Lọc bỏ các tooltip null
        activeTooltips.RemoveAll(t => t == null);

        // Đảm bảo có đủ tooltip instances
        int neededCount = clickableObjects.Count;
        int currentCount = activeTooltips.Count;

        // Lấy tooltip từ preloadedTooltips hoặc spawn mới nếu cần
        for (int i = currentCount; i < neededCount; i++)
        {
            GameObject tooltipInstance = GetOrSpawnTooltipFromPreloaded(canvas.transform);
            if (tooltipInstance != null)
            {
                // Đảm bảo TooltipUI component được khởi tạo
                TooltipUI tooltipUIComponent = tooltipInstance.GetComponent<TooltipUI>();
                if (tooltipUIComponent != null)
                {
                    tooltipUIComponent.Init();
                }
                
                // Đảm bảo tooltip được thêm vào activeTooltips nếu chưa có
                if (!activeTooltips.Contains(tooltipInstance))
                {
                    activeTooltips.Add(tooltipInstance);
                }
            }
        }

        // Lọc lại sau khi spawn
        activeTooltips.RemoveAll(t => t == null);

        // Duyệt qua list và gọi Show cho mỗi TooltipUI
        for (int i = 0; i < clickableObjects.Count && i < activeTooltips.Count; i++)
        {
            ClickAble clickable = clickableObjects[i];
            if (clickable == null) continue;

            GameObject tooltipInstance = activeTooltips[i];
            if (tooltipInstance == null) continue;

            TooltipUI tooltipUIComponent = tooltipInstance.GetComponent<TooltipUI>();
            if (tooltipUIComponent == null)
            {
                Debug.LogWarning($"Tooltip instance {i} không có TooltipUI component!");
                continue;
            }

            // Đảm bảo đã được init
            if (tooltipUIComponent.panel == null)
            {
                tooltipUIComponent.Init();
            }

            // Gọi hàm Show với ClickAble
            tooltipUIComponent.Show(clickable, clickable);
        }
    }

    /// <summary>
    /// Hiển thị tooltip cho một ClickAble object cụ thể
    /// </summary>
    /// <param name="clickable">ClickAble object cần hiển thị tooltip</param>
    public void ShowTooltipForClickable(ClickAble clickable)
    {
        if (clickable == null) return;

        // Sử dụng tooltipUI chính để hiển thị
        if (tooltipUI != null)
        {
            tooltipUI.Show(clickable, clickable);
        }
    }

}
