
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizUIController : MonoBehaviour
{
    [Header("Left Panel (list = image + text, không bấm)")]
    public Transform leftListContent;
    public LeftItem leftItemPrefab;
    public TMP_Text leftHeader;
    public Button submitAllButton;

    [Header("Right Panel (đã setup sẵn, không Vertical)")]
    public TMP_Text categoryText;
    public TMP_Text questionTitle;
    public OptionUI optA;
    public OptionUI optB;
    public OptionUI optC;
    public OptionUI optD;
    public Button btnPrev;
    public Button btnNext;
    public bool isSubmit = false;
    public Button close;

    public GameObject resultPanel;
    public GameObject panel;
    public TMP_Text txtScore, txtNumber;
    public Button btnRevise, btnReview;

    List<Question> _qs;
    int _idx = 0;
    int[] _chosen;
    LeftItem[] _leftItems;


    public void Init(string path, string pardId, string type)
    {
        // Load tất cả câu hỏi từ CSV (không lọc theo partId)
        var allQuestions = CsvLoader.LoadQuestionsFromResources(path, null);
        if (allQuestions == null || allQuestions.Count == 0) { Debug.LogError("No questions loaded"); return; }

        // Chọn ngẫu nhiên 20 câu hỏi (hoặc ít hơn nếu không đủ 20)
        int questionCount = Mathf.Min(20, allQuestions.Count);
        _qs = ShuffleAndTake(allQuestions, questionCount);

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
        btnReview.onClick.AddListener(() => {resultPanel.SetActive(false); panel.SetActive(false); });
        btnRevise.onClick.AddListener(() => {
            resultPanel.SetActive(false);
            panel.SetActive(false);
            this.gameObject.SetActive(false);
        });
        UpdateSubmitState();
    }

    void BuildLeftList()
    {

        foreach (Transform c in leftListContent) Destroy(c.gameObject);
        _leftItems = new LeftItem[_qs.Count];

        if (leftHeader) leftHeader.text = "CÂU HỎI";
        for (int i = 0; i < _qs.Count; i++)
        {
            var go = Instantiate(leftItemPrefab, leftListContent);
            var leftItem = go.GetComponent<LeftItem>();
            leftItem.Init();


            int questionIndex = i;


            Button targetButton = null;
            if (leftItem.questBtn != null)
            {
                targetButton = leftItem.questBtn;
            }
            else
            {
                targetButton = go.GetComponent<Button>();
            }


            if (targetButton != null)
            {
                targetButton.onClick.RemoveAllListeners();
                targetButton.onClick.AddListener(() => BindQuestion(questionIndex));
            }

            var t = go.GetComponentInChildren<TMP_Text>();
            if (t) t.text = $"Câu {i + 1}";
            _leftItems[i] = go;

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

        var img = _leftItems[i].GetComponentInChildren<Image>();
        if (img) img.color = answered ? new Color(0.85f, 1f, 0.9f, 1f) : new Color(1f, 1f, 1f, 1f);
    }

    void SetLeftItemResultVisual(int i, bool isCorrect)
    {

        var img = _leftItems[i].GetComponentInChildren<Image>();
        if (img)
        {
            if (isCorrect)
                img.color = new Color(0.5259024f, 1f, 0.5259024f, 1f);
            else
                img.color = new Color(1f, 0.3830188f, 0.3830188f, 1f);
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
        resultPanel.SetActive(true);
        panel.SetActive(true);
        int correct = 0;
        for (int i = 0; i < _qs.Count; i++)
        {
            if (_chosen[i] == _qs[i].correct)
            {
                correct++;

                SetLeftItemResultVisual(i, true);
            }
            else
            {

                SetLeftItemResultVisual(i, false);
            }
        }

        float totalScore = correct * 0.5f;
        float maxScore = _qs.Count * 0.5f;

        txtScore.text = (totalScore/maxScore).ToString();
        txtNumber.text = correct.ToString() + "/" + _qs.Count.ToString();


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

    // Hàm shuffle và lấy số lượng câu hỏi ngẫu nhiên
    List<Question> ShuffleAndTake(List<Question> source, int count)
    {
        var shuffled = new List<Question>(source);
        
        // Fisher-Yates shuffle
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }
        
        // Lấy số lượng cần thiết
        return shuffled.Take(count).ToList();
    }
}
