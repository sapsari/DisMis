using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugButton : MonoBehaviour
{
    public Disinformer target;

    GameMngr gameMngr;

    // Start is called before the first frame update
    void Start()
    {
        gameMngr = FindObjectOfType<GameMngr>();
    }

    public void onButtonClick()
    {
        if (target.IsUnrevealed)
            return;

        //--gameMngr.unreveal(target.unreveal);
        target.IsUnrevealed = true;
        
        target.button.gameObject.SetActive(false);

        gameMngr.Unlink(target);
    }
}
