using System.Collections.Generic;
using TMPro;
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

    private List<string> stageOrder;
    private int currentStageIndex;

    public TextMeshProUGUI stageText; // ���� �������� UI

    // --- StageManagerWrapper���� ������ �ʵ� ---
    public SpriteListSO stageImageListSO;
    public UnityEngine.UI.Image stageUIImage;
    // -----------------------------------------

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
        int stageIndex = GetCurrentStageIndex();

        if (stageImageListSO != null && stageImageListSO.sprites.Count > stageIndex)
        {
            stageUIImage.sprite = stageImageListSO.sprites[stageIndex];
            Debug.Log($"�������� �̹��� ����: {stageIndex}");
        }
        else
        {
            Debug.LogWarning("�������� �̹��� ����Ʈ�� �ش� �ε����� �����ϴ�.");
        }
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