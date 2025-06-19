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

        // CardManager�� checkCardList�� ���ԵǾ� �ִ��� Ȯ��
        bool isInCheckList = false;
        var cardManager = FindObjectOfType<CardManager>();
        if (cardManager != null && cardManager.checkCardList != null)
        {
            isInCheckList = cardManager.checkCardList.Contains(gameObject);
        }

        // ����ī�嵵 �ƴϰ� üũ����Ʈ���� ������ Background �̵�
        Transform bg = transform.Find("Background");
        if (bg != null && !isBuffCard && !isInCheckList)
            bg.position += new Vector3(0f, 1f, -1f);

        if (isBuffCard)
        {
            // ȭ�� �߾ӿ� hoverUI ǥ��
            Vector3 screenCenter = Camera.main.WorldToScreenPoint(Vector3.zero);
            hoverUI.transform.position = screenCenter;
        }
        else if (isInCheckList)
        {
            // üũ����Ʈ�� ������ ���� ��Ĵ�� ǥ��
            Vector3 worldPos = (bg != null ? bg.position : transform.position) + new Vector3(0, 2.5f, 0f);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            hoverUI.transform.position = screenPos;
        }
        else
        {
            // ����ī�嵵 �ƴϰ� üũ����Ʈ���� ������ Background �̵��� ��ġ �������� ǥ��
            Vector3 worldPos = (bg != null ? bg.position : transform.position) + new Vector3(0, 2.5f, 0f);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            hoverUI.transform.position = screenPos;
        }

        hoverText.text = cardInfo;
        hoverUI.SetActive(true);
    }

    public void HideHoverUI()
    {
        if (!isHovering) return;
        isHovering = false;

        // CardData ������Ʈ���� ī�� Ÿ�� Ȯ��
        var cardData = GetComponent<CardData>();
        bool isBuffCard = cardData != null && cardData.cardType == CardData.CardType.Buff;

        // CardManager�� checkCardList�� ���ԵǾ� �ִ��� Ȯ��
        bool isInCheckList = false;
        var cardManager = FindObjectOfType<CardManager>();
        if (cardManager != null && cardManager.checkCardList != null)
        {
            isInCheckList = cardManager.checkCardList.Contains(gameObject);
        }

        // ����ī���̰ų� üũ����Ʈ�� ������ Background �̵� ���͸� ���� ����
        // �� ��(�Ϲ�ī�� + üũ����Ʈ�� ���� ���)�� Background �̵� ����
        Transform bg = transform.Find("Background");
        if (bg != null && !isBuffCard && !isInCheckList)
            bg.position -= new Vector3(0f, 1f, -1f);

        if (hoverUI != null)
            hoverUI.SetActive(false);
    }
}
