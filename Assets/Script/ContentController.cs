using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContentController : MonoBehaviour
{
    public Image image;
    public TMP_Text titleText, contentText;
    public Button quizz, close;


    public void Init(Sprite img, string title, string content, PathType path, string partId)
    {
        image.sprite = img;
        titleText.text = title;
        contentText.text = content;
        close.onClick.RemoveAllListeners();
        close.onClick.AddListener(() => {
            this.gameObject.SetActive(false);
            UIController.instance.joystick.gameObject.SetActive(true);
        });  
        quizz.onClick.RemoveAllListeners();
        quizz.onClick.AddListener(() => quizzAction(path.ToString(), partId, title));
    }
    private void quizzAction(string path, string partId, string text)
    {
        
        UIController.instance.quizUIController.gameObject.SetActive(true);
        UIController.instance.quizUIController.Init(partId, path, text);
    }
}
