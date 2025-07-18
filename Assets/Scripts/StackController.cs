using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class StackController : MonoBehaviour
{
    public int maxStack = 3;
    public float stackSpacing = 1f;
    public float followSpeed = 5f;
    public float initialHeight = 3f;
    public float baseSmoothTime = 0.1f;
    public float perIndexDelay = 0.05f;

    public Transform stackAnchor;
    public List<Transform> stacked = new List<Transform>();
    List<Vector3> velocities = new List<Vector3>();
    private List<bool> hasSnapped = new List<bool>();

    public PlayerController player;

    public TextMeshProUGUI stackText;

    private void Awake()
    {
        if(player == null)
        {
            player = GetComponent<PlayerController>();
        }

        stackAnchor = transform.GetChild(transform.childCount - 1);
        stackText.text = "0/" + maxStack.ToString();
    }

    public void StackNPC(Transform target)
    {
        if (stacked.Count >= maxStack || target == null) return;

        Rigidbody rb = target.GetComponent<Rigidbody>();
        Collider collider = target.GetComponent<Collider>();

        if (rb) 
            rb.isKinematic = true;
        if(collider)
            collider.enabled = false;

        target.SetParent(transform, true);

        target.GetComponent<NPCBehaviour>().smr.rootBone.transform.localPosition = Vector3.zero;

        stacked.Add(target);
        velocities.Add(Vector3.zero);
        hasSnapped.Add(false);
        stackText.text = stacked.Count.ToString() + "/" + maxStack.ToString();
    }

    private void LateUpdate()
    {
        for (int i = 0; i < stacked.Count; i++)
        { 
            Transform t = stacked[i];
            Vector3 targetPos = stackAnchor.position + Vector3.up * (i * stackSpacing);

            if (!hasSnapped[i])
            {
                t.position = targetPos;
                velocities[i] = Vector3.zero;
                hasSnapped[i] = true;
            }
            else
            {
                float smoothTime = baseSmoothTime + (i * perIndexDelay);

                Vector3 currentVelocity = velocities[i];

                t.position = Vector3.SmoothDamp(t.position, targetPos, ref currentVelocity, smoothTime);
                t.GetComponent<NPCBehaviour>().smr.rootBone.position = Vector3.SmoothDamp(t.GetComponent<NPCBehaviour>().smr.rootBone.position, 
                    targetPos, ref currentVelocity, smoothTime);
            }
        }
    }

    public void ClearStack()
    {
        for (int i = 0; i < stacked.Count; i++)
        {
            Rigidbody rb = stacked[i].GetComponent<Rigidbody>();
            if (rb)
                rb.isKinematic = false;

            stacked[i].SetParent(null, true);

            Destroy(stacked[i].gameObject);
        }

        stacked.Clear();
        velocities.Clear();
        hasSnapped.Clear();

        stackText.text = stacked.Count.ToString() + "/" + maxStack.ToString();
    }

    public void UpgradeMaxStack()
    {
        maxStack += 2;
        stackText.text = stacked.Count.ToString() + "/" + maxStack.ToString();
    }
}
