using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using UnityEngine.PlayerLoop;

/*
public class DragTileChild : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        transform.parent.GetComponent<DragTile>().OnTriggerEnter2D(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {

    }
}
*/

public class DragTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static DragTile DraggedInstance;

    Vector3 startPos;
    Vector3 offset;

    int pointerId;
    float zDistanceToCamera;

    bool IsUI;

    DropTile colliding;

    public UnityEngine.UI.Image ButtonImage;

    internal Color color;

    public State state;

    GameMngr gameMngr;

    public string Name;

    private void Start()
    {
        /*
        foreach (Transform child in transform)
        {
            child.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            child.GetComponent<CircleCollider2D>().enabled = true;
        }*/

        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        //GetComponent<CircleCollider2D>().enabled = true;
        GetComponent<BoxCollider2D>().enabled = true;

        // make it appear in front of other shapes
        transform.Translate(new Vector3(0, 0, -1));

        collidings = new List<Transform>();

        color = GetComponent<SpriteRenderer>().color;

        gameMngr = FindObjectOfType<GameMngr>();

        Debug.Log("dragtile start");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin Drag");

        gameMngr.tooltip.Hide();

        pointerId = eventData.pointerId;

        DraggedInstance = this;
        startPos = transform.position;

        //ButtonImage.enabled = false;

        if (IsUI)
            offset = startPos - new Vector3(eventData.position.x, eventData.position.y);
        else
        {
            zDistanceToCamera = Mathf.Abs(startPos.z - Camera.main.transform.position.z);

            offset = startPos - Camera.main.ScreenToWorldPoint(
                new Vector3(eventData.position.x, eventData.position.y, zDistanceToCamera)
            );
        }

        //**--Quiz.Current.OnDragStart(this);
    }

    //void SetPos(Vector3 pos)
    Vector3 Pos
    {
        set
        {
            //transform.position = value;
            var dif = value - transform.position;
            transform.parent.transform.Translate(dif);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Input.touchCount > 1)
            return;

        if (eventData.pointerId != pointerId)
        {
            // Bu parmak, takip ettiğimiz parmak değil. Fonksiyondan direkt çık (OnDrag'i çalıştırma)
            return;
        }


        if (IsUI)
            Pos = new Vector3(eventData.position.x, eventData.position.y)
                + offset;
        else
            Pos = Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, zDistanceToCamera)
                ) + offset;


        /*
        Vector2 pointerKonum = eventData.position;
        // Objeyi parmağın olduğu konuma, kameradan 5 metre uzakta olacak şekilde ışınla
        //obje.position = kamera.ScreenToWorldPoint(new Vector3(pointerKonum.x, pointerKonum.y, 5f));
        transform.position = Camera.main.ScreenToWorldPoint(new Vector3(pointerKonum.x, pointerKonum.y, 5f));
        */
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != pointerId)
        {
            // Bu parmak, takip ettiğimiz parmak değil. Fonksiyondan direkt çık (OnDrag'i çalıştırma)
            return;
        }
        pointerId = -1;

        DraggedInstance = null;
        offset = Vector3.zero;

        //ButtonImage.enabled = true;


        var dropped = collidings.Any();
        /*
        if (colliding != null)
            colliding.OnDropped();
            */
        if (dropped)
        {
            var closest = collidings.MinBy(c => (c.position - transform.position).sqrMagnitude).First();
            dropped = closest.GetComponent<DropTile>().OnDropped(this);
        }

        if (dropped)
            Destroy(this.transform.parent.gameObject);//**--

        if (!dropped)
            // reset pos
            Pos = startPos;


        //**--if (!dropped)
        //**--Quiz.Current.OnDragEndWithoutDrop(this);
    }

    //#if UPDATE

    Transform lastClosest;
    void Update()
    {


        //foreach (var c in collidings)
        //    Debug.DrawLine(c.position, c.position + Vector3.one, Color.red);

        var closest = collidings.MinBy(c => (c.position - transform.position).sqrMagnitude).FirstOrDefault();

        if (closest == lastClosest)
        {
            // nop
        }
        else
        {
            if (lastClosest != null)
                lastClosest.GetComponent<DropTile>().StopBlink();
            if (closest != null)
                closest.GetComponent<DropTile>().StartBlink();
        }
        lastClosest = closest;



        //if (collidings.Any())
        {
            //closest = collidings.MinBy(c => (c.position - transform.position).sqrMagnitude).First();
            //Debug.DrawLine(closest.position, closest.position + Vector3.one * 2, Color.red);

            //closest.


        }


        //Debug.DrawLine(Vector3.zero, Vector3.one * 100, Color.red);
        //Debug.DrawLine(transform.position + Vector3.zero, transform.position + Vector3.one * 100, Color.red);
    }
    //#endif // update


    //int collidingIndex;
    //Transform[] collidings = new Transform[5];
    List<Transform> collidings;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("TRIGGER ENTER 1");
        colliding = collision.gameObject.GetComponent<DropTile>();
        if (colliding == null) return;
        if (colliding.IsDropped) return;
        var asd = collision.gameObject;
        //Debug.Log("self:" + gameObject);
        //Debug.Log(collision);
        //Debug.Log(asd);
        //Debug.Log(asd.name);
        //if (colliding == null) return;
        collidings.Add(asd.transform);
        //Debug.Log(asd.transform);
        //Debug.Log("TRIGGER ENTER 2");

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        colliding = null;
        collidings.Remove(collision.transform);
        //Debug.Log("TRIGGER EXIT");
    }


    private void OnMouseEnter()
    {
        if (DraggedInstance != null)
            return;

        gameMngr.tooltip.Show(Name);
    }

    private void OnMouseExit()
    {
        gameMngr.tooltip.Hide();
    }

}
