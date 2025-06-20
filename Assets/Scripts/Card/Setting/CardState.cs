using TMPro;
using UnityEngine;

public class CardState : MonoBehaviour
{
    public bool isClick;
    public bool isFlip;

    public GameObject hoverUI;
    public TextMeshProUGUI hoverText;
    public string cardInfo = "ī�� ����";

    
    private bool isHovering = false; // ���� ���콺�� �ö� ���� ����

    private void Start()
    {
        if (hoverUI != null)
            hoverUI.SetActive(false);
    }

    public void CardStateClicked()
    {
        isClick = !isClick;
    }

    public void ShowHoverUI()
    {
        if (isHovering) return;
        isHovering = true;

        if (hoverUI == null || hoverText == null) return;

        // CardData ������Ʈ���� ī�� Ÿ�� Ȯ��
        var cardData = GetComponent<CardData>();
        bool isBuffCard = cardData != null && cardData.cardType == CardData.CardType.Buff;
        bool isCharacterCard = cardData != null && cardData.cardType == CardData.CardType.Character;

        // CardManager�� checkCardList�� ���ԵǾ� �ִ��� Ȯ��
        bool isInCheckList = false;
        var cardManager = FindObjectOfType<CardManager>();
        if (cardManager != null && cardManager.checkCardList != null)
        {
            isInCheckList = cardManager.checkCardList.Contains(gameObject);
        }

        // GameMaster�� gamestate�� Shop���� Ȯ��
        var gameMaster = FindObjectOfType<GameMaster>();
        bool isShopState = (gameMaster != null && gameMaster.gameState == GameMaster.GameState.Shop);

        // ����ī�嵵 �ƴϰ� üũ����Ʈ���� ����, Shop���°� �ƴϸ� Background �̵�
        Transform bg = transform.Find("Background");
        if (bg != null && !isBuffCard && !isInCheckList && !isShopState)
            bg.position += new Vector3(0f, 1f, 0f);

        // --- UI ǥ�� ���� ---
        if (isBuffCard)
        {
            // ����ī��� �������
            Vector3 screenCenter = Camera.main.WorldToScreenPoint(Vector3.zero);
            hoverUI.transform.position = screenCenter;
            int coinCost = cardData != null ? cardData.CardCost : 0;
            hoverText.text = $"{cardInfo}\n����: {coinCost}��";
        }
        else if (isShopState && isCharacterCard)
        {
            // Shop������ ĳ����ī��� ���� �����ؼ� �߾ӿ� ǥ��
            Vector3 screenCenter = Camera.main.WorldToScreenPoint(Vector3.zero);
            hoverUI.transform.position = screenCenter;
            int coinCost = cardData != null ? cardData.CardCost : 0;
            hoverText.text = $"{cardInfo}\n����: {coinCost}��";
        }
        else if (isInCheckList)
        {
            Vector3 worldPos = (bg != null ? bg.position : transform.position) + new Vector3(0, 2.5f, 0f);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            hoverUI.transform.position = screenPos;
            hoverText.text = cardInfo;
        }
        else
        {
            Vector3 worldPos = (bg != null ? bg.position : transform.position) + new Vector3(0, 2.5f, 0f);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            hoverUI.transform.position = screenPos;
            hoverText.text = cardInfo;
        }

        hoverUI.SetActive(true);
    }

    public void HideHoverUI()
    {
        if (!isHovering) return;
        isHovering = false;

        var cardData = GetComponent<CardData>();
        bool isBuffCard = cardData != null && cardData.cardType == CardData.CardType.Buff;

        bool isInCheckList = false;
        var cardManager = FindObjectOfType<CardManager>();
        if (cardManager != null && cardManager.checkCardList != null)
        {
            isInCheckList = cardManager.checkCardList.Contains(gameObject);
        }

        // GameMaster�� gamestate�� Shop�̸� Background ���� ����
        var gameMaster = FindObjectOfType<GameMaster>();
        bool isShopState = (gameMaster != null && gameMaster.gameState == GameMaster.GameState.Shop);

        // ����ī���̰ų� üũ����Ʈ�� ������ Background �̵� ���͸� ���� ����
        // �� ��(�Ϲ�ī�� + üũ����Ʈ�� ���� ���)�� Background �̵� ����, �� Shop���´� ����
        Transform bg = transform.Find("Background");
        if (bg != null && !isBuffCard && !isInCheckList && !isShopState)
            bg.position -= new Vector3(0f, 1f, 0f);

        if (hoverUI != null)
            hoverUI.SetActive(false);
    }

    public void ResetCardPosition()
    {
        // ī�� ��ġ �ʱ�ȭ
        Transform bg = transform.Find("Background");
        if (bg != null)
        {
            bg.position = new Vector3(bg.position.x, -1, bg.position.z); // Y���� 0���� �ʱ�ȭ
        }
        
        // Ŭ�� ���� �ʱ�ȭ
        isClick = false;
    }
}
