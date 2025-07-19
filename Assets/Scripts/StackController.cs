using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Script dedicado a lógica de empilhamento do jogador
/// </summary>
public class StackController : MonoBehaviour
{
    public int maxStack = 2;
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
    public SpawnerManager spawner;

    public TextMeshProUGUI stackText;

    private void Awake()
    {
        if (player == null)
        {
            player = GetComponent<PlayerController>();
        }

        stackAnchor = transform.GetChild(transform.childCount - 1); // Pega referencia do ponto exato onde vai colocar os NPCs em cima do jogador
        stackText.text = "0/" + maxStack.ToString();                // Muda quantos personagens estão empilhados
    }

    // Metodo para colocar o NPC em cima do jogador
    public void StackNPC(Transform target)
    {
        if (stacked.Count >= maxStack || target == null) return;

        var rb = target.GetComponent<Rigidbody>();
        var col = target.GetComponent<Collider>();

        rb.isKinematic = false;

        // Desativa colisão com o jogador mas mantém a fisica com o mundo ativa
        Physics.IgnoreCollision(col, GetComponent<Collider>(), true);

        // Coloca o NPC como filho do ponto onde vai empilhar eles
        target.SetParent(stackAnchor, true);

        // Armazena o NPC nas listas
        stacked.Add(target);
        velocities.Add(Vector3.zero);
        hasSnapped.Add(false);
        stackText.text = $"{stacked.Count}/{maxStack}";
    }

    private void FixedUpdate()
    {
        // Lógica para a inercia da pilha de NPCs
        for (int i = 0; i < stacked.Count; i++)
        {
            Transform t = stacked[i];
            Rigidbody rb = t.GetComponent<Rigidbody>();

            // Onde o personagem deveria estar
            Vector3 targetPos = stackAnchor.position + Vector3.up * (i * stackSpacing);

            // No primeiro frame, coloca o personagem na posição exata em cima do jogador
            if (!hasSnapped[i])
            {
                rb.position = targetPos;
                t.position = targetPos;
                velocities[i] = Vector3.zero;
                hasSnapped[i] = true;
                continue;
            }

            // Utilizo o SmoothDamp pra dar uma movimentação suave em direção aonde deveria estar
            float smoothTime = baseSmoothTime + i * perIndexDelay;
            Vector3 vel = velocities[i];

            Vector3 newPos = Vector3.SmoothDamp(rb.position, targetPos, ref vel, smoothTime);

            // Diz pra engine de fisica mover até a posição que tem que estar
            rb.MovePosition(newPos);

            velocities[i] = vel;
        }
    }

    // Limpa a pilha
    public void ClearStack()
    {
        // Itera por cada membro da pilha e retira ele do jogador e devolve ao SpawnManager e para a pool
        for (int i = 0; i < stacked.Count; i++)
        {
            Transform t = stacked[i];
            NPCBehaviour npc = t.GetComponent<NPCBehaviour>();

            t.SetParent(spawner.transform, false);

            npc.onPlayer = false;

            spawner.ForceReturnToPool(npc);

            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
        }

        // Limpa as listas
        stacked.Clear();
        velocities.Clear();
        hasSnapped.Clear();

        stackText.text = stacked.Count.ToString() + "/" + maxStack.ToString();
    }

    // Para aumentar o numero máximo de personagens que o jogador pode carregar
    public void UpgradeMaxStack()
    {
        maxStack += 1;
        stackText.text = stacked.Count.ToString() + "/" + maxStack.ToString();
    }
}
