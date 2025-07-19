using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Script que gerencia todas as variaveis como dinheiro e chamada de upgrades
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int currentMoney;
    public int sellAmount = 3;
    
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI sellAmountText;
    public GameObject shopUI;

    public StackController stack;

    float deltaTime;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        moneyText.text = currentMoney.ToString();
        sellAmountText.text = sellAmount.ToString();

        shopUI.SetActive(false);
    }

    // Adiciona um valor aleatorio para o dinheiro atual do jogador
    public void UpdateMoney()
    {
        int money = Random.Range(1, 4);
        currentMoney += money;

        moneyText.text = currentMoney.ToString();
    }

    public void OpenShop()
    {
        shopUI.SetActive(true);
    }

    public void CloseShop()
    {
        shopUI.SetActive(false);
    }

    // Compra a melhoria da pilha, atualiza o valor de venda da melhoria e muda para uma cor aleatoria o personagem do jogador
    public void BuyUpgrade()
    {
        if(currentMoney < sellAmount) return;

        currentMoney -= sellAmount;
        moneyText.text = currentMoney.ToString();

        sellAmount += Random.Range(2, 7);
        sellAmountText.text = sellAmount.ToString();

        stack.UpgradeMaxStack();

        Color randomColor = Random.ColorHSV();
        stack.GetComponentInChildren<Renderer>().material.color = randomColor;
    }
}
