using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameMngr : MonoBehaviour
{
    Disinformer[] disinformers;
    Wellinformer[] wellinformers;
    Link[] links;

    DropTable dropTable;
    internal Popup popup;

    public UnityEngine.UI.Button endTurnButton;

    internal bool updateBar;
    internal System.Collections.Concurrent.ConcurrentQueue<int> scores 
        = new System.Collections.Concurrent.ConcurrentQueue<int>();

    internal int scoreWell = 100;
    internal int scoreDisMis = -100;

    public TMPro.TMP_Text textBarLeft, textBarRight;
    public UnityEngine.UI.RawImage imageBarBottom, imageBarTop;

    public TMPro.TMP_Text textDebug;

    internal Tooltip tooltip;

    internal bool IsOver;

    // Start is called before the first frame update
    void Start()
    {
        disinformers = FindObjectsOfType<Disinformer>();
        wellinformers = FindObjectsOfType<Wellinformer>();
        links = FindObjectsOfType<Link>();

        dropTable = FindObjectOfType<DropTable>();
        popup = FindObjectOfType<Popup>();
        tooltip = FindObjectOfType<Tooltip>();

        endTurnButton.interactable = false;
        foreach (var disinformer in disinformers)
        {
            if (disinformer.button != null)
                disinformer.button.interactable = false;
        }

        HideGraph();
    }

    bool isFirst = true;
    bool isFirstEndTurn = true;
    int roundNo;

    bool isFirstAvailableUnreveal = true;

    // Update is called once per frame
    void Update()
    {
        if (isFirst)
        {
            isFirst = false;

            //StartRound();
            popup.Display("Let's fight against\ndisinformation", true, () => StartRound());
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            //EndRound();
            //StartRound();
            StartCoroutine(EndAndStartRound());
        }

        if(isRoundStarted && !IsOver && wellinformers.All(i => !i.HasDrag))
        {
            endTurnButton.interactable = true;

            if(isFirstEndTurn)
            {
                isFirstEndTurn = false;

                popup.Display("End turn by clicking\nright bottom button", false);
            }
        }

        if(isRoundStarted)
        {
            foreach (var disinformer in disinformers)
            {
                var unreveal = getUnreveal(disinformer.unreveal);

                if (disinformer.button != null)
                { 
                    disinformer.button.interactable = disinformer.IsRoot || unreveal.HasValue;

                    if (unreveal.HasValue && isFirstAvailableUnreveal)
                    {
                        isFirstAvailableUnreveal = false;

                        var buttonText = disinformer.button.GetComponentInChildren<TMPro.TMP_Text>().text;
                        popup.Display("Find out more!\nClick button of " + buttonText+ " at the left", false);
                    }
                }
            }
        }

        if(updateBar)
        {
            updateBar = false;

            while(scores.TryDequeue(out int score))
            {
                if (score > 0)
                    scoreWell += score;
                else
                    scoreDisMis += score;
            }

            var scoreDMP = scoreDisMis * -1;
            int left = scoreDMP * 100 / (scoreDMP + scoreWell);
            int right = 100 - left;

            textBarLeft.text = "%" + left.ToString();
            textBarRight.text = "%" + right.ToString();

            var barWidth = imageBarBottom.GetComponent<RectTransform>().sizeDelta.x;
            var leftWidth = barWidth * left / 100;
            var barTopRect = imageBarTop.GetComponent<RectTransform>();
            var sd = barTopRect.sizeDelta;
            sd.x = leftWidth;
            barTopRect.sizeDelta = sd;

            textDebug.text = $"{scoreDisMis}/{scoreWell}";
        }
    }

    IEnumerator EndAndStartRound()
    {
        endTurnButton.interactable = false;
        foreach (var disinformer in disinformers)
        {
            if (disinformer.button != null)
                disinformer.button.interactable = false;
        }

        EndRound();
        yield return new WaitForSeconds(3f);
        StartRound();
        yield break;
    }

    void StartRound() => StartCoroutine(StartRoundAux());

    bool isRoundStarted;
    IEnumerator StartRoundAux()
    {
        roundNo++;

        foreach (var disinformer in disinformers)
        {
            if (!disinformer.IsSpawner)
                continue;

            if (disinformer.silenced)
                continue;

            var isDisInfo = Random.Range(0f, 1f) > .8f;
            disinformer.Create(isDisInfo);

            if (Random.Range(0f, 1f) > .2f)
                disinformer.Create();

            yield return new WaitForSeconds(.2f);
        }

        yield return new WaitForSeconds(Disinformer.moveDuration);

        // to make sure every info type is unique
        var creationIndices = new List<int>();

        foreach (var wellinformer in wellinformers)
        {
            wellinformer.Create(creationIndices);
            yield return new WaitForSeconds(.2f);
        }

        isRoundStarted = true;

        if (roundNo == 1)
            popup.Display("Drag & drop correct info\nfrom right side to the board", false);

        if (roundNo == 4)
            popup.Display("Try making big continuous\nareas, like 4x4 rectangle");

        yield break;
    }

    void EndRound() => StartCoroutine(EndRoundAux());

    IEnumerator EndRoundAux()
    {
        isRoundStarted = false;

        for (int x = 0; x < dropTable.width; x++)
        {
            for (int y = 0; y < dropTable.height; y++)
            {
                dropTable.tiles[x, y].ShowScore();
            }
        }
        yield break;
    }

    public void OnEndTurn()
    {
        popup.HideIf("End");

        // there is another popup about sth else (probably disinform buttons)
        // so disable ending turn for now
        if (popup.HasOtherThan("End"))
            return;

        StartCoroutine(EndAndStartRound());
    }

    bool canBeUnrevealed(int x, int y, Vector2Int size)
    {
        for (int sx = x; sx < x + size.x; sx++)
        {
            for (var sy = y; sy < y + size.y; sy++)
            {
                if (dropTable.tiles[sx, sy].state.Type != State.StateType.Wellinformed)
                    return false;
            }
        }
        return true;
    }

    Vector2Int? getUnreveal(Vector2Int size)
    {
        for (int x = 0; x <= dropTable.width - size.x; x++)
        {
            for (int y = 0; y <= dropTable.height - size.y; y++)
            {
                if (canBeUnrevealed(x, y, size))
                    return new Vector2Int(x, y);
            }
        }

        return null;
    }

    public void unreveal(Vector2Int size)
    {
        var coords = getUnreveal(size).Value;

        for (int x = coords.x; x < coords.x + size.x; x++)
        {
            for (int y = coords.y; y < coords.y + size.y; y++)
            {
                dropTable.tiles[x, y].state.Type = State.StateType.Clueless;
                dropTable.tiles[x, y].state.Score = 0;
                dropTable.tiles[x, y].GetComponent<SpriteRenderer>().color = Color.black;//**--
            }
        }
    }

    public void onUnrevealButton(UnityEngine.UI.Button button)
    {
        popup.HideIf("Find");

        //var curButton = UnityEngine. EventSystem.current.currentSelectedGameObject;
        var dis = disinformers.FirstOrDefault(d => d.button == button);

        if (dis.IsUnrevealed)
            return;

        if (!dis.IsRoot)
            unreveal(dis.unreveal);
        dis.IsUnrevealed = true;

        button.gameObject.SetActive(false);

        Unlink(dis);

        if (dis.IsRoot)
            Silence(dis);
    }

    void HideGraph()
    {
        foreach (var link in links)
            link.GetComponent<SpriteRenderer>().enabled = false;

        foreach (var dis in disinformers)
        {
            if (!dis.IsSpawner)
            {
                dis.GetComponentInChildren<SpriteRenderer>().enabled = false;
                dis.button?.gameObject.SetActive(false);
            }
        }

    }

    public void Unlink(Disinformer disParam)
    {
        foreach (var link in links)
        {
            if (link.right == disParam && !link.silenced)
            {
                link.GetComponent<SpriteRenderer>().enabled = true;
            }
        }

        foreach(var dis in disinformers)
        {
            if (dis.IsSpawner)
                continue;

            var totalLink = links.Count(l => l.left == dis);
            var activeLink = links.Count(l => l.left == dis && l.GetComponent<SpriteRenderer>().enabled);

            if (activeLink == 0)
                continue;

            if (dis.silenced)
                continue;

            dis.GetComponentInChildren<SpriteRenderer>().enabled = true;

            if (dis.IsUnrevealed)
                continue;

            if (activeLink < totalLink)
            {
                dis.GetComponentInChildren<SpriteRenderer>().color = Color.black;
                dis.button?.gameObject.SetActive(false);
            }
            else
            {
                dis.GetComponentInChildren<SpriteRenderer>().color = Color.white;
                dis.button?.gameObject.SetActive(true);
            }

        }
    }

    void Silence(Disinformer dis) => StartCoroutine(SilenceAux(dis));

    IEnumerator SilenceAux(Disinformer dis)
    {
        var toSilence = new List<GameObject>();

        AddToList(dis);

        foreach (var link in links)
        {
            if (link.silenced)
                continue;

            if (link.left.silenced && !toSilence.Contains(link.gameObject))
            {
                toSilence.Add(link.gameObject);
                link.silenced = true;
            }
        }

        foreach(var td in toSilence)
        {
            var sr = td.GetComponent<SpriteRenderer>();
            if (sr == null)
                sr = td.GetComponentInChildren<SpriteRenderer>();
            sr.enabled = false;

            yield return new WaitForSeconds(.2f);
        }

        if (disinformers.All(d => d.silenced))
        {
            IsOver = true;
            StartCoroutine(EndEffect1_Board());
            StartCoroutine(EndEffect2_RemoveDrags());
        }

        yield break;

        void AddToList(Disinformer cur)
        {
            toSilence.Add(cur.gameObject);
            cur.silenced = true;

            foreach (var link in links.Where(l => l.left == cur))
            {
                var incomplete = links.Any(l => l.right == link.right && HasDependency(l.left));

                if(!incomplete)
                {
                    toSilence.Add(link.gameObject);
                    link.silenced = true;
                    AddToList(link.right);
                }
            }

            bool HasDependency(Disinformer d)
            {
                if (!d.IsUnrevealed)
                    return true;

                foreach(var link in links.Where(l => l.right == d))
                {
                    if (HasDependency(link.left))
                        return true;
                }

                return false;
            }
        }
    }

    public bool HasDep(Disinformer d)
    {
        if (!d.IsUnrevealed)
            return true;

        foreach (var link in links.Where(l => l.right == d))
        {
            if (HasDep(link.left))
                return true;
        }

        return false;
    }

    IEnumerator EndEffect1_Board()
    {
        var drops = new List<DropTile>();

        for (var x = 0; x < dropTable.width; x++)
        {
            for (int y = 0; y < dropTable.height; y++)
            {
                var drop = dropTable.tiles[x, y];

                if (drop.state.Type != State.StateType.Wellinformed)
                    drops.Add(drop);
            }
        }

        Shuffle(drops);

        ColorUtility.TryParseHtmlString("#98DC53", out var color);

        foreach(var drop in drops)
        {
            drop.sprite.color = color;

            drop.state.Type = State.StateType.Wellinformed;

            drop.state.Score = 1;

            yield return new WaitForSeconds(Random.Range(0, .1f));
        }

        popup.Display("Congratulations!!!\nGame is over", func: () => Application.Quit());

        yield break;
    }

    public static void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }


    IEnumerator EndEffect2_RemoveDrags()
    {
        foreach(var wi in wellinformers)
        {
            if(wi.HasDrag)
            {
                // Destroy(this.transform.parent.gameObject);//**--
                Destroy(wi.curDrag.transform.parent.gameObject);
                yield return new WaitForSeconds(.3f);
            }    
        }
    }

}
