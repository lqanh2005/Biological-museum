using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public static class CsvLoader
{
    // CSV có header: partId,title,optA,optB,optC,optD,correct
    public static List<Question> LoadQuestionsFromResources(string path = "quiz", string onlyPartId = null)
    {
        var txt = Resources.Load<TextAsset>(path);
        if (!txt) { Debug.LogError("CSV not found at Resources/" + path); return new List<Question>(); }

        var lines = txt.text.Replace("\r", "").Split('\n');
        var list = new List<Question>();
        if (lines.Length <= 1) return list;

        for (int i = 1; i < lines.Length; i++) // skip header
        {
            var line = lines[i].TrimEnd();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cols = ParseCsvLine(line);
            if (cols.Count < 7) continue;

            string partId = cols[0].Trim();
            if (!string.IsNullOrEmpty(onlyPartId) &&
                !partId.Equals(onlyPartId, System.StringComparison.OrdinalIgnoreCase)) continue;

            var q = new Question
            {
                partId = partId,
                title = cols[1],
                opts = new[] { cols[2], cols[3], cols[4], cols[5] },
                correct = Mathf.Clamp("ABCD".IndexOf(cols[6].Trim().ToUpperInvariant()[0]), 0, 3)
            };
            list.Add(q);
        }
        return list;
    }

    // parser CSV có hỗ trợ dấu phẩy trong ""
    static List<string> ParseCsvLine(string line)
    {
        var res = new List<string>();
        bool quoted = false;
        var cur = new System.Text.StringBuilder();
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '\"')
            {
                if (quoted && i + 1 < line.Length && line[i + 1] == '\"') { cur.Append('\"'); i++; }
                else quoted = !quoted;
            }
            else if (c == ',' && !quoted)
            {
                res.Add(cur.ToString());
                cur.Length = 0;
            }
            else cur.Append(c);
        }
        res.Add(cur.ToString());
        return res;
    }
}


[Serializable]
public class Question
{
    public string partId;
    public string title;
    public string[] opts = new string[4];
    public int correct; // 0..3
}

[Serializable]
public class QuizSet
{
    public string partId;               // dùng nếu bạn lọc theo bộ phận
    public List<Question> questions = new();
}