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

    // �ݷ��� ��� UI (�ִ� 3������)
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
    /// �ݷ��� ���� ��� UI ���� (�ִ� 3������)
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
        // ��� �̹��� ��Ȱ��ȭ
        foreach (var img in collectorResultImages)
            if (img != null) img.enabled = false;
        if (combos == null || combos.Count == 0)
            return;
        /*
        // ��ü ���� ������ ���� ǥ��
        int totalScore = 0;
        foreach (var combo in combos)
            totalScore += combo.score;
        if (collectorCheckScore != null)
            collectorCheckScore.text = totalScore.ToString();
        */
        // ��Ʈ ���� �̸� ���
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
                imageIdx = 3; // ��Ʈ�� 4��° �̹���(�ε��� 3)�� ����
            }
            else
            {
                imageIdx = normalIdx;
                normalIdx++;
                if (imageIdx == 3) // 4��°�� ��Ʈ ����, �ݷ��ʹ� 3��������
                    continue;
            }

            if (imageIdx < collectorResultImages.Length && collectorResultImages[imageIdx] != null)
            {
                if (resultSprite != null)
                {
                    collectorResultImages[imageIdx].sprite = resultSprite;
                    collectorResultImages[imageIdx].enabled = true;
                    Debug.Log($"[UIManager] Sprite ���� ����: {resourcePath} (���� {imageIdx})");
                }
                else
                {
                    collectorResultImages[imageIdx].enabled = false;
                    Debug.LogWarning($"[UIManager] Sprite�� ã�� �� ����: {resourcePath} (���� {imageIdx})");
                }
            }
        }
    }

    // �ݷ��ͺ� ���ҽ� ����/�̹��� ���λ� ��ȯ (�ʿ�� Ȯ��)
    private string GetResourceFolder(string collectorName)
    {
        switch (collectorName)
        {
            case "�������ݷ���": return "Image/Result/1.Jooin";
            case "�����ݷ���": return "Image/Result/2.Beldir";
            case "�̾Ƴ��ݷ���": return "Image/Result/3.Iana";
            case "��Ʈ���ݷ���": return "Image/Result/4.Sitry";
            case "�����ݷ���": return "Image/Result/5.Noi";
            case "�����ݷ���": return "Image/Result/6.Limi";
            case "�����ݷ���": return "Image/Result/7.Raz";
            // ��Ʈ/Ư�� ���յ� �ʿ�� �߰�
            default: return "Image/Result/8.Concept";
        }
    }
    private string GetImagePrefix(string collectorName)
    {
        switch (collectorName)
        {
            case "�������ݷ���": return "Jooin";
            case "�����ݷ���": return "Beldir";
            case "�̾Ƴ��ݷ���": return "Iana";
            case "��Ʈ���ݷ���": return "Sitry";
            case "�����ݷ���": return "Noi";
            case "�����ݷ���": return "Limi";
            case "�����ݷ���": return "Raz";
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