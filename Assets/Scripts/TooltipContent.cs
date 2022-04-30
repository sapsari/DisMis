using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipContent : MonoBehaviour
{
    public string Content;

    Tooltip tooltip;
    GameMngr gameMngr;

    private void Start()
    {
        tooltip = FindObjectOfType<Tooltip>();
        gameMngr = FindObjectOfType<GameMngr>();
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

    public void Enter()
    {
        var content = Content;

        if (content == "%PUBLIC%")
            content = "Public opinion is " + gameMngr.scoreDisMis.ToString() + 
                " vs " + gameMngr.scoreWell.ToString();

        tooltip.Show(content);
    }

    public void Exit()
    {
        tooltip.Hide();
    }
}
