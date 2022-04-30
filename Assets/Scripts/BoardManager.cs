using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public RenderTexture tex;

    public TMPro.TMP_Text topleft;
    //public 
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //ReadTexture();
    }

    void ReadTexture()
    {
        var w = tex.width;
        var h = tex.height;
        Texture2D tex2d = new Texture2D(w, h);

        RenderTexture.active = tex;
        tex2d.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex2d.Apply();
        RenderTexture.active = null;

        Color[] colors = tex2d.GetPixels();

        var green = 0;
        var red = 0;
        var blue = 0;
        foreach (var color in colors)
        {
            if(color.r > 0)red++;
            if(color.g > 0)green++;
            if(color.b > 0)blue++;
        }

        //Debug.Log($"RGB: ({red}, {green}, {blue})");
        topleft.text = $"RGB: ({red}, {green}, {blue})";
    }
}
