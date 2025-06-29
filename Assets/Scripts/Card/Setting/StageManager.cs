using System.Collections.Generic;
using TMPro;
using System.Linq;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public Dictionary<string, int> stageScoreTable = new Dictionary<string, int>()
    {
    {"1-1", 300}, {"1-2", 500}, {"2-1", 800}, {"2-2", 1000},
    {"3-1", 1500}, {"3-2", 2000}, {"4-1", 3000}, {"4-2", 3500},
    {"5-1", 4000}, {"5-2", 4500}, {"6-1", 5500}, {"6-2", 7000},
    {"7-1", 7500}, {"7-2", 8500},
    {"����", 100} // �� ���� �������� �߰�
    };


    public enum StageType { Jooin, Beldir, Iana, Sitry, Noi, Limi, Raz, Churos, Chami, Donddatge, Muddung, Rasky, Roze, Yasu, Tutorial } // Tutorial �߰�
    public StageType stageType;

    private List<string> stageOrder;
    private int currentStageIndex;

    public TextMeshProUGUI stageText; // ���� �������� UI

    public GameObject clearObject; // �ν����Ϳ��� ���
    public GameObject clearButton; // �ν����Ϳ��� ���
    public GameObject restartButton; // �ν����Ϳ��� ���
    public GameObject tutorialexitButton; // �ν����Ϳ��� ���

    public TextMeshProUGUI clearText; // ���� �������� UI

    // --- StageManagerWrapper���� ������ �ʵ� ---
    public SpriteListSO stageImageListSO;
    public UnityEngine.UI.Image stageUIImage;
    // -----------------------------------------
    // �ʿ��� �������� �ν����Ϳ��� �Ҵ�
    public GameObject rozeCardPrefab;     // ���� ī�� ������ (Noi)
    public GameObject backCardPrefab;     // �޸� ī�� ������ (Raz)

    public void ApplyStageEffect(
        ScoreManager scoreManager,
        CardManager cardManager,
        BuffManager buffManager,
        ref int targetScore,
        ref int coin)
    {

        cardManager.InitStageVariables();
        scoreManager.InitStageFlags();
        switch (stageType)
        {
            case StageType.Jooin:
                Debug.Log("���� �������� ȿ�� ����");
                break;

            case StageType.Beldir:
                // Set�� check ī�� 1���� Coin -1 (ScoreManager���� ó�� �ʿ�)
                // ����� �÷��׸� ����, ���� ������ ScoreManager����
                scoreManager.isBeldirStage = true;
                break;

            case StageType.Iana:
                cardManager.isIanaStage = true;
                break;

            case StageType.Sitry:
                // ���� ���� �� ĳ���͸� ����� (ScoreManager ��� ó�� �ʿ�)
                scoreManager.isSitryStage = true;
                break;

            case StageType.Noi:
                cardManager.isNoiStage = true;
                break;

            case StageType.Limi:
                // Drop�� ī�带 ������ (Drop �� ��ο� X)
                cardManager.isLimiStage = true;
                break;

            case StageType.Raz:
                // ��ο� �� ī�带 Ȯ�������� ������
                cardManager.isRazStage = true;
                break;
        }
    }

    public void SetTutorialStage()
    {
        stageType = StageType.Tutorial;
        UpdateStageImage();
    }

    public void InitializeTutorial()
    {
        stageOrder = new List<string> { "����" };
        currentStageIndex = 0;
        UpdateStageUI();
        UpdateStageImage();
        if (clearObject != null) clearObject.SetActive(false);
        if (clearButton != null) clearButton.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);
    }

    private static readonly StageType[] X1Types = {
        StageType.Churos, StageType.Chami, StageType.Donddatge, StageType.Muddung,
        StageType.Rasky, StageType.Roze, StageType.Yasu
    };

    // x-1 Ÿ�Ժ� x-2 ����
    private static readonly Dictionary<StageType, StageType> X1ToX2Map = new Dictionary<StageType, StageType>
    {
        { StageType.Churos, StageType.Jooin },
        { StageType.Chami, StageType.Sitry },
        { StageType.Donddatge, StageType.Limi },
        { StageType.Muddung, StageType.Iana },
        { StageType.Rasky, StageType.Raz },
        { StageType.Roze, StageType.Noi },
        { StageType.Yasu, StageType.Beldir }
    };

    private HashSet<StageType> clearedTypes = new HashSet<StageType>();

    /// <summary>
    /// x-1, x-2 Ÿ���� �������� ���� (Ŭ������ Ÿ���� ����)
    /// </summary>
    public (StageType x1, StageType x2)? DecideStageTypeAndUpdateImage()
    {
        string currentStage = GetCurrentStageName();
        bool isX1Stage = currentStage.EndsWith("-1");

        if (isX1Stage)
        {
            // x-1 ��������: x-1 Ÿ�Ը� ���� ����
            var availableX1 = X1Types.Where(t => !clearedTypes.Contains(t)).ToList();
            if (availableX1.Count == 0)
                return null;

            var x1 = availableX1[Random.Range(0, availableX1.Count)];
            var x2 = X1ToX2Map[x1];
            stageType = x1;
            lastX1Type = x1;
            UpdateStageImage();
            return (x1, x2);
        }
        else
        {
            // x-2 ��������: x-1���� �̹� ������ Ÿ���� ���ΰ��� �ٷ� ���
            if (lastX1Type.HasValue)
            {
                var x1 = lastX1Type.Value;
                var x2 = X1ToX2Map[x1];
                stageType = x2;
                UpdateStageImage();
                return (x1, x2);
            }
            else
            {
                // ���� ó��: ���� x-1 Ÿ�� ������ ������ ���� �Ұ�
                return null;
            }
        }
    }

    // x-1 ������������ ���õ� Ÿ���� ������ �ʵ� �߰�
    private StageType? lastX1Type = null;

    // x-1 ������������ Ÿ���� ������ �� ����
    public void SetLastX1Type(StageType x1)
    {
        lastX1Type = x1;
    }

    public void Initialize()
    {
        if (currentStageIndex != 0)
        {
            Debug.LogWarning("StageManager�� �̹� �ʱ�ȭ�Ǿ����ϴ�. �ٽ� �ʱ�ȭ���� �ʽ��ϴ�.");
        }
        else
        {
            // ���� �������� ���� ����
            var originalOrder = new List<string>(stageScoreTable.Keys);
            stageOrder = new List<string>();

            // �� �������� �ڿ� Shop �������� ����
            for (int i = 0; i < originalOrder.Count; i++)
            {
                stageOrder.Add(originalOrder[i]);
                // ������ ���������� �ƴϸ� Shop ����
                if (i < originalOrder.Count - 1)
                    stageOrder.Add($"Shop-{originalOrder[i]}");
            }
            currentStageIndex = 0;
        }
        UpdateStageUI();
        UpdateStageImage();
        clearObject.SetActive(false);
        clearButton.SetActive(false);
        restartButton.SetActive(false);


    }

    public void OnClickClear()
    {
        clearObject.SetActive(false);
        Debug.Log("Ŭ���� ��ư Ŭ��: Ŭ���� ������Ʈ ��Ȱ��ȭ");
    }

    public bool IsShopStage()
    {
        return GetCurrentStageName().StartsWith("Shop-");
    }

    public void GoToNextStage()
    {
        currentStageIndex++;
        UpdateStageUI();
        UpdateStageImage();
    }

    public void UpdateStageUI()
    {
        string currentStage = GetCurrentStageName();
        if (stageText != null)
        {
            if (currentStage.StartsWith("Shop-"))
                stageText.text = "Shop";
            else
                stageText.text = $"Stage : {currentStage}";
        }
    }

    public void MarkStageTypeCleared(StageType type)
    {
        clearedTypes.Add(type);
    }

    public bool IsLastStage()
    {
        return currentStageIndex >= stageOrder.Count - 1;
    }
    /*
    public bool CheckStageClear(int currentScore, out string message)
    {
        string currentStage = stageOrder[currentStageIndex];
        int requiredScore = stageScoreTable[currentStage];
        if (stageType == StageType.Jooin)
        {
            Debug.Log("���Τ����� �������� ȿ�� ����");
            requiredScore = (int)(requiredScore * 3.3f);
        }

        if (currentScore >= requiredScore)
        {
            message = $"�������� {currentStage} Ŭ����!";
            currentStageIndex++;

            // 7-2 Ŭ���� �� Clear ������Ʈ Ȱ��ȭ �� �ؽ�Ʈ ���
            if (currentStage == "7-2")
            {
                if (clearObject != null)
                    clearObject.SetActive(true);
                if (clearText != null)
                    clearText.text = "��� �������� Ŭ����!";
            }

            if (currentStageIndex >= stageOrder.Count)
            {
                message = "��� �������� Ŭ����!";
                return true; // ������ �������� Ŭ����
            }

            UpdateStageUI();
            UpdateStageImage();
            return true;
        }
        else
        {
            message = $"�������� {currentStage} ����! ��ǥ ����: {requiredScore}";
            return false;
        }
    }
    */
    public bool CheckStageResult(int currentScore, int setCount, out string message)
    {
        string currentStage = stageOrder[currentStageIndex];
        tutorialexitButton.SetActive(false);
        restartButton.SetActive(false);
        clearButton.SetActive(false);

        // Ʃ�丮�� ���������� ��ųʸ� ���� ���� ���� ó��
        if (currentStage == "����" || stageType == StageType.Tutorial)
        {
            int requiredScore = 100;
            if (currentScore >= requiredScore)
            {
                if (clearObject != null)
                    clearObject.SetActive(true);
                if (clearText != null)
                    clearText.text = "Ʃ�丮�� Ŭ����!.";
                if (tutorialexitButton != null)
                    tutorialexitButton.SetActive(true);
                // �߰� ���� �ʿ�� ���⿡ �ۼ�
                message = "Ʃ�丮�� Ŭ����!";
                return true;
            }
            else if (setCount == 0 && currentScore < requiredScore) // ���� requiredScore��!
            {
                if (clearObject != null)
                    clearObject.SetActive(true);
                if (clearText != null)
                    clearText.text = "Ʃ�丮�� ����! ��ǥ ����: 100";
                if (restartButton != null)
                    restartButton.SetActive(true);

                message = "Ʃ�丮�� ����!";
                return false;
            }
        }

        // ���� �Ϲ� �������� ó��
        int required = stageScoreTable[currentStage];
        if (stageType == StageType.Jooin)
            required = (int)(required * 3.3f);

        if (setCount == 0 && currentScore < required)
        {
            if (clearObject != null)
                clearObject.SetActive(true);
            if (clearText != null)
                clearText.text = "�������� : " + currentStage + " ����!  ��ǥ ���� : " + required;
            message = $"�������� {currentStage} ����!";
            if (restartButton != null)
                restartButton.SetActive(true);

            return false;
        }

        if (currentScore >= required)
        {
            if (clearObject != null)
                clearObject.SetActive(true);
            if (clearText != null)
                clearText.text = "�������� Ŭ����!";

            message = $"�������� {currentStage} Ŭ����!";
            if (clearButton != null)
                clearButton.SetActive(true);
            currentStageIndex++;

            if (currentStage == "7-2" || currentStageIndex >= stageOrder.Count)
            {
                message = "��� �������� Ŭ����!";
                if (clearObject != null)
                    clearObject.SetActive(true);
                if (clearText != null)
                    clearText.text = "��� �������� Ŭ����!";
                if (restartButton != null)
                    restartButton.SetActive(true);
            }

            UpdateStageUI();
            UpdateStageImage();
            return true;
        }

        message = "�� �� ���� ����";
        return false;
    }
    public string GetCurrentStageName()
    {
        if (stageOrder == null || stageOrder.Count == 0)
        {
            Debug.LogError("stageOrder�� �ʱ�ȭ���� �ʾҽ��ϴ�. Initialize()�� ���� ȣ���ؾ� �մϴ�.");
            return "";
        }
        return stageOrder[currentStageIndex];
    }

    // --- StageManagerWrapper���� ������ �޼��� ---
    public void UpdateStageImage()
    {
        // �̹��� �̸�: "Stage" + stageType (��: StageYasu)
        string imageName = $"Stage{stageType}";
        string resourcePath = $"Image/Stage/{imageName}";
        Sprite sprite = Resources.Load<Sprite>(resourcePath);

        if (stageUIImage != null)
        {
            stageUIImage.sprite = sprite;
            stageUIImage.enabled = sprite != null;
        }

        Debug.Log($"�������� �̹��� ����: {resourcePath} {(sprite != null ? "����" : "����")}");
    }
    // --------------------------------------------

    public int GetCurrentStageIndex()
    {
        return currentStageIndex;
    }

    public int GetTargetScore()
    {
        string currentStage = stageOrder[currentStageIndex];

        if (stageOrder == null || stageOrder.Count == 0)
            return 0;

        if (currentStage == "����" || stageType == StageType.Tutorial)
            return 100; // Ʃ�丮���� 100�� ����
    
        if (stageScoreTable.TryGetValue(currentStage, out int targetScore))
        {
            // Jooin ���������� 3.3�� ��ȯ
            if (stageType == StageType.Jooin)
                return (int)(targetScore * 3.3f);
            else
                return targetScore;
        }
        else
            return 0;
    }


}