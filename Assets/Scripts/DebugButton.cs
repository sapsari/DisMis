using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        if (target == null)
        {
            All();
            return;
        }

        ClickAux(target);
    }

    private void ClickAux(Disinformer target)
    {
        if (target.IsUnrevealed)
            return;

        //--gameMngr.unreveal(target.unreveal);
        target.IsUnrevealed = true;

        target.button.gameObject.SetActive(false);

        gameMngr.Unlink(target);
    }

    void All()
    {
        var targets = FindObjectsOfType<DebugButton>().Select(b => b.target).Where(b => b != null);

        foreach (var target in targets.Where(t => t.IsSpawner))
            ClickAux(target);

        foreach (var target in targets.Where(t => !t.IsSpawner && !t.IsRoot))
            ClickAux(target);

        foreach (var target in targets.Where(t => t.IsRoot))
            ClickAux(target);
    }
}
