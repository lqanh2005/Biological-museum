// QuizUIController.cs
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizUIController : MonoBehaviour
{
    [Header("Left Panel (list = image + text, không bấm)")]
    public Transform leftListContent;       // bố trí Vertical Layout Group
    public LeftItem leftItemPrefab;       // prefab đơn giản: Image + TMP Text
    public TMP_Text leftHeader;             // “CÂU HỎI”
    public Button submitAllButton;          // NỘP BÀI (bật khi tất cả đã chọn)

    [Header("Right Panel (đã setup sẵn, không Vertical)")]
    public TMP_Text categoryText;           // ví dụ “Khái niệm”
    public TMP_Text questionTitle;          // “Câu 1: …”
    public OptionUI optA;
    public OptionUI optB;
    public OptionUI optC;
    public OptionUI optD;
    public Button btnPrev;
    public Button btnNext;
    public bool isSubmit = false;
    public Button close;
    // runtime
    List<Question> _qs;
    int _idx = 0;
    int[] _chosen;          // -1 nếu chưa chọn
    LeftItem[] _leftItems;


    public void Init(string path, string pardId, string type)
    {
        _qs = CsvLoader.LoadQuestionsFromResources(pardId,
                string.IsNullOrWhiteSpace(path) ? null : path);
        if (_qs == null || _qs.Count == 0) { Debug.LogError("No questions loaded"); return; }

        if (categoryText) categoryText.text = type;
        _chosen = new int[_qs.Count];
        for (int i = 0; i < _chosen.Length; i++) _chosen[i] = -1;

        BuildLeftList();
        BindQuestion(0);
        close.onClick.RemoveAllListeners();
        close.onClick.AddListener(() => {
            gameObject.SetActive(false);
            isSubmit = false;
            UIController.instance.joystick.gameObject.SetActive(true);
        });
        btnPrev.onClick.RemoveAllListeners();
        btnPrev.onClick.AddListener(() => BindQuestion(Mathf.Clamp(_idx - 1, 0, _qs.Count - 1)));
        btnNext.onClick.RemoveAllListeners();
        btnNext.onClick.AddListener(() => BindQuestion(Mathf.Clamp(_idx + 1, 0, _qs.Count - 1)));
        submitAllButton.onClick.AddListener(SubmitAll);
        UpdateSubmitState();
    }

    void BuildLeftList()
    {
        // clear cũ
        foreach (Transform c in leftListContent) Destroy(c.gameObject);
        _leftItems = new LeftItem[_qs.Count];

        if (leftHeader) leftHeader.text = "CÂU HỎI";
        for (int i = 0; i < _qs.Count; i++)
        {
            var go = Instantiate(leftItemPrefab, leftListContent);
            var leftItem = go.GetComponent<LeftItem>();
            leftItem.Init();

            // Tạo biến local để mỗi lambda capture đúng index
            int questionIndex = i;

            // Ưu tiên sử dụng questBtn từ LeftItem, nếu không có thì dùng Button component chính
            Button targetButton = null;
            if (leftItem.questBtn != null)
            {
                targetButton = leftItem.questBtn;
            }
            else
            {
                targetButton = go.GetComponent<Button>();
            }

            // Đảm bảo chỉ có một listener được thêm vào
            if (targetButton != null)
            {
                targetButton.onClick.RemoveAllListeners();
                targetButton.onClick.AddListener(() => BindQuestion(questionIndex));
            }

            var t = go.GetComponentInChildren<TMP_Text>();
            if (t) t.text = $"Câu {i + 1}";
            _leftItems[i] = go;
            // có thể thêm dấu tick nhỏ/đổi màu khi đã chọn:
            SetLeftItemAnsweredVisual(i, false);
        }
    }

    void BindQuestion(int index)
    {
        _idx = index;
        var q = _qs[index];

        if (questionTitle) questionTitle.text = $"Câu {index + 1}: {q.title}";

        optA.Bind("A", q.opts[0], 0, OnChoose);
        optB.Bind("B", q.opts[1], 1, OnChoose);
        optC.Bind("C", q.opts[2], 2, OnChoose);
        optD.Bind("D", q.opts[3], 3, OnChoose);

        // nếu đã submit thì highlight đúng/sai, nếu chưa thì chỉ tô lại lựa chọn
        if (isSubmit)
        {
            int correct = _qs[index].correct;
            int chosen = _chosen[index];
            if (chosen == correct)
                MarkCurrentCorrect();
            else
                MarkCurrentWrong(correct, chosen);
        }
        else
        {
            // nếu đã chọn trước đó thì tô lại
            var chosen = _chosen[index];
            if (chosen >= 0) SetSelectedVisual(chosen);
            else ClearSelectedVisual();
        }

        btnPrev.interactable = _idx > 0;
        btnNext.interactable = _idx < _qs.Count - 1;
    }

    void OnChoose(int choiceIndex)
    {
        _chosen[_idx] = choiceIndex;
        SetSelectedVisual(choiceIndex);
        SetLeftItemAnsweredVisual(_idx, true);
        UpdateSubmitState();
    }

    void SetSelectedVisual(int i)
    {
        optA.SetNormal(); optB.SetNormal(); optC.SetNormal(); optD.SetNormal();
        switch (i)
        {
            case 0: optA.SetSelected(); break;
            case 1: optB.SetSelected(); break;
            case 2: optC.SetSelected(); break;
            case 3: optD.SetSelected(); break;
        }
    }
    void ClearSelectedVisual() { optA.SetNormal(); optB.SetNormal(); optC.SetNormal(); optD.SetNormal(); }

    void SetLeftItemAnsweredVisual(int i, bool answered)
    {
        // ví dụ đổi alpha nền của image (vì item không phải button)
        var img = _leftItems[i].GetComponentInChildren<Image>();
        if (img) img.color = answered ? new Color(0.85f, 1f, 0.9f, 1f) : new Color(1f, 1f, 1f, 1f);
    }

    void SetLeftItemResultVisual(int i, bool isCorrect)
    {
        // Đổi màu item bên trái dựa trên kết quả đúng/sai
        var img = _leftItems[i].GetComponentInChildren<Image>();
        if (img)
        {
            if (isCorrect)
                img.color = new Color(0.5259024f, 1f, 0.5259024f, 1f); // màu xanh cho đúng
            else
                img.color = new Color(1f, 0.3830188f, 0.3830188f, 1f); // màu đỏ cho sai
        }
    }

    void UpdateSubmitState()
    {
        bool allAnswered = true;
        for (int i = 0; i < _chosen.Length; i++) if (_chosen[i] < 0) { allAnswered = false; break; }
        if (submitAllButton) submitAllButton.interactable = allAnswered;
    }

    void SubmitAll()
    {
        isSubmit = true;
        // Chỉ gọi được khi đã enable (tức là chọn hết)
        int correct = 0;
        for (int i = 0; i < _qs.Count; i++)
        {
            if (_chosen[i] == _qs[i].correct)
            {
                correct++;
                // Đổi màu xanh cho item đúng
                SetLeftItemResultVisual(i, true);
            }
            else
            {
                // Đổi màu đỏ cho item sai
                SetLeftItemResultVisual(i, false);
            }
        }

        Debug.Log($"Score: {correct}/{_qs.Count}");

        // Tô đúng/sai cho câu đang mở (tuỳ bạn muốn highlight global hay chỉ current)
        int c = _qs[_idx].correct;
        if (_chosen[_idx] == c)
            MarkCurrentCorrect();
        else
            MarkCurrentWrong(c, _chosen[_idx]);
    }

    void MarkCurrentCorrect()
    {
        optA.SetNormal(); optB.SetNormal(); optC.SetNormal(); optD.SetNormal();
        switch (_qs[_idx].correct)
        {
            case 0: optA.SetCorrect(); break;
            case 1: optB.SetCorrect(); break;
            case 2: optC.SetCorrect(); break;
            case 3: optD.SetCorrect(); break;
        }
    }
    void MarkCurrentWrong(int correct, int chosen)
    {
        optA.SetNormal(); optB.SetNormal(); optC.SetNormal(); optD.SetNormal();
        switch (correct)
        {
            case 0: optA.SetCorrect(); break;
            case 1: optB.SetCorrect(); break;
            case 2: optC.SetCorrect(); break;
            case 3: optD.SetCorrect(); break;
        }
        switch (chosen)
        {
            case 0: optA.SetWrong(); break;
            case 1: optB.SetWrong(); break;
            case 2: optC.SetWrong(); break;
            case 3: optD.SetWrong(); break;
        }
    }
}
