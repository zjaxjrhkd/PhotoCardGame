using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI setCountText;
    public TextMeshProUGUI dropCountText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI deckCountText; // 덱 장수 표시용 Text


    public void UpdateScoreUI(int score)
    {
        if (scoreText != null) scoreText.text = "Score : " + score.ToString();
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
}
