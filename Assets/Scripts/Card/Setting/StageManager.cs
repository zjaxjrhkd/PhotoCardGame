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

    public TextMeshProUGUI stageText; // 현재 스테이지 UI

    // --- StageManagerWrapper에서 가져온 필드 ---
    public SpriteListSO stageImageListSO;
    public UnityEngine.UI.Image stageUIImage;
    // -----------------------------------------

    public void Initialize()
    {
        if (currentStageIndex != 0)
        {
            Debug.LogWarning("StageManager가 이미 초기화되었습니다. 다시 초기화하지 않습니다.");
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
            message = $"스테이지 {currentStage} 클리어!";
            currentStageIndex++;

            if (currentStageIndex >= stageOrder.Count)
            {
                message = "모든 스테이지 클리어!";
                return true; // 마지막 스테이지 클리어
            }

            UpdateStageUI();
            UpdateStageImage();
            return true;
        }
        else
        {
            message = $"스테이지 {currentStage} 실패! 목표 점수: {requiredScore}";
            return false;
        }
    }

    public bool CheckStageResult(int currentScore, out string message)
    {
        string currentStage = stageOrder[currentStageIndex];
        int requiredScore = stageScoreTable[currentStage];

        if (currentScore >= requiredScore)
        {
            message = $"스테이지 {currentStage} 클리어!";
            currentStageIndex++;

            if (currentStageIndex >= stageOrder.Count)
            {
                message = "모든 스테이지 클리어!";
            }

            UpdateStageUI();
            UpdateStageImage();
            return true;
        }
        else
        {
            message = $"스테이지 {currentStage} 실패! 목표 점수: {requiredScore}";
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

    // --- StageManagerWrapper에서 가져온 메서드 ---
    public void UpdateStageImage()
    {
        int stageIndex = GetCurrentStageIndex();

        if (stageImageListSO != null && stageImageListSO.sprites.Count > stageIndex)
        {
            stageUIImage.sprite = stageImageListSO.sprites[stageIndex];
            Debug.Log($"스테이지 이미지 변경: {stageIndex}");
        }
        else
        {
            Debug.LogWarning("스테이지 이미지 리스트에 해당 인덱스가 없습니다.");
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