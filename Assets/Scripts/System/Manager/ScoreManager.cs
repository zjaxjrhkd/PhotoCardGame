using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ScoreManager : MonoBehaviour
{
    public int score;

    // ScoreCalculator에서 가져온 콜렉터 Dictionary
    private Dictionary<string, List<int>> collectors = new Dictionary<string, List<int>>()
    {
        { "이주인콜렉터", new List<int>{1,8,15,22,29,36,43,50} },
        { "벨디르콜렉터", new List<int>{2,9,16,23,30,37,44,51} },
        { "이아나콜렉터", new List<int>{3,10,17,24,31,38,45,52} },
        { "시트리콜렉터", new List<int>{4,11,18,25,32,39,46,53} },
        { "노이콜렉터", new List<int>{5,12,19,26,33,40,47} },
        { "리미콜렉터", new List<int>{6,13,20,27,34,41,48} },
        { "라즈콜렉터", new List<int>{7,14,21,28,35,42,49} },
        { "Vlup!", new List<int>{1,2,3,4,5,6,7} },
        { "Vfes", new List<int>{8,9,10,11,12,13,14} },
        { "CheerUp!", new List<int>{15,16,17,18,19,20,21} },
        { "ColdSleep", new List<int>{28,29,30,31,32,34,35} },
        { "Daystar", new List<int>{37,38,39} },
        { "Inovil", new List<int>{40,41,42} },
        { "LoveLetter", new List<int>{43,44,45,46,47,48,49} },
        { "매월매주", new List<int>{50,51,52,53} },
    };

    private int GetScoreByCount(string name, int count)
    {
        if (name == "Daystar" || name == "Inovil")
        {
            return count == 3 ? 500 : 0;
        }
        else if (name == "매월매주")
        {
            return count == 4 ? 600 : 0;
        }

        return count switch
        {
            1 => 10,
            2 => 20,
            3 => 40,
            4 => 80,
            5 => 160,
            6 => 320,
            7 => 640,
            _ => 0
        };
    }

    // 이름 + 점수 세트 반환
    public Dictionary<string, int> GetMatchedCollectorScores(List<int> ownedCardIds)
    {
        Dictionary<string, int> result = new Dictionary<string, int>();

        foreach (var entry in collectors)
        {
            string name = entry.Key;
            List<int> ids = entry.Value;

            int matchCount = ids.Count(id => ownedCardIds.Contains(id));
            int score = GetScoreByCount(name, matchCount);

            if (score > 0)
            {
                result[name] = score;
            }
        }

        return result;
    }

    // 예시: 카드 효과 적용
    public void ApplyCardEffects(List<GameObject> checkCardList)
    {
        foreach (var card in checkCardList)
        {
            CardData cardData = card.GetComponent<CardData>();
            if (cardData != null)
            {
                // 예시: 카드마다 10점 추가
                score += 10;
            }
        }
    }

    // 예시: 콜렉터 조합 점수 적용
    public void ApplyCollectorCombos(List<GameObject> checkCardList, TMPro.TextMeshProUGUI resultText)
    {
        List<int> ownedCardIds = new List<int>();
        foreach (var card in checkCardList)
        {
            CardData cardData = card.GetComponent<CardData>();
            if (cardData != null)
                ownedCardIds.Add(cardData.cardId);
        }

        var combos = GetMatchedCollectorScores(ownedCardIds);
        foreach (var combo in combos)
        {
            score += combo.Value;
            if (resultText != null)
                resultText.text += $"{combo.Key} +{combo.Value}점\n";
        }
    }

    public void SetScore()
    {
        score = 0;
    }
}