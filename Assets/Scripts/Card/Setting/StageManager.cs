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
    {"연습", 100} // ← 연습 스테이지 추가
    };


    public enum StageType { Jooin, Beldir, Iana, Sitry, Noi, Limi, Raz, Churos, Chami, Donddatge, Muddung, Rasky, Roze, Yasu, Tutorial } // Tutorial 추가
    public StageType stageType;

    private List<string> stageOrder;
    private int currentStageIndex;

    public TextMeshProUGUI stageText; // 현재 스테이지 UI

    public GameObject clearObject; // 인스펙터에서 등록
    public GameObject clearButton; // 인스펙터에서 등록
    public GameObject restartButton; // 인스펙터에서 등록
    public GameObject tutorialexitButton; // 인스펙터에서 등록

    public TextMeshProUGUI clearText; // 현재 스테이지 UI

    // --- StageManagerWrapper에서 가져온 필드 ---
    public SpriteListSO stageImageListSO;
    public UnityEngine.UI.Image stageUIImage;
    // -----------------------------------------
    // 필요한 프리팹을 인스펙터에서 할당
    public GameObject rozeCardPrefab;     // 로제 카드 프리팹 (Noi)
    public GameObject backCardPrefab;     // 뒷면 카드 프리팹 (Raz)

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
                Debug.Log("주인 스테이지 효과 적용");
                break;

            case StageType.Beldir:
                // Set시 check 카드 1개당 Coin -1 (ScoreManager에서 처리 필요)
                // 여기는 플래그만 세팅, 실제 차감은 ScoreManager에서
                scoreManager.isBeldirStage = true;
                break;

            case StageType.Iana:
                cardManager.isIanaStage = true;
                break;

            case StageType.Sitry:
                // 가장 많은 한 캐릭터만 적용됨 (ScoreManager 등에서 처리 필요)
                scoreManager.isSitryStage = true;
                break;

            case StageType.Noi:
                cardManager.isNoiStage = true;
                break;

            case StageType.Limi:
                // Drop한 카드를 수집함 (Drop 후 드로우 X)
                cardManager.isLimiStage = true;
                break;

            case StageType.Raz:
                // 드로우 한 카드를 확률적으로 뒤집음
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
        stageOrder = new List<string> { "연습" };
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

    // x-1 타입별 x-2 매핑
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
    /// x-1, x-2 타입을 랜덤으로 결정 (클리어한 타입은 제외)
    /// </summary>
    public (StageType x1, StageType x2)? DecideStageTypeAndUpdateImage()
    {
        string currentStage = GetCurrentStageName();
        bool isX1Stage = currentStage.EndsWith("-1");

        if (isX1Stage)
        {
            // x-1 스테이지: x-1 타입만 랜덤 선택
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
            // x-2 스테이지: x-1에서 이미 결정된 타입의 매핑값을 바로 사용
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
                // 예외 처리: 이전 x-1 타입 정보가 없으면 선택 불가
                return null;
            }
        }
    }

    // x-1 스테이지에서 선택된 타입을 저장할 필드 추가
    private StageType? lastX1Type = null;

    // x-1 스테이지에서 타입을 선택할 때 저장
    public void SetLastX1Type(StageType x1)
    {
        lastX1Type = x1;
    }

    public void Initialize()
    {
        if (currentStageIndex != 0)
        {
            Debug.LogWarning("StageManager가 이미 초기화되었습니다. 다시 초기화하지 않습니다.");
        }
        else
        {
            // 기존 스테이지 순서 생성
            var originalOrder = new List<string>(stageScoreTable.Keys);
            stageOrder = new List<string>();

            // 각 스테이지 뒤에 Shop 스테이지 삽입
            for (int i = 0; i < originalOrder.Count; i++)
            {
                stageOrder.Add(originalOrder[i]);
                // 마지막 스테이지가 아니면 Shop 삽입
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
        Debug.Log("클리어 버튼 클릭: 클리어 오브젝트 비활성화");
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
            Debug.Log("주인ㅁㄴㅇ 스테이지 효과 적용");
            requiredScore = (int)(requiredScore * 3.3f);
        }

        if (currentScore >= requiredScore)
        {
            message = $"스테이지 {currentStage} 클리어!";
            currentStageIndex++;

            // 7-2 클리어 시 Clear 오브젝트 활성화 및 텍스트 출력
            if (currentStage == "7-2")
            {
                if (clearObject != null)
                    clearObject.SetActive(true);
                if (clearText != null)
                    clearText.text = "모든 스테이지 클리어!";
            }

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
    */
    public bool CheckStageResult(int currentScore, int setCount, out string message)
    {
        string currentStage = stageOrder[currentStageIndex];
        tutorialexitButton.SetActive(false);
        restartButton.SetActive(false);
        clearButton.SetActive(false);

        // 튜토리얼 스테이지는 딕셔너리 접근 없이 직접 처리
        if (currentStage == "연습" || stageType == StageType.Tutorial)
        {
            int requiredScore = 100;
            if (currentScore >= requiredScore)
            {
                if (clearObject != null)
                    clearObject.SetActive(true);
                if (clearText != null)
                    clearText.text = "튜토리얼 클리어!.";
                if (tutorialexitButton != null)
                    tutorialexitButton.SetActive(true);
                // 추가 동작 필요시 여기에 작성
                message = "튜토리얼 클리어!";
                return true;
            }
            else if (setCount == 0 && currentScore < requiredScore) // 여기 requiredScore로!
            {
                if (clearObject != null)
                    clearObject.SetActive(true);
                if (clearText != null)
                    clearText.text = "튜토리얼 실패! 목표 점수: 100";
                if (restartButton != null)
                    restartButton.SetActive(true);

                message = "튜토리얼 실패!";
                return false;
            }
        }

        // 기존 일반 스테이지 처리
        int required = stageScoreTable[currentStage];
        if (stageType == StageType.Jooin)
            required = (int)(required * 3.3f);

        if (setCount == 0 && currentScore < required)
        {
            if (clearObject != null)
                clearObject.SetActive(true);
            if (clearText != null)
                clearText.text = "스테이지 : " + currentStage + " 실패!  목표 점수 : " + required;
            message = $"스테이지 {currentStage} 실패!";
            if (restartButton != null)
                restartButton.SetActive(true);

            return false;
        }

        if (currentScore >= required)
        {
            if (clearObject != null)
                clearObject.SetActive(true);
            if (clearText != null)
                clearText.text = "스테이지 클리어!";

            message = $"스테이지 {currentStage} 클리어!";
            if (clearButton != null)
                clearButton.SetActive(true);
            currentStageIndex++;

            if (currentStage == "7-2" || currentStageIndex >= stageOrder.Count)
            {
                message = "모든 스테이지 클리어!";
                if (clearObject != null)
                    clearObject.SetActive(true);
                if (clearText != null)
                    clearText.text = "모든 스테이지 클리어!";
                if (restartButton != null)
                    restartButton.SetActive(true);
            }

            UpdateStageUI();
            UpdateStageImage();
            return true;
        }

        message = "알 수 없는 오류";
        return false;
    }
    public string GetCurrentStageName()
    {
        if (stageOrder == null || stageOrder.Count == 0)
        {
            Debug.LogError("stageOrder가 초기화되지 않았습니다. Initialize()를 먼저 호출해야 합니다.");
            return "";
        }
        return stageOrder[currentStageIndex];
    }

    // --- StageManagerWrapper에서 가져온 메서드 ---
    public void UpdateStageImage()
    {
        // 이미지 이름: "Stage" + stageType (예: StageYasu)
        string imageName = $"Stage{stageType}";
        string resourcePath = $"Image/Stage/{imageName}";
        Sprite sprite = Resources.Load<Sprite>(resourcePath);

        if (stageUIImage != null)
        {
            stageUIImage.sprite = sprite;
            stageUIImage.enabled = sprite != null;
        }

        Debug.Log($"스테이지 이미지 변경: {resourcePath} {(sprite != null ? "성공" : "실패")}");
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

        if (currentStage == "연습" || stageType == StageType.Tutorial)
            return 100; // 튜토리얼은 100점 고정
    
        if (stageScoreTable.TryGetValue(currentStage, out int targetScore))
        {
            // Jooin 스테이지면 3.3배 반환
            if (stageType == StageType.Jooin)
                return (int)(targetScore * 3.3f);
            else
                return targetScore;
        }
        else
            return 0;
    }


}