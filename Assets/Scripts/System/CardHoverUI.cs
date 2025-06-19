using UnityEngine;
using TMPro;

public class CardHoverUI : MonoBehaviour
{
    public GameObject hoverUI;                  // UI ������Ʈ
    public TextMeshProUGUI hoverText;           // �ؽ�Ʈ
    public string cardInfo = "ī�� ���� ����";   // ���� ����

    private CardState cardState;                // ī�� ����
    private GameMaster gameMaster;              // ���� ���� üũ��

    private void Start()
    {
        cardState = GetComponent<CardState>();
        gameMaster = FindObjectOfType<GameMaster>();
        if (hoverUI != null)
            hoverUI.SetActive(false); // ���� �� ������
    }
    /*
    private void OnMouseEnter()
    {
        if (CanShowHoverUI())
        {
            Vector3 newPos;
            var cardData = GetComponent<CardData>();
            if (cardData != null && cardData.cardType == CardData.CardType.Buff)
            {
                Debug.Log($"[CardHoverUI] ����ī���Դϴ�: {gameObject.name} (cardId: {cardData.cardId})");
                newPos = new Vector3(0f, 0f, -1f);
            }
            else
            {
                Debug.Log($"[CardHoverUI] �Ϲ�ī�� �Ǵ� ��Ÿ: {gameObject.name}");
                newPos = transform.position + new Vector3(0f, 3.2f, 0f);
            }
            hoverUI.transform.position = newPos;
            hoverText.text = cardInfo;
            hoverUI.SetActive(true);
        }
    }
    */
    /*
    private void OnMouseExit()
    {
        if (hoverUI != null)
            hoverUI.SetActive(false);
    }

    private bool CanShowHoverUI()
    {
        // ī�尡 �ƴϸ�(�޺�/��������/���� ��) �׻� ǥ��, ī��� ���� ����
        if (cardState == null)
            return true;

        return gameMaster != null &&
               gameMaster.IsDrawState() &&
               !cardState.isClick;
    }
    */
}
