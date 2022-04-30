using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Wellinformer : MonoBehaviour
{
    public GameObject[] infoPrefabs;

    GameObject curInfo;
    internal DragTile curDrag;

    // Start is called before the first frame update
    void Start()
    {
        //Create();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Create(List<int> creationIndices)
    {
        var index = Random.Range(0, infoPrefabs.Length);

        while (creationIndices.Contains(index))
            index = Random.Range(0, infoPrefabs.Length);

        creationIndices.Add(index);

        var prefab = infoPrefabs[index];
        curInfo = Instantiate(prefab, transform);
        curInfo.transform.localPosition = Vector3.zero;
        curInfo.transform.localPosition = new Vector3(1, 0, 0);
        curInfo.SetActive(true);
        curDrag = curInfo.GetComponentInChildren<DragTile>();
    }

    public bool HasDrag => transform.childCount > 1;
}
