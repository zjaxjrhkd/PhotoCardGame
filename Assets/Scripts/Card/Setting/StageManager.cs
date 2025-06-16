using System.Collections.Generic;
using TMPro;
using System.Linq;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    private Dictionary<string, int> stageScoreTable = new Dictionary<string, int>()
    {
        {"1-1", 300}, {"1-2", 500}, {"2-1", 800}, {"2-2", 1000},
        {"3-1", 1500}, {"3-2", 2000}, {"4-1", 3000}, {"4-2", 3500},
        {"5-1", 4000}, {"5-2", 4500}, {"6-1", 5500}, {"6-2", 7000},
        {"7-1", 7500}, {"7-2", 8500}, {"8-1", 9000}, {"8-2", 10000}
    };

    public enum StageType { Jooin, Beldir, Iana, Sitry, Noi, Limi, Raz, Churos, Chami, Donddatge, Muddung, Rasky, Roze, Yasu }
    public StageType stageType;

    private List<string> stageOrder;
    private int currentStageIndex;

    public TextMeshProUGUI stageText; // ���� �������� UI

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
                // ��ǥ ���� 10��
                targetScore *= 10;
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
            stageOrder = new List<string>(stageScoreTable.Keys);
            currentStageIndex = 0;
        }
        UpdateStageUI();
        UpdateStageImage();
    }

    public void MarkStageTypeCleared(StageType type)
    {
        clearedTypes.Add(type);
    }

    public bool IsLastStage()
    {
        return currentStageIndex >= stageOrder.Count - 1;
    }

    public bool CheckStageClear(int currentScore, out string message)
    {
        string currentStage = stageOrder[currentStageIndex];
        int requiredScore = stageScoreTable[currentStage];

        if (currentScore >= requiredScore)
        {
            message = $"�������� {currentStage} Ŭ����!";
            currentStageIndex++;

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

    public bool CheckStageResult(int currentScore, out string message)
    {
        string currentStage = stageOrder[currentStageIndex];
        int requiredScore = stageScoreTable[currentStage];

        if (currentScore >= requiredScore)
        {
            message = $"�������� {currentStage} Ŭ����!";
            currentStageIndex++;

            if (currentStageIndex >= stageOrder.Count)
            {
                message = "��� �������� Ŭ����!";
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

    public string GetCurrentStageName()
    {
        if (stageOrder == null || stageOrder.Count == 0)
        {
            Debug.LogError("stageOrder�� �ʱ�ȭ���� �ʾҽ��ϴ�. Initialize()�� ���� ȣ���ؾ� �մϴ�.");
            return "";
        }
        return stageOrder[currentStageIndex];
    }

    private void UpdateStageUI()
    {
        string currentStage = GetCurrentStageName();
        if (stageText != null)
        {
            stageText.text = $"Stage : {currentStage}";
        }
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
        if (stageOrder == null || stageOrder.Count == 0)
            return 0;

        string currentStage = stageOrder[currentStageIndex];
        if (stageScoreTable.TryGetValue(currentStage, out int targetScore))
            return targetScore;
        else
            return 0;
    }


}