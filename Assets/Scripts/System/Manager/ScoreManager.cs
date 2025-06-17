using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public class ScoreManager : MonoBehaviour
{
    public int score;
    public int scoreYet;
    public int resultScore;
    public float rate = 1.0f; // 배율
    public UIManager uiManager;

    private readonly List<int> dantalkGroup1 = new List<int> { 5, 12, 19, 26, 33, 40, 47 };
    private readonly List<int> dantalkGroup2 = new List<int> { 6, 13, 20, 27, 34, 41, 48 };
    private readonly List<int> dantalkGroup3 = new List<int> { 7, 14, 21, 28, 35, 42, 49 };

    public Dictionary<string, int> lastComboScores = new Dictionary<string, int>();


    public bool isBeldirStage = false;
    public bool isSitryStage = false;

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
        { "Vfes1", new List<int>{8,9,10,11} },
        { "Vfes2", new List<int>{12,13,14} },
        { "CheerUp", new List<int>{15,16,17,18,19,20,21} },
        { "ColdSleep", new List<int>{28,29,30,31,32,34,35} },
        { "Daystar", new List<int>{37,38,39} },
        { "Innovill", new List<int>{40,41,42} },
        { "LoveLetter", new List<int>{43,44,45,46,47,48,49} },
        { "Mea", new List<int>{50,51,52,53} },
        { "Dantalk", new List<int>{} }
    };

    public void InitStageFlags()
    {
        isBeldirStage = false;
        isSitryStage = false;
    }

    private int GetScoreByCount(string name, int count)
    {
        // 세트별 점수 규칙
        if (name == "Daystar" || name == "Innovill" || name == "Vfes2")
        {
            // Daystar: 37,38,39 (3장), Inovil: 40,41,42 (3장)
            return count == 3 ? 500 : 0;
        }
        else if (name == "Mea" || name == "Vfes1")
        {
            // 매월매주: 50,51,52,53 (4장)
            return count == 4 ? 600 : 0;
        }
        else if (name == "Vlup" || name == "CheerUp" ||
                 name == "ColdSleep" || name == "LoveLetter")
        {
            // Vlup!: 1~7, Vfes: 8~14, CheerUp!: 15~21, ColdSleep: 28,29,30,31,32,34,35 (7장), LoveLetter: 43~49 (7장)
            return count == 7 ? 1000 : 0;
        }
        else if (name == "이아나콜렉터")
        {
            // 이아나콜렉터: 1~7장까지 84점씩 누적
            return count switch
            {
                1 => 84,
                2 => 168,
                3 => 252,
                4 => 336,
                5 => 420,
                6 => 504,
                7 => 588,
                _ => 0
            };
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
        "Vlup", "Vfes1", "Vfes2", "CheerUp", "ColdSleep", "Daystar", "Innovill", "LoveLetter", "Mea"
    };

        foreach (var entry in collectors)
        {
            string name = entry.Key;
            List<int> ids = entry.Value;
            int matchCount = ids.Count(id => ownedCardIds.Contains(id));
            int score = 0;

            if (name == "Dantalk")
            {
                // Dantalk 조건 체크
                bool hasGroup1 = dantalkGroup1.Any(id => ownedCardIds.Contains(id));
                bool hasGroup2 = dantalkGroup2.Any(id => ownedCardIds.Contains(id));
                bool hasGroup3 = dantalkGroup3.Any(id => ownedCardIds.Contains(id));
                if (hasGroup1 && hasGroup2 && hasGroup3)
                {
                    score = 777;
                }
            }
            else if (setNames.Contains(name))
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

        // 콤보 점수 결과를 lastComboScores에 저장 (호버UI에서 접근 가능)
        lastComboScores = new Dictionary<string, int>(result);

        return result;
    }

    public void CheckAndApplySetBackground(List<GameObject> checkCardList)
    {
        // 세트 이름 목록
        HashSet<string> setNames = new HashSet<string>
    {
        "Vlup", "Vfes1", "Vfes2", "CheerUp", "ColdSleep", "Daystar", "Innovill", "LoveLetter", "Mea"
    };

        // 선택된 카드의 cardId 수집
        List<int> ownedCardIds = new List<int>();
        foreach (var card in checkCardList)
        {
            CardData cardData = card.GetComponent<CardData>();
            if (cardData != null)
                ownedCardIds.Add(cardData.cardId);
        }

        var combos = GetMatchedCollectorScores(ownedCardIds);

        // 세트가 완성된 경우에만 배경 변경
        foreach (var combo in combos)
        {
            if ((setNames.Contains(combo.Key) || combo.Key == "Dantalk") && combo.Value > 0)
            {
                string resourcePath = $"Image/BackGround/{combo.Key}";
                Sprite newBackground = Resources.Load<Sprite>(resourcePath);
                if (uiManager != null)
                    uiManager.ChangeStageBackground(newBackground);
                Debug.Log($"[ScoreManager] 세트/단톡 조합 완성: {combo.Key}, 배경 변경 시도 ({resourcePath})");
                break;
            }
        }
    }

    public IEnumerator ApplyCardEffects(List<GameObject> checkCardList)
    {
        // 복사본 생성 (반복 중 리스트 변경 방지)
        var cardListCopy = new List<GameObject>(checkCardList);

        foreach (var card in cardListCopy)
        {
            if (card == null) continue;

            CardData cardData = card.GetComponent<CardData>();
            if (cardData != null)
            {
                scoreYet += 10;
                cardData.UseEffect();
                resultScore = Mathf.RoundToInt(scoreYet * rate);

                if (uiManager != null)
                    uiManager.UpdateScoreCalUI(rate, scoreYet, resultScore);
            }
            yield return new WaitForSeconds(0.5f); // 0.5초 대기
        }
    }

    
    public IEnumerator ApplyHandTypeCardEffects(List<GameObject> playCardList)
    {
        if (playCardList == null || playCardList.Count == 0)
            yield break;

        foreach (var cardObj in playCardList)
        {
            if (cardObj == null)
            {
                Debug.LogWarning("[ScoreManager] playCardList에 null 오브젝트가 있습니다.");
                continue;
            }

            // playCardList에는 반드시 CardData가 붙은 Object-Data 오브젝트만 들어가야 함
            CardData cardData = cardObj.GetComponentInChildren<CardData>();
            if (cardData == null)
            {
                Debug.LogWarning($"[ScoreManager] CardData가 없음: 오브젝트 이름={cardObj.name}");
                continue;
            }
            Debug.Log($"{cardData.playType}");

            if (cardData.playType == CardData.PlayType.Hand)
            {
                Debug.Log($"[ScoreManager] Hand 타입 효과 발동 대상: 카드ID {cardData.cardId}, 카드명: {cardData.cardName}");
                cardData.UseEffect();
                Debug.Log($"[ScoreManager] Hand 타입 효과 발동 완료: 카드ID {cardData.cardId}");
                yield return new WaitForSeconds(0.5f);
            }
        }
    }


    /*
    public IEnumerator ApplyHandTypeCardEffects(List<GameObject> playCardList)
    {
        if (playCardList == null || playCardList.Count == 0)
            yield break;

        // CardData 리스트로 변환
        List<CardData> handCardDataList = new List<CardData>();
        foreach (var cardObj in playCardList)
        {
            if (cardObj == null)
            {
                Debug.LogWarning("[ScoreManager] playCardList에 null 오브젝트가 있습니다.");
                continue;
            }
            CardData cardData = cardObj.GetComponent<CardData>();
            if (cardData == null)
            {
                Debug.LogWarning($"[ScoreManager] CardData가 없음: 오브젝트 이름={cardObj.name}");
                continue;
            }
            handCardDataList.Add(cardData);
        }

        // Hand 타입만 효과 발동
        foreach (var cardData in handCardDataList)
        {
            if (cardData.playType == CardData.PlayType.Hand)
            {
                Debug.Log($"[ScoreManager] Hand 타입 효과 발동 대상: 카드ID {cardData.cardId}, 카드명: {cardData.cardName}");
                cardData.UseEffect();
                Debug.Log($"[ScoreManager] Hand 타입 효과 발동 완료: 카드ID {cardData.cardId}");
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
    */

    public IEnumerator ApplyCollectorCombosCoroutine(List<GameObject> checkCardList)
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
            scoreYet += combo.Value;
            resultScore = Mathf.RoundToInt(scoreYet * rate);

            // 점수 계산 UI 갱신
            if (uiManager != null)
                uiManager.UpdateScoreCalUI(rate, scoreYet, resultScore);

            yield return new WaitForSeconds(0.5f); // 0.5초 대기
        }
    }

    public int CalculateResultScore()
    {
        resultScore = Mathf.RoundToInt(scoreYet * rate);
        Debug.Log($"[ScoreManager] scoreYet: {scoreYet}, rate: {rate}, resultScore: {resultScore}");
        score += resultScore;
        return resultScore;
    }

    public void SetScore()
    {
        score = 0;
    }

    public void SetCalculateResutScore()
    {
        scoreYet = 0;
        rate = 1.0f;
        resultScore = 0;
    }
}