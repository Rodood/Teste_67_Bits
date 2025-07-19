using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

/// <summary>
/// Script para gerenciar o spawn dos inimigos
/// </summary>
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
        // Cria todos os NPCs que serão usados logo no inicio do jogo para poder tentar otimizar eles, fazendo uma pool de objetos
        for (int i = 0; i < maxPoolSize; i++)
        {
            var go = Instantiate(npcPrefab, Vector3.zero, Quaternion.identity, transform);
            go.SetActive(false);
            pool.Enqueue(go.GetComponent<NPCBehaviour>());
        }
    }

    // Invoca o NPC na lista depois de um tempo
    private void Update()
    {
        timer += Time.deltaTime;

        if(timer >= spawnInterval && activeNPCs.Count < maxPoolSize)
        {
            SpawnFromPool();
            timer = 0f;
        }
    }

    // Chama um NPC da pool de objetos e coloca em um dos lugares aleatorios e inicializa ele
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

    // Força com que o NPC volte a pool caso necessário que force
    public void ForceReturnToPool(NPCBehaviour npc)
    {
        activeNPCs.Add(npc);
        NotifyNPCRemoved(npc);
    }

    // Coloca o NPC para ser removido, reseta ele e devolve a pool de objetos
    public void NotifyNPCRemoved(NPCBehaviour npc)
    {
        if (!activeNPCs.Remove(npc))
        {
            return;
        }

        if (npc.onPlayer) return;

        npc.Cleanup();
        npc.gameObject.SetActive(false);
        pool.Enqueue(npc);
    }
}
