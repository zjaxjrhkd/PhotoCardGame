using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Text 컴포넌트용
using TMPro; // TextMeshProUGUI용

public class TutorialSystem : MonoBehaviour
{
    [TextArea] public string[] info;
    public GameObject tutorialButton;
    public GameObject tutorialText;

    private int currentIndex = 1;

    // 버튼에서 이 메서드를 OnClick에 연결하세요.
    public void OnclickTutorial()
    {
        tutorialButton.SetActive(true);

    }

    public void OnclickText()
    {
        if (info == null || info.Length == 0) return;

        if (currentIndex < info.Length - 1)
        {
            currentIndex++;
            SetTutorialText();
        }
        else
        {
            currentIndex = 1;
            SetTutorialText();
            // 마지막 텍스트에서 버튼 비활성화
            tutorialButton.SetActive(false);
        }
    }

    private void SetTutorialText()
    {
        if (tutorialText == null) return;

        // TextMeshProUGUI 우선, 없으면 Text
        var tmp = tutorialText.GetComponent<TMPro.TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = info[currentIndex];
            return;
        }
        var txt = tutorialText.GetComponent<UnityEngine.UI.Text>();
        if (txt != null)
        {
            txt.text = info[currentIndex];
        }
    }

    private void Start()
    {
        currentIndex = 0;
        SetTutorialText();
    }
}