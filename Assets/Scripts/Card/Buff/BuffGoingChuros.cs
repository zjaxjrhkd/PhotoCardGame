using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DOTween ���ӽ����̽� �߰�

public class BuffGoingChuros : MonoBehaviour, ICardEffect
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
        int[] targetIds = { 1, 8, 15, 22, 29, 36, 43, 50 };
        bool success = cardManager.DrawRandomTargetCard(targetIds);
        if (success)
        {
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
        }
        else
            Debug.Log("��ο� ����(Ȯ�� �Ǵ� ī�� ����)");

        yield break;
    }
}