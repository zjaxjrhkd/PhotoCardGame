using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public int score;

    // ScoreCalculator에서 가져온 콜렉터 Dictionary
    public Dictionary<string, List<int>> collectors = new Dictionary<string, List<int>>()
    {
        { "이주인콜렉터", new List<int>{1,8,15,22,29,36,43,50} },
        { "벨디르콜렉터", new List<int>{2,9,16,23,30,37,44,51} },
        { "이아나콜렉터", new List<int>{3,10,17,24,31,38,45,52} },
        { "시트리콜렉터", new List<int>{4,11,18,25,32,39,46,53} },
        { "노이콜렉터", new List<int>{5,12,19,26,33,40,47} },
        { "리미콜렉터", new List<int>{6,13,20,27,34,41,48} },
        { "라즈콜렉터", new List<int>{7,14,21,28,35,42,49} },
        { "Vlup", new List<int>{1,2,3,4,5,6,7} },
        { "Vfes", new List<int>{8,9,10,11,12,13,14} },
        { "CheerUp", new List<int>{15,16,17,18,19,20,21} },
        { "ColdSleep", new List<int>{28,29,30,31,32,34,35} },
        { "Daystar", new List<int>{37,38,39} },
        { "Innovill", new List<int>{40,41,42} },
        { "LoveLetter", new List<int>{43,44,45,46,47,48,49} },
        { "Mea", new List<int>{50,51,52,53} },
    };

    private int GetScoreByCount(string name, int count)
    {
        // 세트별 점수 규칙
        if (name == "Daystar" || name == "Innovill")
        {
            // Daystar: 37,38,39 (3장), Inovil: 40,41,42 (3장)
            return count == 3 ? 500 : 0;
        }
        else if (name == "Mea")
        {
            // 매월매주: 50,51,52,53 (4장)
            return count == 4 ? 600 : 0;
        }
        else if (name == "Vlup" || name == "Vfes" || name == "CheerUp" ||
                 name == "ColdSleep" || name == "LoveLetter")
        {
            // Vlup!: 1~7, Vfes: 8~14, CheerUp!: 15~21, ColdSleep: 28,29,30,31,32,34,35 (7장), LoveLetter: 43~49 (7장)
            return count == 7 ? 1000 : 0;
        }

        // 콜렉터(개별): 2장 이상부터 점수, 1~7장까지 점수 차등
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



    public Dictionary<string, int> GetMatchedCollectorScores(List<int> ownedCardIds)
    {
        Dictionary<string, int> result = new Dictionary<string, int>();

        // 세트 이름 목록
        HashSet<string> setNames = new HashSet<string>
    {
        "Vlup", "Vfes", "CheerUp", "ColdSleep", "Daystar", "Innovill", "LoveLetter", "Mea"
    };

        foreach (var entry in collectors)
        {
            string name = entry.Key;
            List<int> ids = entry.Value;
            int matchCount = ids.Count(id => ownedCardIds.Contains(id));
            int score = 0;

            if (setNames.Contains(name))
            {
                // 세트: 모든 카드 소유 시에만 점수 부여
                if (matchCount == ids.Count)
                {
                    score = GetScoreByCount(name, matchCount);
                }
            }
            else
            {
                // 콜렉터: 2개 이상 소유 시부터 점수 부여
                if (matchCount >= 2)
                {
                    score = GetScoreByCount(name, matchCount);
                }
            }

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
    public void ApplyCollectorCombos(List<GameObject> checkCardList)
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
        }
    }

    public void SetScore()
    {
        score = 0;
    }
}