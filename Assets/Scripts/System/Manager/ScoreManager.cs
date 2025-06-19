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
    public float rate = 1.0f; // ����
    public UIManager uiManager;

    private readonly List<int> dantalkGroup1 = new List<int> { 5, 12, 19, 26, 33, 40, 47, 55 };
    private readonly List<int> dantalkGroup2 = new List<int> { 6, 13, 20, 27, 34, 41, 48, 56 };
    private readonly List<int> dantalkGroup3 = new List<int> { 7, 14, 21, 28, 35, 42, 49, 57 };

    public Dictionary<string, int> lastComboScores = new Dictionary<string, int>();

    public GameMaster gameMaster;

    public bool isBeldirStage = false;
    public bool isSitryStage = false;

    public void Init(GameMaster gameMaster)
    {
        this.gameMaster = gameMaster; // gameMaster �Ҵ�
    }

    // ScoreCalculator���� ������ �ݷ��� Dictionary
    public Dictionary<string, List<int>> collectors = new Dictionary<string, List<int>>()
    {
        { "�������ݷ���", new List<int>{1,8,15,29,36,43,50} },
        { "�����ݷ���", new List<int>{2,9,16,30,37,44,51} },
        { "�̾Ƴ��ݷ���", new List<int>{3,10,17,31,38,45,52} },
        { "��Ʈ���ݷ���", new List<int>{4,11,18,32,39,46,53} },
        { "�����ݷ���", new List<int>{5,12,19,33,40,47, 55} },
        { "�����ݷ���", new List<int>{6,13,20,34,41,48, 56} },
        { "�����ݷ���", new List<int>{7,14,21,35,42,49, 57} },
        { "Vlup", new List<int>{1,2,3,4,5,6,7} },
        { "Vfes1", new List<int>{8,9,10,11} },
        { "Vfes2", new List<int>{12,13,14} },
        { "CheerUp", new List<int>{15,16,17,18,19,20,21} },
        { "ColdSleep", new List<int>{28,29,30,31,32,34,35} },
        { "Daystar", new List<int>{37,38,39} },
        { "Innovill", new List<int>{40,41,42} },
        { "LoveLetter", new List<int>{43,44,45,46,47,48,49} },
        { "Mea", new List<int>{50,51,52,53} },
        { "Dantalk", new List<int>{} },
        { "SpringInnovill", new List<int>{55,56,57} }
    };

    public void InitStageFlags()
    {
        isBeldirStage = false;
        isSitryStage = false;
    }

    private int GetScoreByCount(string name, int count)
    {
        // ��Ʈ�� ���� ��Ģ
        if (name == "Daystar" || name == "Innovill" || name == "Vfes2" || name == "SpringInnovill")
        {
            // Daystar: 37,38,39 (3��), Inovil: 40,41,42 (3��)
            return count == 3 ? 800 : 0;
        }
        else if (name == "Mea" || name == "Vfes1")
        {
            // �ſ�����: 50,51,52,53 (4��)
            return count == 4 ? 1000 : 0;
        }
        else if (name == "Vlup" || name == "CheerUp" ||
                 name == "ColdSleep" || name == "LoveLetter")
        {
            // Vlup!: 1~7, Vfes: 8~14, CheerUp!: 15~21, ColdSleep: 28,29,30,31,32,34,35 (7��), LoveLetter: 43~49 (7��)
            return count == 7 ? 6000 : 0;
        }
        else if (name == "�̾Ƴ��ݷ���")
        {
            // �̾Ƴ��ݷ���: 1~7����� 84���� ����
            return count switch
            {
                1 => 84,
                2 => 168,
                3 => 336,
                4 => 672,
                5 => 1344,
                6 => 2688,
                7 => 5376,
                _ => 0
            };
        }
        // �ݷ���(����): 2�� �̻���� ����, 1~7����� ���� ����
        return count switch
        {
            1 => 10,
            2 => 40,
            3 => 160,
            4 => 640,
            5 => 1280,
            6 => 2560,
            7 => 5120,
            _ => 0
        };
    }



    public Dictionary<string, int> GetMatchedCollectorScores(List<int> ownedCardIds)
    {
        Dictionary<string, int> result = new Dictionary<string, int>();

        // ��Ʈ �̸� ���
        HashSet<string> setNames = new HashSet<string>
    {
        "Vlup", "Vfes1", "Vfes2", "CheerUp", "ColdSleep", "Daystar", "Innovill", "LoveLetter", "Mea","SpringInnovill"
    };

        foreach (var entry in collectors)
        {
            string name = entry.Key;
            List<int> ids = entry.Value;
            int matchCount = ids.Count(id => ownedCardIds.Contains(id));
            int score = 0;

            if (name == "Dantalk")
            {

                // Dantalk ���� üũ
                bool hasGroup1 = dantalkGroup1.Any(id => ownedCardIds.Contains(id));
                bool hasGroup2 = dantalkGroup2.Any(id => ownedCardIds.Contains(id));
                bool hasGroup3 = dantalkGroup3.Any(id => ownedCardIds.Contains(id));
                if (hasGroup1 && hasGroup2 && hasGroup3)
                {
                    score = 300;
                }
            }
            else if (setNames.Contains(name))
            {
                // ��Ʈ: ��� ī�� ���� �ÿ��� ���� �ο�
                if (matchCount == ids.Count)
                {
                    score = GetScoreByCount(name, matchCount);
                }
            }
            else
            {
                // �ݷ���: 2�� �̻� ���� �ú��� ���� �ο�
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

        // �޺� ���� ����� lastComboScores�� ���� (ȣ��UI���� ���� ����)
        lastComboScores = new Dictionary<string, int>(result);

        return result;
    }

    public void CheckAndApplySetBackground(List<GameObject> checkCardList)
    {
        // ��Ʈ �̸� ���
        HashSet<string> setNames = new HashSet<string>
    {
        "Vlup", "Vfes1", "Vfes2", "CheerUp", "ColdSleep", "Daystar", "Innovill", "LoveLetter", "Mea", "SpringInnovill", "Dantalk"
    };

        // ���õ� ī���� cardId ����
        List<int> ownedCardIds = new List<int>();
        foreach (var card in checkCardList)
        {
            CardData cardData = card.GetComponent<CardData>();
            if (cardData != null)
                ownedCardIds.Add(cardData.cardId);
        }

        var combos = GetMatchedCollectorScores(ownedCardIds);

        // ��Ʈ�� �ϼ��� ��쿡�� ��� ���� (Dantalk�� ����)
        foreach (var combo in combos)
        {
            if ((setNames.Contains(combo.Key)) && combo.Value > 0 && combo.Key != "Dantalk")
            {
                string resourcePath = $"Image/BackGround/{combo.Key}";
                Sprite newBackground = Resources.Load<Sprite>(resourcePath);
                if (uiManager != null)
                    uiManager.ChangeStageBackground(newBackground);
                Debug.Log($"[ScoreManager] ��Ʈ/���� ���� �ϼ�: {combo.Key}, ��� ���� �õ� ({resourcePath})");
                break;
            }
        }
    }

    public IEnumerator ApplyCardEffects(List<GameObject> checkCardList)
    {
        // ���纻 ���� (�ݺ� �� ����Ʈ ���� ����)
        var cardListCopy = new List<GameObject>(checkCardList);

        foreach (var card in cardListCopy)
        {
            if (card == null) continue;

            CardData cardData = card.GetComponentInChildren<CardData>();
            if (cardData != null)
            {
                scoreYet += 10;
                cardData.UseEffect();
                resultScore = Mathf.RoundToInt(scoreYet * rate);

                if (uiManager != null)
                    uiManager.UpdateScoreCalUI(rate, scoreYet, resultScore);
            }
            yield return new WaitForSeconds(0.5f); // 0.5�� ���
        }
    }


    public IEnumerator ApplyHandTypeCardEffects(List<GameObject> playCardList, List<GameObject> checkCardList)
    {
        if (playCardList == null || playCardList.Count == 0)
            yield break;

        foreach (var cardObj in playCardList)
        {
            if (cardObj == null || !cardObj.activeInHierarchy)
                continue;

            // checkCardList�� ���Ե� ī��� ȿ�� �ߵ����� ����
            if (checkCardList != null && checkCardList.Contains(cardObj))
                continue;

            CardData cardData = cardObj.GetComponentInChildren<CardData>();
            if (cardData == null)
            {
                Debug.LogWarning($"[ScoreManager] CardData�� ����: ������Ʈ �̸�={cardObj.name}");
                continue;
            }
            Debug.Log($"{cardData.playType}");

            if (cardData.playType == CardData.PlayType.Hand)
            {
                Debug.Log($"[ScoreManager] Hand Ÿ�� ȿ�� �ߵ� ���: ī��ID {cardData.cardId}, ī���: {cardData.cardName}");
                cardData.UseEffect();
                Debug.Log($"[ScoreManager] Hand Ÿ�� ȿ�� �ߵ� �Ϸ�: ī��ID {cardData.cardId}");
                yield return new WaitForSeconds(0.5f);
            }
        }
    }


    /*
    public IEnumerator ApplyHandTypeCardEffects(List<GameObject> playCardList)
    {
        if (playCardList == null || playCardList.Count == 0)
            yield break;

        // CardData ����Ʈ�� ��ȯ
        List<CardData> handCardDataList = new List<CardData>();
        foreach (var cardObj in playCardList)
        {
            if (cardObj == null)
            {
                Debug.LogWarning("[ScoreManager] playCardList�� null ������Ʈ�� �ֽ��ϴ�.");
                continue;
            }
            CardData cardData = cardObj.GetComponent<CardData>();
            if (cardData == null)
            {
                Debug.LogWarning($"[ScoreManager] CardData�� ����: ������Ʈ �̸�={cardObj.name}");
                continue;
            }
            handCardDataList.Add(cardData);
        }

        // Hand Ÿ�Ը� ȿ�� �ߵ�
        foreach (var cardData in handCardDataList)
        {
            if (cardData.playType == CardData.PlayType.Hand)
            {
                Debug.Log($"[ScoreManager] Hand Ÿ�� ȿ�� �ߵ� ���: ī��ID {cardData.cardId}, ī���: {cardData.cardName}");
                cardData.UseEffect();
                Debug.Log($"[ScoreManager] Hand Ÿ�� ȿ�� �ߵ� �Ϸ�: ī��ID {cardData.cardId}");
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

            gameMaster.musicManager.PlayCardEffectSFX(); // SFX ���

            // ���� ��� UI ����
            if (uiManager != null)
                uiManager.UpdateScoreCalUI(rate, scoreYet, resultScore);

            yield return new WaitForSeconds(0.5f); // 0.5�� ���
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