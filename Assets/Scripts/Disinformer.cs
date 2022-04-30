using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Disinformer : MonoBehaviour
{
    public GameObject disinfoPrefab;
    public GameObject misinfoPrefab;

    DropTable dropTable;

    public UnityEngine.UI.Button button;

    public Vector2Int unreveal;

    public bool IsSpawner;

    public bool IsUnrevealed;
    public bool IsRoot;

    Tooltip tooltip;
    public string name;

    internal bool silenced;

    /*
    public GameObject curInfo;
    public DragTile curDrag;
    public DropTile curDest;
    */

    // Start is called before the first frame update
    void Start()
    {
        dropTable = FindObjectOfType<DropTable>();

        if (button != null && !IsRoot)
            button.GetComponentInChildren<TMPro.TMP_Text>().text = unreveal.y + "x" + unreveal.x;

        tooltip = FindObjectOfType<Tooltip>();
    }

    /*
    bool isFirst = true;
    // Update is called once per frame
    void Update()
    {
        if (isFirst)
        {
            isFirst = false;
            
            Create();
            Create();
        }
    }
    */

    public const float moveDuration = 1.5f;
    public void Create(bool isDisInfo = false)
    {
        var prefab = isDisInfo ? disinfoPrefab : misinfoPrefab;
        var curInfo = Instantiate(prefab, transform);
        curInfo.transform.localPosition = Vector3.zero;
        curInfo.SetActive(true);
        var curDrag = curInfo.GetComponentInChildren<DragTile>();
        curDrag.state.Score = Random.Range(1, 3) * -1;
        if (curDrag.state.Type == State.StateType.Disinformed)
            curDrag.state.Score = Random.Range(3, 6) * -1;

        // enable only one child (select a random image)
        var tr = curDrag.transform;
        var activeChildIndex = Random.Range(0, tr.childCount);
        for(int i = 0; i < tr.childCount; i++)
        {
            if (i != activeChildIndex)
                tr.GetChild(i).gameObject.SetActive(false);
        }


        var x = Random.Range(0, dropTable.width);
        var y = Random.Range(0, dropTable.height);

        var destination = dropTable.tiles[x, y];
        var curDest = destination;


        curInfo.transform
            .DOMove(destination.transform.position, moveDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => OnTweenComplete(curDrag, curDest, curInfo))
            ;
    }

    void OnTweenComplete(DragTile curDrag, DropTile curDest, GameObject curInfo)
    {
        curDest.OnDropped(curDrag);

        Destroy(curInfo);

        /*
        curInfo = null;
        curDrag = null;
        curDest = null;
        */
    }

    
    private void OnMouseEnter() 
    {
        if (!GetComponentInChildren<SpriteRenderer>().enabled)
            return;


        var text = name;

        if (button != null && !button.gameObject.activeSelf && !IsUnrevealed)
            text = "???";

        tooltip.Show(text);
        //tooltip.Show(Name + " " + FindObjectOfType<GameMngr>().HasDep(this));
    }

    private void OnMouseExit()
    {
        if (!GetComponentInChildren<SpriteRenderer>().enabled)
            return;

        tooltip.Hide();
    }

}
