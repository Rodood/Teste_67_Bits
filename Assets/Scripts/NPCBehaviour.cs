using System.Collections;
using UnityEngine;

/// <summary>
/// Script para o comportamento dos NPCs
/// </summary>
public class NPCBehaviour : MonoBehaviour
{
    public Animator anim;
    public Rigidbody rb;
    public float patrolDistance = 10f;
    public float speed = 2f;
    public float timeoutDuration = 5f;
    public float fadeDuration = 1f;

    public SkinnedMeshRenderer smr;
    private Transform[] bones;
    private Collider[] boneColliders;
    private Rigidbody[] boneRigidbodies;

    private SpawnerManager manager;
    private Vector3 startPos;
    private Vector3 targetPos;
    private bool headingOutward = true;
    public bool onPlayer = false;
    private float spawnTime;

    private Coroutine fadeCoroutine;
    private MaterialPropertyBlock mpb;
    private static readonly int ColorID = Shader.PropertyToID("_Color");

    // Armazena todos os valores necessários para o bom funcionamento
    private void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();

        smr = GetComponentInChildren<SkinnedMeshRenderer>();

        if (smr == null)
            Debug.LogError("Missing SkinnedMeshRenderer on NPC");

        // Armazena os ossos do rig do personagem para poder funcionar corretamente
        bones = smr.bones;
        boneColliders = new Collider[bones.Length];
        boneRigidbodies = new Rigidbody[bones.Length];
        for (int i = 0; i < bones.Length; i++)
        {
            boneColliders[i] = bones[i].GetComponent<Collider>();
            boneRigidbodies[i] = bones[i].GetComponent<Rigidbody>();
        }

        mpb = new MaterialPropertyBlock(); // Gerencia para poder criar varios NPCs com o mesmo material mas diferença na cor
    }

    // Inicializa o NPC quando é chamado pelo SpawnManager e coloca todos os fatores que o NPC precisa ter ao ser instanciado
    public void Initialize(SpawnerManager mgr, Transform spawnPoint)
    {
        manager = mgr;
        startPos = spawnPoint.position;
        transform.position = startPos;
        headingOutward = false;
        onPlayer = false;
        spawnTime = Time.time;

        Vector3 dir = (startPos.x > 0) ? Vector3.left : Vector3.right;
        targetPos = startPos + dir * patrolDistance;

        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        Color color = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f, 1f, 1f);
        mpb.SetColor(ColorID, color);
        smr.SetPropertyBlock(mpb);

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        SetMaterialAlpha(1f);

        anim.SetBool("IsWalking", true);
    }

    private void SetMaterialAlpha(float alpha)
    {
        smr.GetPropertyBlock(mpb);
        Color c = mpb.GetColor(ColorID);
        c.a = alpha;
        mpb.SetColor(ColorID, c);
        smr.SetPropertyBlock(mpb);
    }

    private void Update()
    {
        if (onPlayer) return;

        Patrol();
        if (!anim.enabled)
            CheckTimeout();
    }

    // Controla o movimento do NPC
    private void Patrol()
    {
        Vector3 dest = headingOutward ? startPos : targetPos;
        transform.position = Vector3.MoveTowards(transform.position, dest, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, dest) < 0.1f)
        {
            if (headingOutward)
            {
                manager.NotifyNPCRemoved(this);
            }
            else
            {
                headingOutward = true;
                transform.rotation = Quaternion.LookRotation(dest, Vector3.up);
            }
        }
    }

    // Caso o NPC tenha sido atingido pelo jogador e o jogador não tenha interagido com ele, despawna ele depois de um tempo
    private void CheckTimeout()
    {
        if (Time.time > spawnTime + timeoutDuration && fadeCoroutine == null)
        {
            fadeCoroutine = StartCoroutine(FadeAndDespawn());
        }
    }

    // Tenta fazer com que desapareca lentamente com o passar do tempo até chamar o spawn manager para devolver a pool
    private IEnumerator FadeAndDespawn()
    {
        float elapsed = 0f;
        smr.GetPropertyBlock(mpb);
        Color initial = mpb.GetColor(ColorID);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            Color fadeColor = initial;
            fadeColor.a = Mathf.Lerp(1f, 0f, t);
            mpb.SetColor(ColorID, fadeColor);
            smr.SetPropertyBlock(mpb);
            yield return null;
        }
        manager.NotifyNPCRemoved(this);
    }

    // Ativa o Ragdoll do personagem e tenta colocar ele acima do solo
    public void Knockout()
    {
        anim.enabled = false;
        gameObject.layer = 0;
        smr.transform.position += Vector3.up * 2f;
    }

    // Ativa os bones do personagem após um tempo passar
    public void EnableBones()
    {
        for (int i = 0; i < bones.Length; i++)
        {
            if (boneColliders[i]) boneColliders[i].enabled = true;
            if (boneRigidbodies[i]) boneRigidbodies[i].isKinematic = false;
        }
    }

    // Desativa os bones para que o personagem fique corretamente em cima do jogador
    public void DisableBones()
    {
        for (int i = 0; i < bones.Length; i++)
        {
            if (boneColliders[i]) boneColliders[i].enabled = false;
            if (boneRigidbodies[i]) boneRigidbodies[i].isKinematic = true;
        }
    }

    // Limpa o NPC para que possa voltar a funcionar corretamente antes de voltar a pool
    public void Cleanup()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        SetMaterialAlpha(1f);

        Collider rootCollider = GetComponent<Collider>();
        if (rootCollider) rootCollider.enabled = true;

        anim.enabled = true;
        gameObject.layer = 6;

        manager = null;
    }

    // Gerencia se o jogador entrou em contato com o NPC para adicionar em cima do jogador
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !anim.enabled)
        {
            DisableBones();
            smr.rootBone.localPosition = Vector3.zero;
            smr.rootBone.localRotation = Quaternion.identity;

            onPlayer = true;
            other.GetComponent<StackController>().StackNPC(transform);
        }
    }
}
