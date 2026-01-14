using System;
using UnityEngine;
using Spine.Unity;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    [Header("Check Ground")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundRadius = 0.15f;

    [Header("Spine")]
    public SkeletonAnimation skeleton;

    public GameObject winPanel;
    
    Rigidbody2D rb;

    bool isGrounded;
    bool isFacingRight = true;

    bool isInteracting;
    PushPullObject currentObject;

    float horizontal;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        winPanel.gameObject.SetActive(false);
    }

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        CheckGround();
        HandleInteraction();
        HandleAnimation();
        HandleFlip();
        HandleMovement();
    }

    void FixedUpdate()
    {
    }

    // ================= MOVEMENT =================
    void HandleMovement()
    {
        if (isInteracting)
        {
            rb.linearVelocity = new Vector2(horizontal * moveSpeed * 0.5f, rb.linearVelocity.y);

            if (currentObject)
            {
                currentObject.rb.linearVelocity =
                    new Vector2(rb.linearVelocity.x, currentObject.rb.linearVelocity.y);
            }
            return;
        }

        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

        if ((Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.UpArrow))&& isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    // ================= INTERACTION =================
    void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isInteracting)
            {
                StopInteraction();
            }
            else
            {
                TryInteract();
            }
        }
    }

    void TryInteract()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position,
            isFacingRight ? Vector2.right : Vector2.left, 1f);

        if (hit && hit.collider.CompareTag("Pushable"))
        {
            if (!hit.collider.TryGetComponent<PushPullObject>(out currentObject)) return;

            isInteracting = true;
            rb.gravityScale = 0;
        }
    }

    void StopInteraction()
    {
        isInteracting = false;
        rb.gravityScale = 1;
        currentObject = null;
    }

    // ================= GROUND =================
    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position, groundRadius, groundLayer);
    }

    // ================= ANIMATION =================
    void HandleAnimation()
    {
        if (isInteracting)
        {
            if (horizontal != 0)
            {
                if ((horizontal > 0 && isFacingRight) ||
                    (horizontal < 0 && !isFacingRight))
                {
                    PlayAnim("push");
                }
                else
                {
                    PlayAnim("pull");
                }
            }
            else
            {
                PlayAnim("idle");
            }
            return;
        }

        if (!isGrounded)
        {
            if (rb.linearVelocity.y > 0)
                PlayAnim("jump");
            else
                PlayAnim("idle_jump");
            return;
        }

        if (horizontal != 0)
            PlayAnim("run");
        else
            PlayAnim("idle");
    }

    void PlayAnim(string anim)
    {
        if (skeleton == null || skeleton.AnimationName == anim) return;
        skeleton.AnimationState.SetAnimation(0, anim, true);
    }

    // ================= FLIP =================
    void HandleFlip()
    {
        if (horizontal > 0 && !isFacingRight)
            Flip();
        else if (horizontal < 0 && isFacingRight)
            Flip();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("End"))
        {
            winPanel.gameObject.SetActive(true);
            Time.timeScale = 0;
        }
    }
    
    
}
