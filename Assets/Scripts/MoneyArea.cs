using UnityEngine;

/// <summary>
/// Script para a area em que o jogador entrega os personagens empilhados para receber dinheiro
/// </summary>
public class MoneyArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        StackController stack = other.GetComponent<StackController>();

        // Verifica se o jogador está carregando alguem ou se o script está certo
        if (stack == null || stack.stacked == null || stack.stacked.Count == 0) return;

        int npcsCollected = stack.stacked.Count;

        // Para cada personagem que está carregando, entrega dinheiro para o jogador
        for (int i = 0; i < npcsCollected; i++)
        {
            GameManager.instance.UpdateMoney();
        }

        // Limpa a lista de personagens empilhados
        stack.ClearStack();
    }
}
