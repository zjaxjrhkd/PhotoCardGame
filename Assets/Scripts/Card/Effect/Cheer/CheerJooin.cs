using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheerJooin : MonoBehaviour, ICardEffect
{
    public void Init(ScoreManager scoreManager, GameMaster gameMaster, CardManager cardManager)
    {
        // �ʿ��ϴٸ� �Ŵ��� ������ �����ؼ� ����ϼ���.
        // ����� ������� ������ ����ֵ� �����մϴ�.
    }

    public void Effect()
    {
        Debug.Log("CheerJooin ȿ�� �ߵ�!");
        // �� ī�常�� ȿ�� ����
    }
}
