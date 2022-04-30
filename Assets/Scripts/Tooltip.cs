using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{

    TMPro.TMP_Text text;
    Image image;

    Canvas canvas;

    // Use this for initialization
    void Start()
    {
        this.text = this.GetComponentInChildren<TMPro.TMP_Text>();
        this.image = this.GetComponent<Image>();
        text.enabled = false;
        image.enabled = false;

        canvas = this.transform.parent.GetComponent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Show(string str)
    {

        var pos0 = Input.mousePosition;
        var pos1 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var pos2 = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        //var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        var pos = pos0;

        //var pos = canvas.ScreenToCanvasPosition(Input.mousePosition);

        pos.z = transform.position.z;
        transform.position = pos;
        //transform.po
        text.enabled = true;
        image.enabled = true;
        text.text = str;

        Debug.Log(pos.ToString());

        ((RectTransform)transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, text.preferredWidth + 10);
    }

    public void Hide()
    {
        text.enabled = false;
        image.enabled = false;
    }
}

public static class CanvasPositioningExtensions
{
    public static Vector3 WorldToCanvasPosition(this Canvas canvas, Vector3 worldPosition, Camera camera = null)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }
        var viewportPosition = camera.WorldToViewportPoint(worldPosition);
        return canvas.ViewportToCanvasPosition(viewportPosition);
    }

    public static Vector3 ScreenToCanvasPosition(this Canvas canvas, Vector3 screenPosition)
    {
        var viewportPosition = new Vector3(screenPosition.x / Screen.width,
                                           screenPosition.y / Screen.height,
                                           0);
        return canvas.ViewportToCanvasPosition(viewportPosition);
    }

    public static Vector3 ViewportToCanvasPosition(this Canvas canvas, Vector3 viewportPosition)
    {
        var centerBasedViewPortPosition = viewportPosition - new Vector3(0.5f, 0.5f, 0);
        var canvasRect = canvas.GetComponent<RectTransform>();
        var scale = canvasRect.sizeDelta;
        return Vector3.Scale(centerBasedViewPortPosition, scale);
    }
}
