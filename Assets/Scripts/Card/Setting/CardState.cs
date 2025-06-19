using TMPro;
using UnityEngine;

public class CardState : MonoBehaviour
{
    public bool isClick;
    public bool isFlip;

    public GameObject hoverUI;
    public TextMeshProUGUI hoverText;
    public string cardInfo = "카드 설명";

    
    private bool isHovering = false; // 현재 마우스가 올라간 상태 여부

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

        // CardData 컴포넌트에서 카드 타입 확인
        var cardData = GetComponent<CardData>();
        bool isBuffCard = cardData != null && cardData.cardType == CardData.CardType.Buff;

        // CardManager의 checkCardList에 포함되어 있는지 확인
        bool isInCheckList = false;
        var cardManager = FindObjectOfType<CardManager>();
        if (cardManager != null && cardManager.checkCardList != null)
        {
            isInCheckList = cardManager.checkCardList.Contains(gameObject);
        }

        // 버프카드도 아니고 체크리스트에도 없으면 Background 이동
        Transform bg = transform.Find("Background");
        if (bg != null && !isBuffCard && !isInCheckList)
            bg.position += new Vector3(0f, 1f, -1f);

        if (isBuffCard)
        {
            // 화면 중앙에 hoverUI 표시
            Vector3 screenCenter = Camera.main.WorldToScreenPoint(Vector3.zero);
            hoverUI.transform.position = screenCenter;
        }
        else if (isInCheckList)
        {
            // 체크리스트에 있으면 기존 방식대로 표시
            Vector3 worldPos = (bg != null ? bg.position : transform.position) + new Vector3(0, 2.5f, 0f);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            hoverUI.transform.position = screenPos;
        }
        else
        {
            // 버프카드도 아니고 체크리스트에도 없으면 Background 이동된 위치 기준으로 표시
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

        // CardData 컴포넌트에서 카드 타입 확인
        var cardData = GetComponent<CardData>();
        bool isBuffCard = cardData != null && cardData.cardType == CardData.CardType.Buff;

        // CardManager의 checkCardList에 포함되어 있는지 확인
        bool isInCheckList = false;
        var cardManager = FindObjectOfType<CardManager>();
        if (cardManager != null && cardManager.checkCardList != null)
        {
            isInCheckList = cardManager.checkCardList.Contains(gameObject);
        }

        // 버프카드이거나 체크리스트에 있으면 Background 이동 복귀를 하지 않음
        // 그 외(일반카드 + 체크리스트에 없는 경우)만 Background 이동 복귀
        Transform bg = transform.Find("Background");
        if (bg != null && !isBuffCard && !isInCheckList)
            bg.position -= new Vector3(0f, 1f, -1f);

        if (hoverUI != null)
            hoverUI.SetActive(false);
    }
}
