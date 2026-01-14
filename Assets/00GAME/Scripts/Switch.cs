using UnityEngine;
using System.Collections;

public class Switch : MonoBehaviour
{
    [Header("Settings")]
    public Collider2D door;
    public SpriteRenderer switchRenderer;

    [Header("Movement Settings")]
    public float pressDepth = 1.0f;
    public float deactivationDelay = 0.3f; // Thời gian chờ trước khi nhả nút
    public float moveSpeed = 2f; // Tốc độ di chuyển của nút (càng lớn càng nhanh)

    // Biến nội bộ
    private SpriteRenderer doorRenderer;
    private Vector3 initialPosition; // Vị trí cao nhất (gốc)
    private Vector3 targetPosition;  // Vị trí mà nút CẦN đi tới
    private Coroutine switchCoroutine;

    void Start()
    {
        initialPosition = transform.localPosition;
        
        // Ban đầu mục tiêu chính là vị trí gốc
        targetPosition = initialPosition;

        if (door != null)
        {
            doorRenderer = door.GetComponent<SpriteRenderer>();
        }
    }

    // Dùng Update để xử lý chuyển động mượt
    void Update()
    {
        // Di chuyển vị trí hiện tại tới vị trí mục tiêu (targetPosition)
        // MoveTowards đảm bảo nó dừng lại chính xác tại đích, không bị trượt quá
        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition, 
            targetPosition, 
            moveSpeed * Time.deltaTime
        );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    // Nếu người chơi quay lại, huỷ việc đếm ngược tắt nút
                    if (switchCoroutine != null)
                    {
                        StopCoroutine(switchCoroutine);
                        switchCoroutine = null;
                    }

                    ActivateSwitch();
                    break;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            switchCoroutine = StartCoroutine(DeactivateRoutine());
        }
    }

    IEnumerator DeactivateRoutine()
    {
        yield return new WaitForSeconds(deactivationDelay);
        DeactivateSwitch();
        switchCoroutine = null;
    }

    void ActivateSwitch()
    {
        // === THAY ĐỔI: Chỉ gán mục tiêu, không gán vị trí trực tiếp ===
        targetPosition = new Vector3(initialPosition.x, initialPosition.y - pressDepth, initialPosition.z);

        // Logic game (Mở cửa, đổi màu) vẫn làm ngay lập tức để phản hồi nhanh
        if (door != null) door.isTrigger = true;
        if (switchRenderer != null) switchRenderer.color = Color.green;
        if (doorRenderer != null) doorRenderer.color = Color.green;
    }

    void DeactivateSwitch()
    {
        // === THAY ĐỔI: Gán mục tiêu về lại vị trí gốc ===
        targetPosition = initialPosition;

        // Logic game (Đóng cửa, đổi màu)
        if (door != null) door.isTrigger = false;
        if (switchRenderer != null) switchRenderer.color = Color.white;
        if (doorRenderer != null) doorRenderer.color = Color.white;
    }
}