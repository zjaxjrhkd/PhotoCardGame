using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    //public TextMeshProUGUI resultText;
    public TextMeshProUGUI setCountText;
    public TextMeshProUGUI dropCountText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI deckCountText;
    public TextMeshProUGUI rateText;
    public TextMeshProUGUI scoreYetText;

    // 콜렉터 결과 UI (최대 3개까지)
    public TextMeshProUGUI resultCheckScore;

    public Image[] collectorResultImages;


    public void UpdateScoreUI(int score, int targetScore)
    {
        if (scoreText != null)
            scoreText.text = $"{score} / {targetScore}";
    }

    public void UpdateCountUI(int set, int maxSet, int drop, int maxDrop)
    {
        if (setCountText != null) setCountText.text = $"Set: {set}/{maxSet}";
        if (dropCountText != null) dropCountText.text = $"Drop: {drop}/{maxDrop}";
    }

    public void UpdateCoinUI(int coin)
    {
        if (coinText != null)
            coinText.text = $"Coin: {coin}";
    }

    public void UpdateDeckCountUI(int current, int total)
    {
        if (deckCountText != null)
            deckCountText.text = $"{current} / {total}";
    }
    public void UpdateScoreCalUI(float rate, int scoreYet, int resultScore)
    {
        if (rateText != null) rateText.text = $"{rate}";
        if (scoreYetText != null) scoreYetText.text = $"{scoreYet}";
        if (resultCheckScore != null) resultCheckScore.text = $"{resultScore}";
    }


    /// <summary>
    /// 콜렉터 조합 결과 UI 갱신 (최대 3개까지)
    /// </summary>
    /// 
    public void HideCollectorComboImages()
    {
        if (collectorResultImages == null) return;
        foreach (var img in collectorResultImages)
            if (img != null) img.enabled = false;
    }



    public void UpdateCollectorResultUI(List<CollectorComboResult> combos)
    {
        // 모든 이미지 비활성화
        foreach (var img in collectorResultImages)
            if (img != null) img.enabled = false;
        if (combos == null || combos.Count == 0)
            return;
        /*
        // 전체 조합 점수의 합을 표시
        int totalScore = 0;
        foreach (var combo in combos)
            totalScore += combo.score;
        if (collectorCheckScore != null)
            collectorCheckScore.text = totalScore.ToString();
        */
        // 세트 조합 이름 목록
        HashSet<string> setNames = new HashSet<string>
    {
        "Vlup!", "Vfes", "CheerUp!", "ColdSleep", "Daystar", "Innovill", "LoveLetter", "Mea"
    };

        int normalIdx = 0;
        foreach (var c in combos)
        {
            string resourceFolder = GetResourceFolder(c.collectorName);
            string imagePrefix = GetImagePrefix(c.collectorName);
            string imageName = $"{imagePrefix}_{c.ownedCount}";
            string resourcePath = $"{resourceFolder}/{imageName}";
            Sprite resultSprite = Resources.Load<Sprite>(resourcePath);

            int imageIdx;
            if (setNames.Contains(c.collectorName))
            {
                imageIdx = 3; // 세트는 4번째 이미지(인덱스 3)에 고정
            }
            else
            {
                imageIdx = normalIdx;
                normalIdx++;
                if (imageIdx == 3) // 4번째는 세트 전용, 콜렉터는 3개까지만
                    continue;
            }

            if (imageIdx < collectorResultImages.Length && collectorResultImages[imageIdx] != null)
            {
                if (resultSprite != null)
                {
                    collectorResultImages[imageIdx].sprite = resultSprite;
                    collectorResultImages[imageIdx].enabled = true;
                    Debug.Log($"[UIManager] Sprite 적용 성공: {resourcePath} (슬롯 {imageIdx})");
                }
                else
                {
                    collectorResultImages[imageIdx].enabled = false;
                    Debug.LogWarning($"[UIManager] Sprite를 찾을 수 없음: {resourcePath} (슬롯 {imageIdx})");
                }
            }
        }
    }

    // 콜렉터별 리소스 폴더/이미지 접두사 반환 (필요시 확장)
    private string GetResourceFolder(string collectorName)
    {
        switch (collectorName)
        {
            case "이주인콜렉터": return "Image/Result/1.Jooin";
            case "벨디르콜렉터": return "Image/Result/2.Beldir";
            case "이아나콜렉터": return "Image/Result/3.Iana";
            case "시트리콜렉터": return "Image/Result/4.Sitry";
            case "노이콜렉터": return "Image/Result/5.Noi";
            case "리미콜렉터": return "Image/Result/6.Limi";
            case "라즈콜렉터": return "Image/Result/7.Raz";
            // 세트/특수 조합도 필요시 추가
            default: return "Image/Result/8.Concept";
        }
    }
    private string GetImagePrefix(string collectorName)
    {
        switch (collectorName)
        {
            case "이주인콜렉터": return "Jooin";
            case "벨디르콜렉터": return "Beldir";
            case "이아나콜렉터": return "Iana";
            case "시트리콜렉터": return "Sitry";
            case "노이콜렉터": return "Noi";
            case "리미콜렉터": return "Limi";
            case "라즈콜렉터": return "Raz";
            case "Vlup": return "Vlup!";
            case "Vfes": return "Vfes";
            case "CheerUp": return "CheerUp";
            case "ColdSleep": return "ColdSleep";
            case "Daystar": return "Daystar";
            case "Innovill": return "Innovill";
            case "LoveLetter": return "LoveLetter";
            case "Mea": return "Mea";

            default: return "Default";
        }
    }
}