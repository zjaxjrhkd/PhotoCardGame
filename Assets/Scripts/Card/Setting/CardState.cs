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
        bool isCharacterCard = cardData != null && cardData.cardType == CardData.CardType.Character;

        // CardManager의 checkCardList에 포함되어 있는지 확인
        bool isInCheckList = false;
        var cardManager = FindObjectOfType<CardManager>();
        if (cardManager != null && cardManager.checkCardList != null)
        {
            isInCheckList = cardManager.checkCardList.Contains(gameObject);
        }

        // GameMaster의 gamestate가 Shop인지 확인
        var gameMaster = FindObjectOfType<GameMaster>();
        bool isShopState = (gameMaster != null && gameMaster.gameState == GameMaster.GameState.Shop);

        // 버프카드도 아니고 체크리스트에도 없고, Shop상태가 아니면 Background 이동
        Transform bg = transform.Find("Background");
        if (bg != null && !isBuffCard && !isInCheckList && !isShopState)
            bg.position += new Vector3(0f, 1f, 0f);

        // --- UI 표시 로직 ---
        if (isBuffCard)
        {
            // 버프카드는 기존대로
            Vector3 screenCenter = Camera.main.WorldToScreenPoint(Vector3.zero);
            hoverUI.transform.position = screenCenter;
            int coinCost = cardData != null ? cardData.CardCost : 0;
            hoverText.text = $"{cardInfo}\n가격: {coinCost}원";
        }
        else if (isShopState && isCharacterCard)
        {
            // Shop상태의 캐릭터카드는 가격 포함해서 중앙에 표시
            Vector3 screenCenter = Camera.main.WorldToScreenPoint(Vector3.zero);
            hoverUI.transform.position = screenCenter;
            int coinCost = cardData != null ? cardData.CardCost : 0;
            hoverText.text = $"{cardInfo}\n가격: {coinCost}원";
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

        // GameMaster의 gamestate가 Shop이면 Background 복귀 금지
        var gameMaster = FindObjectOfType<GameMaster>();
        bool isShopState = (gameMaster != null && gameMaster.gameState == GameMaster.GameState.Shop);

        // 버프카드이거나 체크리스트에 있으면 Background 이동 복귀를 하지 않음
        // 그 외(일반카드 + 체크리스트에 없는 경우)만 Background 이동 복귀, 단 Shop상태는 금지
        Transform bg = transform.Find("Background");
        if (bg != null && !isBuffCard && !isInCheckList && !isShopState)
            bg.position -= new Vector3(0f, 1f, 0f);

        if (hoverUI != null)
            hoverUI.SetActive(false);
    }

    public void ResetCardPosition()
    {
        // 카드 위치 초기화
        Transform bg = transform.Find("Background");
        if (bg != null)
        {
            bg.position = new Vector3(bg.position.x, -1, bg.position.z); // Y축을 0으로 초기화
        }
        
        // 클릭 상태 초기화
        isClick = false;
    }
}
