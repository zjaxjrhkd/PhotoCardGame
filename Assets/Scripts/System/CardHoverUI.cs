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
        // GameState가 Draw 상태이고 카드가 선택되지 않았을 때만 UI 표시
        return gameMaster != null &&
               gameMaster.IsDrawState() &&
               cardState != null &&
               !cardState.isClick;
    }
}
