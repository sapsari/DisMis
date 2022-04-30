using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class DropTile : MonoBehaviour
{
    Collider2D myCollider;
    internal SpriteRenderer sprite;

    public bool IsDropped { get; private set; }


    internal Vector2Int pos;

    DropTable table;

    public State state;

    public TMPro.TMP_Text text;

    Vector3 targetDisMis, targetWell;

    GameMngr gameMngr;

    // Start is called before the first frame update
    void Start()
    {
        myCollider = GetComponent<Collider2D>();
        sprite = GetComponent<SpriteRenderer>();

        //child.GetComponent<CircleCollider2D>().enabled = true;
        myCollider.enabled = true;

        table = FindObjectOfType<DropTable>();

        targetDisMis = GameObject.Find("DisMisTarget").transform.position;
        targetWell = GameObject.Find("WellTarget").transform.position;

        gameMngr = FindObjectOfType<GameMngr>();

        return;

        var color = sprite.color;
        color.a = 0.5f;
        sprite.color = color;
    }

    IEnumerator blink;
    IEnumerator Blink()
    {
        Color color;

        while (true)
        {
            color = sprite.color;
            color.a = 0.1f;
            sprite.color = color;

            Debug.Log("Blink .1f");

            yield return new WaitForSeconds(.4f);

            color = sprite.color;
            color.a = 0.5f;
            sprite.color = color;

            Debug.Log("Blink .5f");

            yield return new WaitForSeconds(.6f);
        }
    }

    public void StartBlink()
    {
        return;

        Debug.Log("Blink start");

        blink = Blink();
        StartCoroutine(blink);
    }

    public void StopBlink()
    {
        return;

        if (IsDropped) return;

        Debug.Log("Blink stop");

        StopCoroutine(blink);

        var color = sprite.color;
        color.a = 0.5f;
        sprite.color = color;

        Debug.Log("Blink stop .5f");
    }

    void OnDroppedAux(DragTile dragged, Vector3 posOffset)
    {
        // dont let misinfo override disinfo
        if (state.Type == State.StateType.Disinformed && dragged.state.Type == State.StateType.Misinformed)
            return;

        sprite.color = dragged.color;
        //text.color = dragged.color;

        state.Type = dragged.state.Type;
        state.Score = dragged.state.Score;

        dragged.transform.Translate(posOffset.x * -1, posOffset.y * -1, 0);
    }

    public bool OnDropped(DragTile dragged)
    {
        Debug.Log("Dropped");

        //return;

        StopBlink();



        /*
        var color = sprite.color;
        color.a = 1f;
        sprite.color = color;
        */


        foreach (Transform sibling in dragged.transform.parent)
        {
            var curDrag = sibling.GetComponent<DragTile>();
            //            if (curDrag == dragged)
            //              continue;

            var offset = curDrag.transform.position - dragged.transform.position;

            //table.tiles[, Mathf.RoundToInt(offset.x) + pos.y].OnDroppedAux(curDrag);

            var x = Mathf.RoundToInt(offset.y) + pos.x;
            var y = Mathf.RoundToInt(offset.x) + pos.y;

            if (x < 0 || x >= table.tiles.GetLength(0))
                return false;
            if (y < 0 || y >= table.tiles.GetLength(1))
                return false;
        }



        //sprite.color = dragged.color;
        //text.color = dragged.color;

        // align here, if not, just set it to vec3.zero
        var posOffset = dragged.transform.position - this.transform.position;
        var draggedPos = dragged.transform.position;

        foreach (Transform sibling in dragged.transform.parent)
        {
            var curDrag = sibling.GetComponent<DragTile>();
            //            if (curDrag == dragged)
            //              continue;

            var offset = curDrag.transform.position - draggedPos;

            Debug.Log("offset:" + offset);
            Debug.Log("pos:" + pos);
            table.tiles[Mathf.RoundToInt(offset.y) + pos.x, Mathf.RoundToInt(offset.x) + pos.y].OnDroppedAux(curDrag, posOffset);
        }

        Debug.Log("Dropped 1f");

        //**--IsDropped = true;

        //**--

        //**--Quiz.Current.OnDropped(dragged, this);

        return true;
    }

    public void ShowScore() => StartCoroutine(ShowScoreAux());

    IEnumerator ShowScoreAux()
    {
        if(state.Type == State.StateType.Clueless)
            yield break;

        var pos = text.transform.position;
        pos.z = -10;
        text.transform.position = pos;

        text.text = state.Score > 0 ? "+" + state.Score.ToString() : state.Score.ToString();

        var target = state.Type == State.StateType.Wellinformed ? targetWell : targetDisMis;
        target.z = -10;

        const float punchDuration = 1f;
        text.transform.DOPunchScale(Vector3.one * 1.3f, punchDuration, 1);

        yield return new WaitForSeconds(punchDuration);

        text.transform.DOMove(target, 2f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                text.transform.localPosition = Vector3.zero;
                text.transform.rotation = Quaternion.identity;

                gameMngr.updateBar = true;
                gameMngr.scores.Enqueue(state.Score);
            })
            ;

        yield break;
    }

}

