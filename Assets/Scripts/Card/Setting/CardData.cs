using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // 파일 상단에 추가

public class CardData : MonoBehaviour
{
    public int cardId;
    public string cardName;

    public enum CardType { Character, Buff }
    public CardType cardType;

    public enum BuffType { None, Always, OnStart } // 추가
    public BuffType buffType;

    public enum PlayType { Hand, Check }
    public PlayType playType;

    public enum CharacterType { Jooin, Beldir, Iana, Sitry, Noi, Limi, Raz, Churos, Chami, Donddatge, Muddung, Rasky, Roze, Yasu }
    public CharacterType characterType;

    public int CardCost;

    public GameMaster gameMaster; // 필드 추가


    public void InitEffects(ScoreManager scoreManager, GameMaster gameMaster, CardManager cardManager)
    {
        this.gameMaster = gameMaster; // gameMaster 할당
        var effects = GetComponents<ICardEffect>();
        foreach (var effect in effects)
        {
            effect.Init(scoreManager, gameMaster, cardManager);
        }
    }

    public void UseEffect()
    {
        // 버프카드는 흔들림 애니메이션 생략
        if (cardType != CardType.Buff)
        {
            // 카드 흔들림 애니메이션 (Y축으로 0.5만큼 3회)
            Transform tr = this.transform;
            float duration = 0.12f; // 한 번 왕복 시간
            int loopCount = 3;

            // 원래 위치 저장
            Vector3 origin = tr.position;

            // DOTween 시퀀스
            Sequence seq = DOTween.Sequence();
            for (int i = 0; i < loopCount; i++)
            {
                seq.Append(tr.DOMoveY(origin.y + 0.5f, duration).SetEase(Ease.OutQuad));
                seq.Append(tr.DOMoveY(origin.y, duration).SetEase(Ease.InQuad));
            }
            seq.Play();

            // SFX 재생
            if (gameMaster != null && gameMaster.musicManager != null)
                gameMaster.musicManager.PlayCardEffectSFX();
        }

        // 기존 효과 실행
        var effects = GetComponents<ICardEffect>();
        foreach (var effect in effects)
        {
            effect.Effect();
        }
    }
}
