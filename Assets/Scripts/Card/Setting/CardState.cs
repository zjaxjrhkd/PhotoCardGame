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
        cardInfo = "카드 설명"; // 초기 카드 설명 설정
    }

    public void CardStateClicked()
    {
        isClick = !isClick;
    }

    public void ShowHoverUI()
    {
        if (isHovering) return; // 이미 떠 있는 상태면 중복 처리 금지
        isHovering = true;

        if (hoverUI == null || hoverText == null) return;

        transform.position += new Vector3(0f, 1f, 0f); // y+1 이동

        Vector3 worldPos = transform.position + new Vector3(0, 2.2f, 0f); // 최종 y+3.2 기준
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        hoverUI.transform.position = screenPos;
        hoverText.text = cardInfo;
        hoverUI.SetActive(true);
    }

    public void HideHoverUI()
    {
        if (!isHovering) return; // 떠 있지 않으면 다시 내릴 필요 없음
        isHovering = false;

        transform.position -= new Vector3(0f, 1f, 0f); // y-1 복귀

        if (hoverUI != null)
            hoverUI.SetActive(false);
    }
}
