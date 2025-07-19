using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Script que controla toda a movimentação e animação do personagem, incluindo a lógica
/// de ataque dele.
/// </summary>
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
    private float detectionInterval = 0.2f;
    private float detectionTimer = 0f;

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
        
        // Gerencia o tempo desde a vez que detectou se tinha NPCs proximos para otimizar a quantidade de chamadas
        detectionTimer += Time.deltaTime;
        if (detectionTimer >= detectionInterval)
        {
            detectionTimer = 0f;
            if (!isAttacking)
                NPCDetection();
        }
    }

    private void FixedUpdate()
    {
        if (isAttacking) return;
            Walking();
    }

    private void Walking()
    {
        // Calcula a direção do movimento
        Vector3 moveDir = new Vector3(moveAmount.x, 0f, moveAmount.y).normalized;

        // Rotaciona o personagem usando fisica
        if (moveDir.sqrMagnitude > 0.1f)
        {
            moveDir.Normalize();
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotateSpeed * Time.fixedDeltaTime));
            rb.MovePosition(rb.position + moveDir * walkSpeed * Time.fixedDeltaTime);
        }
        else
        {
            moveDir = Vector3.zero;
        }

        // Tive um problema com o colisor do personagem onde as vezes ele começava a rotacionar após encostar
        // em um NPC, criei esse if para poder impedir que ficasse rotacionando.
        if (moveDir == Vector3.zero && rb.angularVelocity.sqrMagnitude > 0.0001f)
            rb.angularVelocity = Vector3.zero;

        // Converte o movimento no mundo para movimento local e manda para a animação do personagem
        Vector3 localDir = transform.InverseTransformVector(moveDir);
        anim.SetFloat("Horizontal", localDir.x);
        anim.SetFloat("Vertical", localDir.z);
    }

    // Ataca o NPC
    public void Attack(Transform target)
    {
        isAttacking = true;
        Vector3 toTarget = (target.position - transform.position);
        toTarget.y = 0f;

        // Rotaciona para encarar o NPC na hora do ataque
        if (toTarget != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(toTarget);

        anim.SetTrigger("Attack");

        var npc = target.GetComponent<NPCBehaviour>();
        if (npc != null)
            npc.Knockout();
    }

    // Permite o personagem voltar a andar após um evento na animação
    public void EndAttack()
    {
        isAttacking = false;
    }

    // Detecta se há NPCs dentro do alcance do jogador
    void NPCDetection()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        if (hits.Length > 0)
            Attack(hits[0].transform);
    }
}
