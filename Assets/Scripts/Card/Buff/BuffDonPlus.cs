using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DOTween ���ӽ����̽� �߰�

public class BuffDonPlus : MonoBehaviour, ICardEffect
{
    private ScoreManager scoreManager;
    private GameMaster gameMaster;
    private CardManager cardManager;

    // �����ݷ�(;) ����, Ÿ�� �̸� �빮�ڷ� ����
    public void Init(ScoreManager scoreManager, GameMaster gameMaster, CardManager cardManager)
    {
        this.scoreManager = scoreManager;
        this.gameMaster = gameMaster;
        this.cardManager = cardManager;
        Debug.Log($"[BuffChuros] Init ȣ���! scoreManager: {(scoreManager != null)}, gameMaster: {(gameMaster != null)}, cardManager: {(cardManager != null)}");
    }

    public void Effect()
    {
        // �ڷ�ƾ���� ����
        gameMaster.StartCoroutine(EffectCoroutine());
    }


    private IEnumerator EffectCoroutine()
    {
        int[] targetIds = { 6, 13, 20, 27, 34, 41, 48 };
        foreach (var card in new List<GameObject>(cardManager.checkCardList))
        {
            if (card == null) continue;
            CardData data = card.GetComponent<CardData>();
            if (data != null && System.Array.Exists(targetIds, id => id == data.cardId))
            {
                scoreManager.rate += 0.2f;
                gameMaster.SpendCoin(1);
                // ī�� ��鸲 �ִϸ��̼� (Y������ 0.5��ŭ 3ȸ)
                Transform tr = this.transform;
                float duration = 0.12f;
                int loopCount = 2;
                Vector3 origin = tr.position;
                Sequence seq = DG.Tweening.DOTween.Sequence();
                for (int i = 0; i < loopCount; i++)
                {
                    seq.Append(tr.DOMoveY(origin.y + 0.5f, duration).SetEase(Ease.OutQuad));
                    seq.Append(tr.DOMoveY(origin.y, duration).SetEase(Ease.InQuad));
                }
                seq.Play();

                if (gameMaster != null && gameMaster.musicManager != null)
                    gameMaster.musicManager.PlayCardEffectSFX();

                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
