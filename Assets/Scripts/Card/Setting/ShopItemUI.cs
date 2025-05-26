using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public Button buyButton;

    private GameObject cardPrefab;
    private int price;
    private GameMaster gameMaster;

    public void Setup(GameObject cardPrefab, int price, GameMaster gameMaster)
    {
        this.cardPrefab = cardPrefab;
        this.price = price;
        this.gameMaster = gameMaster;

        CardData data = cardPrefab.GetComponent<CardData>();
        if (data != null)
        {
            nameText.text = data.cardName;
        }
        else
        {
            nameText.text = "Unknown";
        }

        priceText.text = $"{price} Coin";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(BuyThisCard);
    }

    private void BuyThisCard()
    {
        if (gameMaster.coin < price)
        {
            Debug.Log("������ �����մϴ�.");
            return;
        }

        gameMaster.SpendCoin(price);

        CardData data = cardPrefab.GetComponent<CardData>();
        if (data == null)
        {
            Debug.LogWarning("CardData�� ����");
            return;
        }

        if (data.cardType == CardData.CardType.Buff)
        {
            gameMaster.buffCardList.Add(cardPrefab);
            Debug.Log($"{data.cardName} ���� ī�� ���� �Ϸ�");
        }
        else
        {
            gameMaster.deckList.Add(data.cardId);
            Debug.Log($"{data.cardName} �Ϲ� ī�� ���� �Ϸ�");
        }

        gameObject.SetActive(false); // UI���� ��Ȱ��ȭ (���� �Ϸ� ǥ��)
    }
}
