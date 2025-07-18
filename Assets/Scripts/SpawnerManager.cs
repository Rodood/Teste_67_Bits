using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    public GameObject npcPrefab;
    public Transform[] spawnPoints;

    public int maxPoolSize = 10;

    public float spawnInterval = 1f;

    Queue<NPCBehaviour> pool = new Queue<NPCBehaviour>();
    List<NPCBehaviour> activeNPCs = new List<NPCBehaviour>();
    public float timer = 0f;

    private void Start()
    {
        for (int i = 0; i < maxPoolSize; i++)
        {
            var go = Instantiate(npcPrefab, Vector3.zero, Quaternion.identity, transform);
            go.SetActive(false);
            pool.Enqueue(go.GetComponent<NPCBehaviour>());
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if(timer >= spawnInterval && activeNPCs.Count < maxPoolSize)
        {
            SpawnFromPool();
            timer = 0f;
        }
    }

    private void SpawnFromPool()
    {
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

        NPCBehaviour npc = pool.Dequeue();

        npc.gameObject.SetActive(true);
        npc.transform.position = point.position;
        npc.transform.rotation = point.rotation;

        npc.Initialize(this, point);
        activeNPCs.Add(npc);
    }

    public void NotifyNPCRemoved(NPCBehaviour npc)
    {
        if (!activeNPCs.Remove(npc)) return;

        npc.Cleanup();
        npc.gameObject.SetActive(false);
        pool.Enqueue(npc);
    }
}
