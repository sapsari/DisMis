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

    public UnityEngine.UI.Button endTurnButton;

    internal bool updateBar;
    internal System.Collections.Concurrent.ConcurrentQueue<int> scores 
        = new System.Collections.Concurrent.ConcurrentQueue<int>();

    int scoreWell = 100;
    int scoreDisMis = -100;

    public TMPro.TMP_Text textBarLeft, textBarRight;
    public UnityEngine.UI.RawImage imageBarBottom, imageBarTop;

    public TMPro.TMP_Text textDebug;

    // Start is called before the first frame update
    void Start()
    {
        disinformers = FindObjectsOfType<Disinformer>();
        wellinformers = FindObjectsOfType<Wellinformer>();
        links = FindObjectsOfType<Link>();

        dropTable = FindObjectOfType<DropTable>();

        endTurnButton.interactable = false;
        foreach (var disinformer in disinformers)
        {
            if (disinformer.button != null)
                disinformer.button.interactable = false;
        }

        HideGraph();
    }

    bool isFirst = true;

    // Update is called once per frame
    void Update()
    {
        if (isFirst)
        {
            isFirst = false;

            StartRound();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            //EndRound();
            //StartRound();
            StartCoroutine(EndAndStartRound());
        }

        if(isRoundStarted && wellinformers.All(i => !i.HasDrag))
        {
            endTurnButton.interactable = true;
        }

        if(isRoundStarted)
        {
            foreach (var disinformer in disinformers)
            {
                var unreveal = getUnreveal(disinformer.unreveal);

                if (disinformer.button != null)
                    disinformer.button.interactable = disinformer.IsRoot || unreveal.HasValue;
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
        foreach (var disinformer in disinformers)
        {
            if (!disinformer.IsSpawner)
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
            if (link.right == disParam)
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

        foreach(var td in toSilence)
        {
            Destroy(td);
            yield return new WaitForSeconds(.2f);
        }

        yield break;

        void AddToList(Disinformer cur)
        {
            toSilence.Add(cur.gameObject);

            foreach (var link in links.Where(l => l.left == cur))
            {
                var incomplete = links.Any(l => l.right == link.right && HasDependency(l.left));

                if(!incomplete)
                {
                    toSilence.Add(link.gameObject);
                    AddToList(link.right);
                }
            }

            bool HasDependency(Disinformer d)
            {
                if (d.IsUnrevealed)
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
}
