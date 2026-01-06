using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LeftItem : MonoBehaviour
{
    public Image background;
    public TMP_Text label;
    [HideInInspector] public int index;
    public Button questBtn;
    public Color selectColor;

    public  void Init()
    {
        questBtn.onClick.AddListener(Bind);
    }
    public void Bind()
    {

    }
}
