using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipContent : MonoBehaviour
{
    public string Content;

    Tooltip tooltip;
    private void Start()
    {
        tooltip = FindObjectOfType<Tooltip>();
    }

    private void OnMouseEnter()
    {
        Debug.Log("onmouseenter");
        tooltip.Show(Content);
    }

    private void OnMouseExit()
    {
        tooltip.Hide();
    }

}
