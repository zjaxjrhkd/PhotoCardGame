using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public Button buyButton;

    private int cardId;
    private int price;
    private GameMaster gameMaster;
    private BuffCardListSO buffCardListSO;

    public void Setup(int cardId, int price, GameMaster gameMaster, BuffCardListSO buffCardListSO)
    {
        this.cardId = cardId;
        this.price = price;
        this.gameMaster = gameMaster;
        this.buffCardListSO = buffCardListSO;
    }

}