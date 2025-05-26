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
        cardInfo = "ī�� ����"; // �ʱ� ī�� ���� ����
    }

    public void CardStateClicked()
    {
        isClick = !isClick;
    }

    public void ShowHoverUI()
    {
        if (isHovering) return; // �̹� �� �ִ� ���¸� �ߺ� ó�� ����
        isHovering = true;

        if (hoverUI == null || hoverText == null) return;

        transform.position += new Vector3(0f, 1f, 0f); // y+1 �̵�

        Vector3 worldPos = transform.position + new Vector3(0, 2.2f, 0f); // ���� y+3.2 ����
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        hoverUI.transform.position = screenPos;
        hoverText.text = cardInfo;
        hoverUI.SetActive(true);
    }

    public void HideHoverUI()
    {
        if (!isHovering) return; // �� ���� ������ �ٽ� ���� �ʿ� ����
        isHovering = false;

        transform.position -= new Vector3(0f, 1f, 0f); // y-1 ����

        if (hoverUI != null)
            hoverUI.SetActive(false);
    }
}
