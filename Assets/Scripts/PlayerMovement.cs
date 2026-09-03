using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public sealed class PlayerMovement : MonoBehaviour
{
    private static readonly int MoveX = Animator.StringToHash("moveX");
    private static readonly int MoveY = Animator.StringToHash("moveY");
    private static readonly int IsMoving = Animator.StringToHash("isMoving");

    [SerializeField, Min(0f)] private float moveSpeed = 3f;

    private Rigidbody2D body;
    private Animator animator;
    private Vector2 movementInput;
    private Vector2 facingDirection = Vector2.down;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        body.gravityScale = 0f;
        body.freezeRotation = true;
    }

    private void Update()
    {
        movementInput = ReadMovementInput();

        if (movementInput.sqrMagnitude > 1f)
        {
            movementInput.Normalize();
        }

        bool isMoving = movementInput.sqrMagnitude > 0f;
        if (isMoving)
        {
            facingDirection = GetCardinalDirection(movementInput);
        }

        // Keep only one animation axis active. This prevents competing Any State
        // transitions (and direction flicker) while diagonal movement is held.
        animator.SetFloat(MoveX, facingDirection.x);
        animator.SetFloat(MoveY, facingDirection.y);
        animator.SetBool(IsMoving, isMoving);
    }

    private void FixedUpdate()
    {
        // Velocity is measured in units per second; Unity's fixed physics step
        // supplies the time correction and preserves normal Rigidbody collisions.
        body.linearVelocity = movementInput * moveSpeed;
    }

    private void OnDisable()
    {
        movementInput = Vector2.zero;

        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
        }

        if (animator != null)
        {
            animator.SetBool(IsMoving, false);
        }
    }

    private static Vector2 ReadMovementInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return Vector2.zero;
        }

        float horizontal = ReadAxis(keyboard.aKey, keyboard.leftArrowKey,
            keyboard.dKey, keyboard.rightArrowKey);
        float vertical = ReadAxis(keyboard.sKey, keyboard.downArrowKey,
            keyboard.wKey, keyboard.upArrowKey);

        return new Vector2(horizontal, vertical);
    }

    private static float ReadAxis(KeyControl negativeKey, KeyControl negativeAlternative,
        KeyControl positiveKey, KeyControl positiveAlternative)
    {
        bool negative = negativeKey.isPressed || negativeAlternative.isPressed;
        bool positive = positiveKey.isPressed || positiveAlternative.isPressed;
        return (positive ? 1f : 0f) - (negative ? 1f : 0f);
    }

    private static Vector2 GetCardinalDirection(Vector2 input)
    {
        // Vertical wins exact diagonal ties, making the result deterministic.
        if (Mathf.Abs(input.y) >= Mathf.Abs(input.x))
        {
            return input.y >= 0f ? Vector2.up : Vector2.down;
        }

        return input.x >= 0f ? Vector2.right : Vector2.left;
    }
}
