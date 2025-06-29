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
    public Sprite defaultStageBackground; // �⺻ ��� Sprite
    public GameObject backGround;

    private string[] prevComboKeys = new string[4]; // �� ���Ժ� ���� �޺� Ű ����

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
                Debug.Log("[UIManager] BackGround ������Ʈ�� ��������Ʈ�� �⺻ �̹����� �ʱ�ȭ�Ǿ����ϴ�.");
            }
            else
            {
                Debug.LogWarning("[UIManager] BackGround ������Ʈ�� SpriteRenderer�� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogWarning("[UIManager] ��� �ʱ�ȭ ����: BackGround ������Ʈ �Ǵ� Sprite�� null�Դϴ�.");
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
                Debug.Log("[UIManager] BackGround ������Ʈ�� ��������Ʈ�� ����Ǿ����ϴ�.");
            }
            else
            {
                Debug.LogWarning("[UIManager] BackGround ������Ʈ�� SpriteRenderer�� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogWarning("[UIManager] ��� �̹��� ���� ����: BackGround ������Ʈ �Ǵ� Sprite�� null�Դϴ�.");
        }
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
        // 1. ��� �̹��� ��Ȱ��ȭ
        for (int i = 0; i < collectorResultImages.Length; i++)
        {
            if (collectorResultImages[i] != null)
                collectorResultImages[i].enabled = false;
        }

        // 2. �޺��� ������ prevComboKeys�� null�� �ʱ�ȭ�ϰ� ���� (SFX ��� X)
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

        // 3. �޺����� imageIdx�� �´� �޺� Ű�� newComboKeys�� ����
        foreach (var c in combos)
        {
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
                string comboKey = $"{c.collectorName}_{c.ownedCount}";
                newComboKeys[imageIdx] = comboKey;
            }
        }

        // 4. imageIdx���� prevComboKeys�� newComboKeys�� �ٸ��� SFX
        bool playSfx = false;
        for (int i = 0; i < prevComboKeys.Length; i++)
        {
            if (prevComboKeys[i] != newComboKeys[i])
            {
                playSfx = true;
                break;
            }
        }

        // 5. �̹��� ���� �� Tooltip ó��
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
                    Debug.Log($"[UIManager] Sprite ���� ����: {resourcePath} (���� {imageIdx})");
                }
                else
                {
                    collectorResultImages[imageIdx].enabled = false;
                    Debug.LogWarning($"[UIManager] Sprite�� ã�� �� ����: {resourcePath} (���� {imageIdx})");
                }
            }
        }

        // 6. prevComboKeys ����
        for (int i = 0; i < prevComboKeys.Length; i++)
            prevComboKeys[i] = newComboKeys[i];

        // 7. SFX ���(�޺��� ���� ����, �ٸ� ����)
        if (playSfx)
        {
            var gm = FindObjectOfType<GameMaster>();
            if (gm != null && gm.musicManager != null)
                gm.musicManager.PlayComboCompleteSFX();
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