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
    public Sprite defaultStageBackground; // 기본 배경 Sprite
    public GameObject backGround;

    private string[] prevComboKeys = new string[4]; // 각 슬롯별 이전 콤보 키 저장

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
        if (rateText != null) rateText.text = $"X {rate}";
        if (scoreYetText != null) scoreYetText.text = $"{scoreYet}";
        if (resultCheckScore != null) resultCheckScore.text = $"{resultScore}";
    }


    public void UpdateBackgroundUI()
    {
        if (backGround != null && defaultStageBackground != null)
        {
            var sr = backGround.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = defaultStageBackground;
                Debug.Log("[UIManager] BackGround 오브젝트의 스프라이트가 기본 이미지로 초기화되었습니다.");
            }
            else
            {
                Debug.LogWarning("[UIManager] BackGround 오브젝트에 SpriteRenderer가 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("[UIManager] 배경 초기화 실패: BackGround 오브젝트 또는 Sprite가 null입니다.");
        }
    }

    public void ChangeStageBackground(Sprite newBackground)
    {
        if (backGround != null && newBackground != null)
        {
            var sr = backGround.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = newBackground;
                Debug.Log("[UIManager] BackGround 오브젝트의 스프라이트가 변경되었습니다.");
            }
            else
            {
                Debug.LogWarning("[UIManager] BackGround 오브젝트에 SpriteRenderer가 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("[UIManager] 배경 이미지 변경 실패: BackGround 오브젝트 또는 Sprite가 null입니다.");
        }
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
        // 1. 모든 이미지 비활성화
        for (int i = 0; i < collectorResultImages.Length; i++)
        {
            if (collectorResultImages[i] != null)
                collectorResultImages[i].enabled = false;
        }

        // 2. 콤보가 없으면 prevComboKeys를 null로 초기화하고 종료 (SFX 재생 X)
        if (combos == null || combos.Count == 0)
        {
            for (int i = 0; i < prevComboKeys.Length; i++)
                prevComboKeys[i] = null;
            return;
        }

        HashSet<string> setNames = new HashSet<string>
    {
        "Vlup", "CheerUp!", "ColdSleep", "Daystar", "Innovill", "LoveLetter", "Mea", "SpringInnovill"
    };

        int normalIdx = 0;
        string[] newComboKeys = new string[prevComboKeys.Length];

        // 3. 콤보별로 imageIdx에 맞는 콤보 키를 newComboKeys에 저장
        foreach (var c in combos)
        {
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
                string comboKey = $"{c.collectorName}_{c.ownedCount}";
                newComboKeys[imageIdx] = comboKey;
            }
        }

        // 4. imageIdx별로 prevComboKeys와 newComboKeys가 다르면 SFX
        bool playSfx = false;
        for (int i = 0; i < prevComboKeys.Length; i++)
        {
            if (prevComboKeys[i] != newComboKeys[i])
            {
                playSfx = true;
                break;
            }
        }

        // 5. 이미지 적용 및 Tooltip 처리
        normalIdx = 0;
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
                imageIdx = 3;
            }
            else
            {
                imageIdx = normalIdx;
                normalIdx++;
                if (imageIdx == 3)
                    continue;
            }

            if (imageIdx < collectorResultImages.Length && collectorResultImages[imageIdx] != null)
            {
                if (resultSprite != null)
                {
                    collectorResultImages[imageIdx].sprite = resultSprite;
                    collectorResultImages[imageIdx].enabled = true;

                    var tooltip = collectorResultImages[imageIdx].GetComponent<UIHoverTooltip>();
                    if (tooltip != null)
                    {
                        tooltip.comboName = c.collectorName;
                        tooltip.info = c.collectorName;
                    }
                    Debug.Log($"[UIManager] Sprite 적용 성공: {resourcePath} (슬롯 {imageIdx})");
                }
                else
                {
                    collectorResultImages[imageIdx].enabled = false;
                    Debug.LogWarning($"[UIManager] Sprite를 찾을 수 없음: {resourcePath} (슬롯 {imageIdx})");
                }
            }
        }

        // 6. prevComboKeys 갱신
        for (int i = 0; i < prevComboKeys.Length; i++)
            prevComboKeys[i] = newComboKeys[i];

        // 7. SFX 재생(콤보가 있을 때만, 다를 때만)
        if (playSfx)
        {
            var gm = FindObjectOfType<GameMaster>();
            if (gm != null && gm.musicManager != null)
                gm.musicManager.PlayComboCompleteSFX();
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
            case "Vlup": return "Vlup";
            case "Vfes1": return "Vfes1";
            case "Vfes2": return "Vfes2";
            case "CheerUp": return "CheerUp";
            case "ColdSleep": return "ColdSleep";
            case "Daystar": return "Daystar";
            case "Innovill": return "Innovill";
            case "LoveLetter": return "LoveLetter";
            case "Mea": return "Mea";
            case "Dantalk": return "Dantalk";
            case "SpringInnovill": return "SpringInnovill";

            default: return "Default";
        }
    }
}