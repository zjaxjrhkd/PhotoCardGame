using UnityEngine;

public class HoverManager : MonoBehaviour
{
    private CardState lastHoveredCard;
    private GameMaster gameMaster;

    void Start()
    {
        gameMaster = FindObjectOfType<GameMaster>();
    }

    void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            CardState state = hit.collider.GetComponent<CardState>();

            if (state != null)
            {
                Debug.Log($"[HoverManager] 마우스 감지됨 → {state.name}");

                if (!state.isClick && gameMaster != null && gameMaster.IsInPlayCardList(state.gameObject))
                {
                    if (lastHoveredCard != null && lastHoveredCard != state)
                    {
                        lastHoveredCard.HideHoverUI();
                        Debug.Log($"[HoverManager] 이전 카드 {lastHoveredCard.name} UI 숨김");
                    }

                    state.ShowHoverUI();
                    lastHoveredCard = state;

                    Debug.Log($"[HoverManager] 카드 {state.name} UI 표시");
                    return;
                }
                else
                {
                    Debug.Log("[HoverManager] 상태 조건 불일치 → UI 표시 안함");
                }
            }
            else
            {
                Debug.Log("[HoverManager] 카드 상태 스크립트(CardState) 없음");
            }
        }

        // 마우스가 카드에서 벗어난 경우
        if (lastHoveredCard != null)
        {
            lastHoveredCard.HideHoverUI();
            Debug.Log($"[HoverManager] 카드 {lastHoveredCard.name} 에서 마우스 벗어남 → UI 숨김");
            lastHoveredCard = null;
        }
    }
}
