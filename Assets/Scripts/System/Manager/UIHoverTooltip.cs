using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class UIHoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject hoverUI;
    public TextMeshProUGUI hoverText;
    [TextArea] public string info;

    // 콤보 이름을 지정 (콤보 이미지에만 사용)
    public string comboName;

    // 콤보별 설명 매핑 (인스펙터에서 추가)
    [System.Serializable]
    public class ComboInfo
    {
        public string comboName;
        [TextArea]
        public string info;
    }
    public List<ComboInfo> comboInfos;

    // 스테이지별 설명
    [TextArea] public string jooinInfo;
    [TextArea] public string beldirInfo;
    [TextArea] public string ianaInfo;
    [TextArea] public string sitryInfo;
    [TextArea] public string noiInfo;
    [TextArea] public string limiInfo;
    [TextArea] public string razInfo;
    [TextArea] public string churosInfo;
    [TextArea] public string chamiInfo;
    [TextArea] public string donddatgeInfo;
    [TextArea] public string muddungInfo;
    [TextArea] public string raskyInfo;
    [TextArea] public string rozeInfo;
    [TextArea] public string yasuInfo;

    private StageManager stageManager;
    private ScoreManager scoreManager;
    private CardManager cardManager;

    private void Start()
    {
        stageManager = FindObjectOfType<StageManager>();
        scoreManager = FindObjectOfType<ScoreManager>();
        cardManager = FindObjectOfType<CardManager>();
        if (hoverUI != null)
            hoverUI.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverUI != null)
        {
            // 화면 중앙에 위치시키기 (Canvas 기준)
            RectTransform rect = hoverUI.GetComponent<RectTransform>();
            if (rect != null)
                rect.anchoredPosition = Vector2.zero;

            hoverText.text = GetTooltipInfo();
            hoverUI.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverUI != null)
            hoverUI.SetActive(false);
    }


    private string GetTooltipInfo()
    {
        // 1. 콤보 이름이 지정되어 있으면 콤보 info + 점수 + 보유 개수/전체 개수
        if (!string.IsNullOrEmpty(comboName))
        {
            string comboDesc = null;
            if (comboInfos != null)
            {
                foreach (var c in comboInfos)
                {
                    if (c.comboName == comboName)
                    {
                        comboDesc = string.IsNullOrEmpty(c.info) ? info : c.info;
                        break;
                    }
                }
            }

            // ScoreManager에서 콤보 점수와 CardManager에서 보유 개수 가져오기
            int comboScore = 0;
            int ownedCount = 0;
            int totalCount = 0;
            if (scoreManager != null)
            {
                // 점수
                if (scoreManager.lastComboScores != null)
                    scoreManager.lastComboScores.TryGetValue(comboName, out comboScore);

                // 전체 개수
                if (scoreManager.collectors != null && scoreManager.collectors.ContainsKey(comboName))
                    totalCount = scoreManager.collectors[comboName].Count;
            }

            // 보유 개수: CardManager에서 현재 checkCardList 기준으로 콜렉터별 ownedCount 계산
            if (cardManager != null && scoreManager != null)
            {
                var combos = cardManager.GetCompletedCollectorCombos(scoreManager);
                foreach (var combo in combos)
                {
                    if (combo.collectorName == comboName)
                    {
                        ownedCount = combo.ownedCount;
                        break;
                    }
                }
            }

            string countInfo = "";
            if (totalCount > 0)
            {
                countInfo = $" ({ownedCount}/{totalCount})";
            }

            if (comboDesc != null)
                return $"{comboDesc}\n점수: {comboScore}{countInfo}";
            else
                return $"{info}\n점수: {comboScore}{countInfo}";
        }

        // 2. 스테이지별 info
        if (stageManager != null)
        {
            switch (stageManager.stageType)
            {
                case StageManager.StageType.Jooin: return string.IsNullOrEmpty(jooinInfo) ? info : jooinInfo;
                case StageManager.StageType.Beldir: return string.IsNullOrEmpty(beldirInfo) ? info : beldirInfo;
                case StageManager.StageType.Iana: return string.IsNullOrEmpty(ianaInfo) ? info : ianaInfo;
                case StageManager.StageType.Sitry: return string.IsNullOrEmpty(sitryInfo) ? info : sitryInfo;
                case StageManager.StageType.Noi: return string.IsNullOrEmpty(noiInfo) ? info : noiInfo;
                case StageManager.StageType.Limi: return string.IsNullOrEmpty(limiInfo) ? info : limiInfo;
                case StageManager.StageType.Raz: return string.IsNullOrEmpty(razInfo) ? info : razInfo;
                case StageManager.StageType.Churos: return string.IsNullOrEmpty(churosInfo) ? info : churosInfo;
                case StageManager.StageType.Chami: return string.IsNullOrEmpty(chamiInfo) ? info : chamiInfo;
                case StageManager.StageType.Donddatge: return string.IsNullOrEmpty(donddatgeInfo) ? info : donddatgeInfo;
                case StageManager.StageType.Muddung: return string.IsNullOrEmpty(muddungInfo) ? info : muddungInfo;
                case StageManager.StageType.Rasky: return string.IsNullOrEmpty(raskyInfo) ? info : raskyInfo;
                case StageManager.StageType.Roze: return string.IsNullOrEmpty(rozeInfo) ? info : rozeInfo;
                case StageManager.StageType.Yasu: return string.IsNullOrEmpty(yasuInfo) ? info : yasuInfo;
            }
        }

        // 3. 기본 info
        return info;
    }
}