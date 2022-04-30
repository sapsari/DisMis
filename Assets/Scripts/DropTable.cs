using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropTable : MonoBehaviour
{
    internal DropTile[,] tiles;

    internal int width, height;

    // Start is called before the first frame update
    void Start()
    {
        tiles = new DropTile[transform.childCount, transform.GetChild(0).transform.childCount];

        width = tiles.GetLength(0);
        height = tiles.GetLength(1);

        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                tiles[x, y] = transform.GetChild(x).GetChild(y).GetComponent<DropTile>();
                tiles[x, y].pos = new Vector2Int(x, y);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CountScores()
    {
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                tiles[x, y] = transform.GetChild(x).GetChild(y).GetComponent<DropTile>();
                tiles[x, y].pos = new Vector2Int(x, y);
            }
        }
    }
}
