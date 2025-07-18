using System.Collections;
using UnityEngine;

public class NPCBehaviour : MonoBehaviour
{
    public Animator anim;

    public float patrolDistance = 5f;
    public float speed = 2f;

    public float timeoutDuration = 5f;
    public float fadeDuration = 1f;

    public SkinnedMeshRenderer smr;
    Transform[] bones;
    Material matInstance;
    Coroutine fadeCoroutine;

    SpawnerManager manager;
    Vector3 startPos;
    Vector3 targetPos;
    bool headingOutward = true;
    float spawnTime;

    private void Awake()
    {
        if(anim == null)
        {
            anim = GetComponent<Animator>();
        }

        smr = GetComponentInChildren<SkinnedMeshRenderer>();

        if(smr == null)
        {
            Debug.LogError("Não foi encontrado o SkinnedMeshRenderer");
        }

        bones = smr.bones;
    }

    public void Initialize(SpawnerManager mgr, Transform spawnPoint)
    {
        manager = mgr;
        startPos = spawnPoint.position;
        transform.position = startPos;
        headingOutward = true;

        Vector3 dir = (startPos.x > 0) ? Vector3.left : Vector3.right;

        targetPos = startPos + spawnPoint.right * patrolDistance;

        if(matInstance == null)
        {
            matInstance = Instantiate(smr.material);
            smr.material = matInstance;
        }

        matInstance.color = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f, 1f, 1f);

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            SetMaterialAlpha(1f);
    }

    void SetMaterialAlpha(float a)
    {
        Color color = matInstance.color;
        color.a = a;
        matInstance.color = color;
    }

    private void Update()
    {
        Patrol();
        CheckTimeout();
    }

    private void Patrol()
    {
        Vector3 dest = headingOutward ? startPos : targetPos;
        transform.position = Vector3.MoveTowards(transform.position, dest, speed * Time.deltaTime);

        if(Vector3.Distance(transform.position, dest) < 0.1f)
        {
            if (headingOutward)
            {
                manager.NotifyNPCRemoved(this);
            }
            else
            {
                headingOutward = true;
            }
        }
    }

    private void CheckTimeout()
    {
        if(Time.time > spawnTime + timeoutDuration)
        {
            if (fadeCoroutine == null)
                fadeCoroutine = StartCoroutine(FadeAndDespawn());
        }
    }

    IEnumerator FadeAndDespawn()
    {
        float elapsed = 0f;
        Color initial = matInstance.color;
        while(elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            matInstance.color = Color.Lerp(initial, new Color(initial.r, initial.g, initial.b, 0), t);
            yield return null;
        }
        manager.NotifyNPCRemoved(this);
    }

    public void Knockout()
    {
        anim.enabled = false;
        this.gameObject.layer = 0;

        transform.position += Vector3.up * 1.5f;
    }

    public void EnableBones()
    {
        foreach (Transform bone in bones)
        {
            var col = bone.GetComponent<Collider>();
            var rb = bone.GetComponent<Rigidbody>();

            if (col != null)
                col.enabled = true;

            if (rb != null)
                rb.isKinematic = false;
        }
    }

    public void DisableBones()
    {
        foreach (Transform bone in bones)
        {
            var col = bone.GetComponent<Collider>();
            var rb = bone.GetComponent<Rigidbody>();

            if(col != null)
                col.enabled = false;

            if(rb != null)
                rb.isKinematic = true;
        }
    }

    public void Cleanup()
    {
        if(fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        SetMaterialAlpha(1f);

        anim.enabled = true;
        this.gameObject.layer = 7;

        manager = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && anim.enabled == false)
        {
            this.DisableBones();

            var modelTransform = this.smr.transform;
            modelTransform.localPosition = Vector3.zero;       // adjust to fit your socket
            modelTransform.localRotation = Quaternion.identity;

            other.gameObject.GetComponent<StackController>().StackNPC(modelTransform);
            this.smr.rootBone.localPosition = Vector3.zero;


            manager.NotifyNPCRemoved(this);
        }
    }
}
