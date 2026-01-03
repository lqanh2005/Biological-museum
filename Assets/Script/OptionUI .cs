// OptionUI.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    public Button button;
    public Image background;
    public TMP_Text labelLetter; // “A/B/C/D”
    public TMP_Text labelText;   // nội dung đáp án

    [Header("Colors")]
    public Color normal = Color.white;
    public Color selected = new(0.88f, 0.97f, 1f, 1f);
    public Color correct = new(0.85f, 1f, 0.9f, 1f);
    public Color wrong = new(1f, 0.9f, 0.9f, 1f);

    public int index; // 0..3
    System.Action<int> onClick;

    public void Bind(string letter, string text, int idx, System.Action<int> onClicked)
    {
        labelLetter.text = letter;
        labelText.text = text;
        index = idx;
        onClick = onClicked;
        SetNormal();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(index));
    }

    public void SetNormal() { if (background) background.color = normal; }
    public void SetSelected() { if (background) background.color = selected; }
    public void SetCorrect() { if (background) background.color = correct; }
    public void SetWrong() { if (background) background.color = wrong; }
}
