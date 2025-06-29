using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Text ������Ʈ��
using TMPro; // TextMeshProUGUI��

public class TutorialSystem : MonoBehaviour
{
    [TextArea] public string[] info;
    public GameObject tutorialButton;
    public GameObject tutorialText;

    private int currentIndex = 1;

    // ��ư���� �� �޼��带 OnClick�� �����ϼ���.
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
            // ������ �ؽ�Ʈ���� ��ư ��Ȱ��ȭ
            tutorialButton.SetActive(false);
        }
    }

    private void SetTutorialText()
    {
        if (tutorialText == null) return;

        // TextMeshProUGUI �켱, ������ Text
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