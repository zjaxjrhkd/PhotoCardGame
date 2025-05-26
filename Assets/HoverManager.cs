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
                Debug.Log($"[HoverManager] ���콺 ������ �� {state.name}");

                if (!state.isClick && gameMaster != null && gameMaster.IsInPlayCardList(state.gameObject))
                {
                    if (lastHoveredCard != null && lastHoveredCard != state)
                    {
                        lastHoveredCard.HideHoverUI();
                        Debug.Log($"[HoverManager] ���� ī�� {lastHoveredCard.name} UI ����");
                    }

                    state.ShowHoverUI();
                    lastHoveredCard = state;

                    Debug.Log($"[HoverManager] ī�� {state.name} UI ǥ��");
                    return;
                }
                else
                {
                    Debug.Log("[HoverManager] ���� ���� ����ġ �� UI ǥ�� ����");
                }
            }
            else
            {
                Debug.Log("[HoverManager] ī�� ���� ��ũ��Ʈ(CardState) ����");
            }
        }

        // ���콺�� ī�忡�� ��� ���
        if (lastHoveredCard != null)
        {
            lastHoveredCard.HideHoverUI();
            Debug.Log($"[HoverManager] ī�� {lastHoveredCard.name} ���� ���콺 ��� �� UI ����");
            lastHoveredCard = null;
        }
    }
}
