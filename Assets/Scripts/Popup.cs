using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup : MonoBehaviour
{
    TMPro.TMP_Text text;
    UnityEngine.UI.Image image;

    System.Action func;
    bool clickClose;


    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<UnityEngine.UI.Image>();
        text = GetComponentInChildren<TMPro.TMP_Text>();

        Hide();
    }

    public void Display(string content, bool clickClose = true, System.Action func = null)
    {
        text.text = content;
        this.func = func;
        this.clickClose = clickClose;

        image.enabled = true;
        text.enabled = true;
    }

    public void Hide()
    {
        image.enabled=false;
        text.enabled=false;
    }

    public void OnClick()
    {
        if (!clickClose)
            return;

        Hide();
        func?.Invoke();
    }

    public void HideIf(string contentStartingWith)
    {
        if (!image.enabled)
            return;
        if (!text.text.StartsWith(contentStartingWith))
            return;

        Hide();
        func?.Invoke();
    }

    public bool HasOtherThan(string contentStartingWith)
    {
        if (!image.enabled)
            return false;
        if (!text.text.StartsWith(contentStartingWith))
            return false;

        return true;
    }
    
}
