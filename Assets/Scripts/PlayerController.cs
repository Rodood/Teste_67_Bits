using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public InputActionAsset inputActions;

    InputAction moveAction;

    Vector2 moveAmount;

    public Animator anim;
    public Rigidbody rb;

    public float walkSpeed = 5f;
    public float rotateSpeed = 5f;

    public float detectionRadius = 1.5f;
    public LayerMask enemyLayer;

    bool isAttacking;

    private void OnEnable()
    {
        inputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        inputActions.FindActionMap("Player").Disable();
    }

    private void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");

        if (anim == null)
            anim = GetComponent<Animator>();

        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        moveAmount = moveAction.ReadValue<Vector2>();
        EnemyDetection();
    }

    private void FixedUpdate()
    {
        Walking();
    }

    private void Walking()
    {
        if (isAttacking) return;

        // Compute movement direction in world space
        Vector3 moveDir = new Vector3(moveAmount.x, 0f, moveAmount.y).normalized;

        // Rotate and move the character using physics
        if (moveDir.sqrMagnitude > 0.1f)
        {
            // Smooth rotation towards move direction
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            rb.MoveRotation(Quaternion.Lerp(rb.rotation, targetRot, rotateSpeed * Time.deltaTime));

            rb.MovePosition(rb.position + moveDir * walkSpeed * Time.deltaTime);
        }
        else
        {
            moveDir = Vector3.zero;
        }

        // Convert world movement to local and update animator
        Vector3 localDir = transform.InverseTransformVector(moveDir);
        anim.SetFloat("Horizontal", localDir.x);
        anim.SetFloat("Vertical", localDir.z);
    }

    public void Attack(Transform target)
    {
        isAttacking = true;
        Vector3 toTarget = (target.position - transform.position).normalized;
        // keep player upright:
        toTarget.y = 0;
        transform.LookAt(transform.position + toTarget);

        rb.MoveRotation(transform.rotation);
        anim.SetTrigger("Attack");

        target.gameObject.GetComponent<NPCBehaviour>().Knockout();
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    void EnemyDetection()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        if (hits.Length == 0 || isAttacking)
            return;  // no enemies or already in an attack

        // 2) Immediately pick the first one and attack
        Transform enemy = hits[0].transform;
        Attack(enemy);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red * 0.3f;
        Gizmos.DrawSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
