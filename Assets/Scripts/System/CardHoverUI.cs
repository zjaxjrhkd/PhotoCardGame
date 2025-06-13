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
    
    private void OnMouseEnter()
    {
        if (CanShowHoverUI())
        {
            Vector3 newPos = transform.position + new Vector3(0f, 3.2f, 0f);
            hoverUI.transform.position = newPos;
            hoverText.text = cardInfo;
            hoverUI.SetActive(true);
        }
    }
    
    private void OnMouseExit()
    {
        if (hoverUI != null)
            hoverUI.SetActive(false);
    }

    private bool CanShowHoverUI()
    {
        // GameState�� Draw �����̰� ī�尡 ���õ��� �ʾ��� ���� UI ǥ��
        return gameMaster != null &&
               gameMaster.IsDrawState() &&
               cardState != null &&
               !cardState.isClick;
    }
}
