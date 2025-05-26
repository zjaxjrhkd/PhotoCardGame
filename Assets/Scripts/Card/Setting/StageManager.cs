using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    private Dictionary<string, int> stageScoreTable = new Dictionary<string, int>()
    {
        {"1-1", 100}, {"1-2", 200}, {"2-1", 300}, {"2-2", 400},
        {"3-1", 500}, {"3-2", 600}, {"4-1", 700}, {"4-2", 800},
        {"5-1", 900}, {"5-2", 1000}, {"6-1", 1100}, {"6-2", 1200},
        {"7-1", 1300}, {"7-2", 1400}, {"8-1", 1500}, {"8-2", 1600}
    };

    private List<string> stageOrder;
    private int currentStageIndex;

    public TextMeshProUGUI stageText; // ���� �������� UI

    public void Initialize()
    {
        stageOrder = new List<string>(stageScoreTable.Keys);
        currentStageIndex = 0;
        UpdateStageUI();
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

    public int GetCurrentStageIndex()
    {
        return currentStageIndex;
    }

}
