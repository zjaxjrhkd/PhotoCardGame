using UnityEngine;
using TMPro;

public class CardHoverUI : MonoBehaviour
{
    public GameObject hoverUI;                  // UI 오브젝트
    public TextMeshProUGUI hoverText;           // 텍스트
    public string cardInfo = "카드 설명 예시";   // 설명 문구

    private CardState cardState;                // 카드 상태
    private GameMaster gameMaster;              // 게임 상태 체크용

    private void Start()
    {
        cardState = GetComponent<CardState>();
        gameMaster = FindObjectOfType<GameMaster>();
        if (hoverUI != null)
            hoverUI.SetActive(false); // 시작 시 꺼놓기
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
                Debug.Log($"[CardHoverUI] 버프카드입니다: {gameObject.name} (cardId: {cardData.cardId})");
                newPos = new Vector3(0f, 0f, -1f);
            }
            else
            {
                Debug.Log($"[CardHoverUI] 일반카드 또는 기타: {gameObject.name}");
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
        // 카드가 아니면(콤보/스테이지/상점 등) 항상 표시, 카드면 기존 조건
        if (cardState == null)
            return true;

        return gameMaster != null &&
               gameMaster.IsDrawState() &&
               !cardState.isClick;
    }
    */
}
