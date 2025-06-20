using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DOTween 네임스페이스 추가

public class BuffGoingChuros : MonoBehaviour, ICardEffect
{
    private ScoreManager scoreManager;
    private GameMaster gameMaster;
    private CardManager cardManager;

    // 세미콜론(;) 제거, 타입 이름 대문자로 수정
    public void Init(ScoreManager scoreManager, GameMaster gameMaster, CardManager cardManager)
    {
        this.scoreManager = scoreManager;
        this.gameMaster = gameMaster;
        this.cardManager = cardManager;
        Debug.Log($"[BuffChuros] Init 호출됨! scoreManager: {(scoreManager != null)}, gameMaster: {(gameMaster != null)}, cardManager: {(cardManager != null)}");
    }

    public void Effect()
    {
        // 코루틴으로 실행
        gameMaster.StartCoroutine(EffectCoroutine());
    }



    private IEnumerator EffectCoroutine()
    {
        int[] targetIds = { 1, 8, 15, 22, 29, 36, 43, 50 };
        bool success = cardManager.DrawRandomTargetCard(targetIds);
        if (success)
        {
            // 카드 흔들림 애니메이션 (Y축으로 0.5만큼 3회)
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
            Debug.Log("드로우 실패(확률 또는 카드 없음)");

        yield break;
    }
}