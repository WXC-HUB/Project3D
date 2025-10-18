using UnityEngine;

/// <summary>
/// 简单的玩家移动控制 - Demo专用
/// WASD或方向键移动，鼠标控制视角
/// </summary>
public class SimplePlayerController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("摄像机设置")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float cameraDistance = 10f;
    [SerializeField] private float cameraHeight = 5f;
    
    private Rigidbody rb;
    private Vector3 moveDirection;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // 配置Rigidbody
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.drag = 5f;
        
        // 设置摄像机
        SetupCamera();
        
        Debug.Log("✓ 玩家控制器已激活 - 使用WASD移动，Shift加速");
    }
    
    void Update()
    {
        // 获取输入
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D 或 左/右箭头
        float vertical = Input.GetAxisRaw("Vertical");     // W/S 或 上/下箭头
        
        // 计算移动方向
        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
        
        // 旋转朝向移动方向
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // 更新摄像机位置
        UpdateCamera();
    }
    
    void FixedUpdate()
    {
        // 移动
        if (moveDirection.magnitude > 0.1f)
        {
            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
            Vector3 movement = moveDirection * currentSpeed;
            rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);
        }
    }
    
    void SetupCamera()
    {
        // 如果已有摄像机Transform引用，使用它
        if (cameraTransform != null) return;
        
        // 否则查找主摄像机
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            cameraTransform = mainCam.transform;
        }
    }
    
    void UpdateCamera()
    {
        if (cameraTransform == null) return;
        
        // 摄像机跟随玩家
        Vector3 targetPosition = transform.position - Vector3.forward * cameraDistance + Vector3.up * cameraHeight;
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, 5f * Time.deltaTime);
        cameraTransform.LookAt(transform.position + Vector3.up * 1f);
    }
    
    void OnDrawGizmos()
    {
        // 可视化移动方向
        if (Application.isPlaying && moveDirection.magnitude > 0.1f)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position + Vector3.up, moveDirection * 2f);
        }
    }
}

