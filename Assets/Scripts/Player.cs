using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Player : MonoBehaviour
{
    // Config
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] float climbSpeed = 5f;
    [SerializeField] Animation climbAnimation;

    //State
    bool isAlive = true;

    // References
    Rigidbody2D rigidBody;
    Animator animator;
    CapsuleCollider2D bodyCollider;
    BoxCollider2D feet;

    float initialGravityScale;
    float initialAnimatorSpeed;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        feet = GetComponent<BoxCollider2D>();

        initialGravityScale = rigidBody.gravityScale;
        initialAnimatorSpeed = animator.speed;
    }

    // Update is called once per frame
    void Update()
    {
        Run();
        Jump();
        FlipSprite();
        ClimbLadder();
        CheckEnemyCollision();
    }

    private void Run()
    {
        if (!isAlive) { return; }

        float controlThrow = CrossPlatformInputManager.GetAxis("Horizontal"); // -1 to 1
        Vector2 playerVelocity = new Vector2(controlThrow * runSpeed, rigidBody.velocity.y);
        rigidBody.velocity = playerVelocity;

        bool isRunning = Mathf.Abs(rigidBody.velocity.x) > Mathf.Epsilon;
        bool isClimbing = feet.IsTouchingLayers(LayerMask.GetMask("Climbing"));

        animator.SetBool("Running", isRunning && !isClimbing);
    }

    private void Jump()
    {
        if (!isAlive) { return; }

        bool isJumping = !feet.IsTouchingLayers(LayerMask.GetMask("Ground"));

        if (!isJumping && CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            Vector2 jumpVelocityToAdd = new Vector2(0f, jumpSpeed);
            rigidBody.velocity += jumpVelocityToAdd;
        }
    }

    private void ClimbLadder()
    {
        if (!isAlive) { return; }

        if (!feet.IsTouchingLayers(LayerMask.GetMask("Climbing"))) 
        {
            animator.SetBool("Climbing", false);
            rigidBody.gravityScale = initialGravityScale;
            animator.speed = initialAnimatorSpeed;
            return;
        }

        float controlThrow = CrossPlatformInputManager.GetAxis("Vertical");
        Vector2 climbVelocity = new Vector2(rigidBody.velocity.x, controlThrow * climbSpeed);
        rigidBody.velocity = climbVelocity;
        rigidBody.gravityScale = 0;

        //bool playerHasVerticalSpeed = Mathf.Abs(myRigidBody.velocity.y) > Mathf.Epsilon;
        animator.SetBool("Climbing", true);

        if (controlThrow == 0)
        {
            animator.speed = 0;
        } else
        {
            animator.speed = initialAnimatorSpeed;
        }

    }

    private void FlipSprite()
    {
        if (!isAlive) { return; }

        bool playerHasHorizontalSpeed = Mathf.Abs(rigidBody.velocity.x) > Mathf.Epsilon;

        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(rigidBody.velocity.x), 1f);
        }
    }

    private void CheckEnemyCollision()
    {
        if (!isAlive) { return; }

        if (rigidBody.IsTouchingLayers(LayerMask.GetMask("Enemy")))
        {
            Die();
        }
    }

    private void Die()
    {
        this.isAlive = false;
        this.animator.SetBool("Alive", false);
        this.bodyCollider.enabled = false;
        this.rigidBody.bodyType = RigidbodyType2D.Static;
    }

}
