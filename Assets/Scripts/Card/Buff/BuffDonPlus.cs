using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DOTween 네임스페이스 추가

public class BuffDonPlus : MonoBehaviour, ICardEffect
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
        int[] targetIds = { 6, 13, 20, 27, 34, 41, 48 };
        foreach (var card in new List<GameObject>(cardManager.checkCardList))
        {
            if (card == null) continue;
            CardData data = card.GetComponent<CardData>();
            if (data != null && System.Array.Exists(targetIds, id => id == data.cardId))
            {
                scoreManager.rate += 0.2f;
                gameMaster.SpendCoin(1);
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

                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
