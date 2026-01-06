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
    public Transform cameraRoot;
    public float mouseSensitivity = 120f;
    [Range(0.1f, 1f)] public float webglMobileSensitivityMul = 0.65f;
    public float minPitch = -85f;
    public float maxPitch = 85f;

    [Header("Joystick Move (Optional)")]
    public Joystick moveJoystick;
    public float joystickDeadzone = 0.15f;
    public bool useJoystickOnly = true;

    [Header("Click / Hover (World)")]
    public bool enableWorldClick = true;
    public bool enableHover = true;
    public float clickMaxDuration = 0.25f;
    public float dragStartPixels = 6f;
    public float rayDistance = 100f;
    public LayerMask clickableMask = ~0;

    [Header("Cursor (PC/WebGL)")]
    public bool lockCursorWhileRotating = true;


    CharacterController cc;
    Camera cam;

    Vector3 velocity;
    float pitch;


    bool pointerDown;
    bool pointerStartedOnUI;
    bool rotating;
    bool isMoving;
    Vector2 pointerDownPos;
    float pointerDownTime;
    public NavMeshAgent agent;
    public bool isPlant;
    public Transform targetPlant;
    public List<GameObject> listPlant = new List<GameObject>();
    public bool isNhanSo;
    public Transform targetNhanSo;
    public List<GameObject> listNhanSo = new List<GameObject>();
    public bool isAnimal;
    public Transform targetAnimal;
    public List<GameObject> listAnimal = new List<GameObject>();

    [Header("Auto Move Rotation")]
    public float rotationSpeed = 5f;

    [Header("Camera Settings When Reached Target")]
    public Vector3 targetCameraPosition = new Vector3(0, 0.399f, 0);
    public Vector3 targetCameraRotation = new Vector3(26.217f, 0, 0);
    public float cameraTransitionSpeed = 2f;

    private bool hasShownTooltips = false;
    private bool hasSetCamera = false;
    private bool isTransitioningCamera = false;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Transform lastTargetWithTooltips = null;
    [SerializeField] private Transform currentTarget = null;


    IClickable currentHovered;


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


        if (agent != null)
        {
            agent.updatePosition = true;
            agent.updateRotation = false;
        }
    }

    void Update()
    {
        HandlePointerState();
        HandleLook();


        // Di chuyển bằng NavMeshAgent khi không ở chế độ auto move
        if (!isPlant && !isNhanSo && !isAnimal)
        {
            HandleMove();
        }

        HandleHover();
        AutoMove();
    }

    private void AutoMove()
    {
        Transform target = null;
        bool shouldAutoMove = false;


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

            if (cc != null && cc.enabled)
            {
                cc.enabled = false;
            }


            if (!agent.enabled)
            {
                agent.enabled = true;
            }


            if (!agent.isOnNavMesh)
            {
                Debug.LogWarning("NavMeshAgent không ở trên NavMesh! Vui lòng đảm bảo có NavMesh trong scene và nhân vật đang ở trên NavMesh.");
                return;
            }


            if (currentTarget != target)
            {
                currentTarget = target;
                hasShownTooltips = false;
                lastTargetWithTooltips = target;

                agent.isStopped = false;
            }

            // Kiểm tra xem đã đến target chưa
            // Chỉ coi là đã đến target nếu đã có path và remainingDistance hợp lệ
            bool hasReachedTarget = !agent.pathPending && 
                                    agent.remainingDistance != Mathf.Infinity && 
                                    agent.remainingDistance <= agent.stoppingDistance &&
                                    hasShownTooltips; // Chỉ dừng nếu đã hiển thị tooltip (đảm bảo đã thực sự đến target)

            if (hasReachedTarget)
            {
                // Đã đến target - dừng di chuyển và rotation
                if (!agent.isStopped)
                {
                    agent.isStopped = true;
                    UIController.instance.bottomPanel.SetActive(true);
                    
                    // Set camera khi đến target
                    if (!hasSetCamera && cameraRoot != null)
                    {
                        StartCameraTransition();
                        hasSetCamera = true;
                    }
                }
                
                // Tiếp tục transition camera nếu đang trong quá trình
                if (isTransitioningCamera && cameraRoot != null)
                {
                    UpdateCameraTransition();
                }
                
                // Không gọi SetDestination và LookRotation nữa khi đã đến target
            }
            else
            {
                // Chưa đến target - tiếp tục di chuyển và rotation
                if (agent.isStopped)
                {
                    agent.isStopped = false;
                }

                agent.SetDestination(target.position);

                // Kiểm tra xem đã đến target chưa (sau khi set destination)
                if (!agent.pathPending && 
                    agent.remainingDistance != Mathf.Infinity && 
                    agent.remainingDistance <= agent.stoppingDistance)
                {
                    // Đã đến target - hiển thị tooltip navigation
                    if (!hasShownTooltips && UIController.instance != null)
                    {
                        UIController.instance.StartTooltipNavigation(target);
                        hasShownTooltips = true;
                    }
                }
                else
                {
                    // Chưa đến - tiếp tục rotation
                    Vector3 direction = (target.position - transform.position);
                    direction.y = 0;
                    if (direction.magnitude > 0.1f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                    }
                }
            }


            if (moveJoystick != null && moveJoystick.gameObject.activeSelf)
            {
                moveJoystick.gameObject.SetActive(false);
            }
        }
        else
        {
            // Khi không có auto move, đảm bảo agent vẫn enable để HandleMove() có thể dùng
            // CharacterController vẫn disable vì chúng ta dùng agent cho joystick
            if (cc != null && cc.enabled)
            {
                cc.enabled = false;
            }

            // Đảm bảo agent được enable để HandleMove() có thể sử dụng
            if (agent != null && !agent.enabled)
            {
                agent.enabled = true;
            }

            // Dừng agent nếu đang di chuyển đến target cũ
            if (agent != null && agent.enabled && !agent.isStopped)
            {
                agent.isStopped = true;
            }

            hasShownTooltips = false;
            lastTargetWithTooltips = null;
            currentTarget = null;
            hasSetCamera = false;

            // Dừng tooltip navigation khi rời khỏi target
            if (UIController.instance != null)
            {
                UIController.instance.StopTooltipNavigation();
            }
        }
    }

    // Bắt đầu transition camera đến vị trí mục tiêu
    void StartCameraTransition()
    {
        if (cameraRoot == null) return;

        // Lưu vị trí và rotation ban đầu
        originalCameraPosition = cameraRoot.localPosition;
        originalCameraRotation = cameraRoot.localRotation;
        isTransitioningCamera = true;
    }

    // Cập nhật camera transition mượt mà
    void UpdateCameraTransition()
    {
        if (cameraRoot == null || !isTransitioningCamera) return;

        // Smooth transition position
        cameraRoot.localPosition = Vector3.Lerp(
            cameraRoot.localPosition,
            targetCameraPosition,
            cameraTransitionSpeed * Time.deltaTime
        );

        // Smooth transition rotation
        Quaternion targetRotation = Quaternion.Euler(targetCameraRotation);
        cameraRoot.localRotation = Quaternion.Slerp(
            cameraRoot.localRotation,
            targetRotation,
            cameraTransitionSpeed * Time.deltaTime
        );

        // Cập nhật pitch để giữ sync với rotation
        pitch = cameraRoot.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;

        // Kiểm tra xem đã đến gần target chưa
        float positionDistance = Vector3.Distance(cameraRoot.localPosition, targetCameraPosition);
        float rotationDistance = Quaternion.Angle(cameraRoot.localRotation, targetRotation);

        if (positionDistance < 0.01f && rotationDistance < 0.5f)
        {
            // Đã đến target, set chính xác
            cameraRoot.localPosition = targetCameraPosition;
            cameraRoot.localRotation = targetRotation;
            pitch = targetCameraRotation.x;
            isTransitioningCamera = false;
        }
    }




    void HandlePointerState()
    {

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


        if (pointerDown && !pointerStartedOnUI && !rotating && !isMoving)
        {
            float dist = ((Vector2)Input.mousePosition - pointerDownPos).magnitude;
            if (dist >= dragStartPixels)
            {
                rotating = true;
                ApplyCursorState(true);
            }
        }


        if (Input.GetMouseButtonUp(0))
        {
            if (pointerStartedOnUI)
            {
                pointerDown = false;
                rotating = false;
                pointerStartedOnUI = false;
                return;
            }


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




    void HandleLook()
    {
        if (!rotating || isMoving) return;

        float sens = mouseSensitivity;

#if UNITY_WEBGL && !UNITY_EDITOR

        sens *= webglMobileSensitivityMul;
#endif

        float mx = Input.GetAxis("Mouse X") * sens * Time.deltaTime;
        float my = Input.GetAxis("Mouse Y") * sens * Time.deltaTime;

        transform.Rotate(Vector3.up * mx);

        pitch -= my;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        if (cameraRoot) cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }




    void HandleMove()
    {
        // Đảm bảo agent được enable và trên NavMesh
        if (agent == null) return;
        
        // Không di chuyển bằng joystick nếu đang auto move đến target
        if (isPlant || isNhanSo || isAnimal)
        {
            return;
        }
        
        if (!agent.enabled)
        {
            agent.enabled = true;
        }
        
        if (!agent.isOnNavMesh)
        {
            return;
        }

        // Đảm bảo CharacterController được disable khi dùng agent
        if (cc != null && cc.enabled)
        {
            cc.enabled = false;
        }

        // Dừng agent nếu đang có path đến đâu đó
        if (!agent.isStopped)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        Vector2 move = GetMoveInput();

        isMoving = move.magnitude > 0.01f;

        if (isMoving && rotating)
        {
            rotating = false;
            ApplyCursorState(false);
        }

        Vector3 input = (transform.right * move.x + transform.forward * move.y);
        input = Vector3.ClampMagnitude(input, 1f);

        bool sprint = (!useJoystickOnly && Input.GetKey(KeyCode.LeftShift));
        float speed = moveSpeed * (sprint ? sprintMultiplier : 1f);

        // Sử dụng agent.Move() thay vì cc.Move()
        agent.Move(input * speed * Time.deltaTime);
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

    public void PressJump()
    {
        Jump();
    }

    void Jump()
    {
        if (!cc.isGrounded) return;
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }




    void HandleWorldClick()
    {
        if (!cam) return;
        if (IsPointerOverUI()) return;

        // Ưu tiên: Nếu đang ở tooltip navigation mode, click vào object hiện tại đang được hiển thị
        if (UIController.instance != null && UIController.instance.IsNavigatingTooltips())
        {
            ClickAble currentClickable = UIController.instance.GetCurrentTooltipClickable();
            if (currentClickable != null && currentClickable.isValid)
            {
                if (UIController.instance.tooltipUI != null)
                    UIController.instance.tooltipUI.Hide();

                currentHovered = null;
                currentClickable.OnClicked();
                return;
            }
        }

        // Nếu không ở navigation mode, sử dụng raycast như bình thường
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
        if (!enableHover) 
        {
            // Debug: enableHover is false
            return;
        }
        if (!cam) 
        {
            // Debug: cam is null
            return;
        }

        // Chỉ clear hover khi đang rotating
        if (rotating)
        {
            ClearHover();
            return;
        }

        // Thử raycast trước, nếu hit được object thì không cần kiểm tra UI
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, rayDistance, clickableMask, QueryTriggerInteraction.Ignore);
        
        if (hitSomething)
        {
            var clickable = hit.collider.GetComponentInParent<IClickable>();
            if (clickable != null)
            {
                // Kiểm tra UI chỉ khi cần thiết (nếu đang hover vào UI element thực sự)
                // Nhưng vẫn cho phép hover vào world object
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
                        Debug.Log("Hovering over: " + tooltipText);
                    }
                }
                return;
            }
        }

        // Chỉ clear hover khi không hit được object VÀ không đang ở trên UI
        // Nếu đang ở trên UI nhưng không hit được object, giữ nguyên hover hiện tại
        if (!IsPointerOverUI())
        {
            ClearHover();
        }
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




    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;


        if (EventSystem.current.IsPointerOverGameObject())
            return true;


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
