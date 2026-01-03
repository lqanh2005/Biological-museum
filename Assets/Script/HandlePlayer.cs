using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.AI;
using System;

[RequireComponent(typeof(CharacterController))]
public class PlayerController_Stable : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 6f;
    public float sprintMultiplier = 1.5f;
    public float jumpHeight = 1.2f;
    public float gravity = -20f;

    [Header("Look (Drag Mouse/Touch)")]
    public Transform cameraRoot;                 // pivot (x pitch)
    public float mouseSensitivity = 120f;        // Desktop/WebGL
    [Range(0.1f, 1f)] public float webglMobileSensitivityMul = 0.65f; // giảm nếu WebGL mobile gắt
    public float minPitch = -85f;
    public float maxPitch = 85f;

    [Header("Joystick Move (Optional)")]
    public Joystick moveJoystick;                // joystick trái
    public float joystickDeadzone = 0.15f;
    public bool useJoystickOnly = true;          // mobile bật true; PC muốn test WASD thì false

    [Header("Click / Hover (World)")]
    public bool enableWorldClick = true;
    public bool enableHover = true;
    public float clickMaxDuration = 0.25f;
    public float dragStartPixels = 6f;
    public float rayDistance = 100f;
    public LayerMask clickableMask = ~0;

    [Header("Cursor (PC/WebGL)")]
    public bool lockCursorWhileRotating = true;

    // --- internals
    CharacterController cc;
    Camera cam;

    Vector3 velocity;
    float pitch;

    // Pointer state
    bool pointerDown;
    bool pointerStartedOnUI;
    bool rotating;
    bool isMoving;
    Vector2 pointerDownPos;
    float pointerDownTime;
    public NavMeshAgent agent;
    public bool isPlant;
    public Transform targetPlant;
    public bool isNhanSo;
    public Transform targetNhanSo;
    public bool isAnimal;
    public Transform targetAnimal;
    
    [Header("Auto Move Rotation")]
    public float rotationSpeed = 5f;  // Tốc độ quay khi auto move
    
    // Track xem đã hiển thị tooltip chưa để tránh hiển thị nhiều lần
    private bool hasShownTooltips = false;
    private Transform lastTargetWithTooltips = null;
    [SerializeField] private Transform currentTarget = null; // Track target hiện tại để phát hiện thay đổi

    // Hover tracking
    IClickable currentHovered;

    // UI raycast cache (no GC)
    PointerEventData ped;
    readonly List<RaycastResult> uiHits = new List<RaycastResult>(16);

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        agent = GetComponent<NavMeshAgent>();

        if (cameraRoot == null && Camera.main) cameraRoot = Camera.main.transform;

        cam = Camera.main;
        if (!cam && cameraRoot) cam = cameraRoot.GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Cấu hình NavMeshAgent để tự động di chuyển
        if (agent != null)
        {
            agent.updatePosition = true;   // NavMeshAgent tự động cập nhật vị trí
            agent.updateRotation = false;  // Tắt auto rotation để tự điều khiển quay mượt
        }
    }

    void Update()
    {
        HandlePointerState();   // click/drag detect (rotating)
        HandleLook();           // drag mouse/touch to look
        
        // Chỉ di chuyển bằng CharacterController khi không phải chế độ auto move
        if (!isPlant && !isNhanSo && !isAnimal)
        {
            HandleMove();       // joystick/WASD move
        }
        
        HandleHover();          // optional hover tooltip
        AutoMove();
    }

    private void AutoMove()
    {
        Transform target = null;
        bool shouldAutoMove = false;
        
        // Xác định target dựa trên loại auto move (ưu tiên theo thứ tự: Plant > NhanSo > Animal)
        if (isPlant && targetPlant != null)
        {
            target = targetPlant;
            shouldAutoMove = true;
        }
        else if (isNhanSo && targetNhanSo != null)
        {
            target = targetNhanSo;
            shouldAutoMove = true;
        }
        else if (isAnimal && targetAnimal != null)
        {
            target = targetAnimal;
            shouldAutoMove = true;
        }
        
        if (shouldAutoMove && agent != null && target != null)
        {
            // Disable CharacterController để NavMeshAgent có thể tự động di chuyển
            if (cc != null && cc.enabled)
            {
                cc.enabled = false;
            }
            
            // Đảm bảo NavMeshAgent được enable
            if (!agent.enabled)
            {
                agent.enabled = true;
            }
            
            // Kiểm tra xem agent có ở trên NavMesh không
            if (!agent.isOnNavMesh)
            {
                Debug.LogWarning("NavMeshAgent không ở trên NavMesh! Vui lòng đảm bảo có NavMesh trong scene và nhân vật đang ở trên NavMesh.");
                return;
            }
            
            // Kiểm tra nếu target thay đổi - reset các flag và cho phép di chuyển
            if (currentTarget != target)
            {
                currentTarget = target;
                hasShownTooltips = false;
                lastTargetWithTooltips = target;
                // Reset isStopped để agent có thể di chuyển đến target mới
                agent.isStopped = false;
            }
            
            // Đặt đích đến - NavMeshAgent sẽ tự động di chuyển đến đó
            agent.SetDestination(target.position);
            
            // Kiểm tra xem đã đến đích chưa
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
            {
                // Đã đến đích - dừng agent
                agent.isStopped = true;
                
                // Quay về phía target
                Vector3 direction = (target.position - transform.position);
                direction.y = 0;
                if (direction.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
                
                // Hiển thị tooltip cho tất cả child objects (chỉ một lần)
                if (!hasShownTooltips && UIController.instance != null)
                {
                    UIController.instance.ShowTooltipsForChildren(target);
                    hasShownTooltips = true;
                }
            }
            else
            {
                // Đang di chuyển - quay mượt mà về phía target
                Vector3 direction = (target.position - transform.position);
                direction.y = 0; // Chỉ quay theo trục Y (ngang)
                if (direction.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
            
            // Ẩn joystick khi đang auto move
            if (moveJoystick != null && moveJoystick.gameObject.activeSelf)
            {
                moveJoystick.gameObject.SetActive(false);
            }
        }
        else
        {
            // Khi không phải chế độ auto move, enable CharacterController và disable NavMeshAgent
            if (cc != null && !cc.enabled)
            {
                cc.enabled = true;
            }
            
            if (agent != null && agent.enabled)
            {
                agent.enabled = false;
            }
            
            // Reset flag khi không còn auto move
            hasShownTooltips = false;
            lastTargetWithTooltips = null;
            currentTarget = null;
        }
    }

    // =========================
    // INPUT (Pointer / UI)
    // =========================
    void HandlePointerState()
    {
        // DOWN
        if (Input.GetMouseButtonDown(0))
        {
            pointerDown = true;
            rotating = false;

            pointerStartedOnUI = IsPointerOverUI();
            if (pointerStartedOnUI)
                return;

            pointerDownPos = Input.mousePosition;
            pointerDownTime = Time.time;
        }

        // DRAG -> start rotating (only if not started on UI and not moving)
        if (pointerDown && !pointerStartedOnUI && !rotating && !isMoving)
        {
            float dist = ((Vector2)Input.mousePosition - pointerDownPos).magnitude;
            if (dist >= dragStartPixels)
            {
                rotating = true;
                ApplyCursorState(true);
            }
        }

        // UP
        if (Input.GetMouseButtonUp(0))
        {
            if (pointerStartedOnUI)
            {
                pointerDown = false;
                rotating = false;
                pointerStartedOnUI = false;
                return;
            }

            // click world (only if not rotating)
            if (!rotating && enableWorldClick)
            {
                float held = Time.time - pointerDownTime;
                float dist = ((Vector2)Input.mousePosition - pointerDownPos).magnitude;

                if (held <= clickMaxDuration && dist <= dragStartPixels * 1.2f)
                    HandleWorldClick();
            }

            pointerDown = false;
            rotating = false;
            pointerStartedOnUI = false;

            ApplyCursorState(false);
        }
    }

    void ApplyCursorState(bool isRotating)
    {
        if (!lockCursorWhileRotating) return;

#if UNITY_WEBGL && !UNITY_EDITOR
        Cursor.lockState = isRotating ? CursorLockMode.Confined : CursorLockMode.None;
        Cursor.visible = true;
#else
        Cursor.lockState = isRotating ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isRotating;
#endif
    }

    // =========================
    // LOOK (Drag Mouse/Touch)
    // =========================
    void HandleLook()
    {
        if (!rotating || isMoving) return;

        float sens = mouseSensitivity;

#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL trên mobile thường gắt hơn
        sens *= webglMobileSensitivityMul;
#endif

        float mx = Input.GetAxis("Mouse X") * sens * Time.deltaTime;
        float my = Input.GetAxis("Mouse Y") * sens * Time.deltaTime;

        transform.Rotate(Vector3.up * mx);

        pitch -= my;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        if (cameraRoot) cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    // =========================
    // MOVE
    // =========================
    void HandleMove()
    {
        bool grounded = cc.isGrounded;
        if (grounded && velocity.y < 0f) velocity.y = -2f;

        Vector2 move = GetMoveInput();

        // Kiểm tra xem có đang di chuyển hay không
        isMoving = move.magnitude > 0.01f;
        
        // Nếu đang di chuyển thì dừng rotate
        if (isMoving && rotating)
        {
            rotating = false;
            ApplyCursorState(false);
        }

        Vector3 input = (transform.right * move.x + transform.forward * move.y);
        input = Vector3.ClampMagnitude(input, 1f);

        bool sprint = (!useJoystickOnly && Input.GetKey(KeyCode.LeftShift));
        float speed = moveSpeed * (sprint ? sprintMultiplier : 1f);

        cc.Move(input * speed * Time.deltaTime);

        if (grounded && !useJoystickOnly && Input.GetButtonDown("Jump"))
            Jump();

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }

    Vector2 GetMoveInput()
    {
        Vector2 v = Vector2.zero;

        if (moveJoystick != null)
        {
            v = moveJoystick.Direction;
            if (v.magnitude < joystickDeadzone) v = Vector2.zero;
        }

        if (!useJoystickOnly)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");
            Vector2 kb = new Vector2(x, z);
            if (kb.sqrMagnitude > v.sqrMagnitude) v = kb;
        }

        return Vector2.ClampMagnitude(v, 1f);
    }

    public void PressJump() // gán vào UI Button OnClick
    {
        Jump();
    }

    void Jump()
    {
        if (!cc.isGrounded) return;
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    // =========================
    // WORLD CLICK / HOVER
    // =========================
    void HandleWorldClick()
    {
        if (!cam) return;
        if (IsPointerOverUI()) return;

        // Raycast theo vị trí click ban đầu (ổn định hơn)
        Ray ray = cam.ScreenPointToRay(pointerDownPos);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, clickableMask, QueryTriggerInteraction.Ignore))
        {
            var clickable = hit.collider.GetComponentInParent<IClickable>();
            if (clickable != null)
            {
                if (UIController.instance != null && UIController.instance.tooltipUI != null)
                    UIController.instance.tooltipUI.Hide();

                currentHovered = null;
                clickable.OnClicked();
            }
        }
    }

    void HandleHover()
    {
        if (!enableHover) return;
        if (!cam) return;

        // Mobile thường không hover
        if (useJoystickOnly) return;

        if (IsPointerOverUI() || rotating)
        {
            ClearHover();
            return;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, clickableMask, QueryTriggerInteraction.Ignore))
        {
            var clickable = hit.collider.GetComponentInParent<IClickable>();
            if (clickable != null)
            {
                if (clickable != currentHovered)
                {
                    currentHovered = clickable;

                    string tooltipText = "";
                    var clickAble = hit.collider.GetComponentInParent<ClickAble>();
                    tooltipText = (clickAble != null) ? clickAble.title : hit.collider.gameObject.name;

                    if (!string.IsNullOrEmpty(tooltipText) &&
                        UIController.instance != null && UIController.instance.tooltipUI != null)
                    {
                        UIController.instance.tooltipUI.Show(tooltipText, clickable);
                    }
                }
                return;
            }
        }

        ClearHover();
    }

    void ClearHover()
    {
        if (currentHovered != null)
        {
            currentHovered = null;
            if (UIController.instance != null && UIController.instance.tooltipUI != null)
                UIController.instance.tooltipUI.Hide();
        }
    }

    // =========================
    // UI DETECT (no GC)
    // =========================
    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        // mouse quick check
        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        // touch check (mobile) - fingerId
        if (Input.touchCount > 0)
        {
            int id = Input.GetTouch(0).fingerId;
            if (EventSystem.current.IsPointerOverGameObject(id))
                return true;
        }

        if (ped == null) ped = new PointerEventData(EventSystem.current);
        ped.position = Input.mousePosition;

        uiHits.Clear();
        EventSystem.current.RaycastAll(ped, uiHits);
        return uiHits.Count > 0;
    }

    
}
