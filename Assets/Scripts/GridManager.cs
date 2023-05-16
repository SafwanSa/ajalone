using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width, height;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Transform cam;
    private Dictionary<Vector2, Tile> tiles;

    private void Start()
    {
        GenerateGrid();
    }


    private int GetNumOfBallsInRow(int row)
    {
        switch (row)
        {
            case 1:
            case 9:
                return 5;
            case 2:
            case 8:
                return 6;
            case 3:
            case 7:
                return 7;
            case 4:
            case 6:
                return 8;
            case 5:
                return 9;
            default:
                return -1;
        }
    }

    private void GenerateGrid()
    {
        this.tiles = new Dictionary<Vector2, Tile>();
        var centerNumOfTiles = 9;
        Vector3 center = Vector3.zero;
        // Rows
        for (int y = 1; y <= 9; y++)
        {
            int numOfBalls = this.GetNumOfBallsInRow(y);
            // Cols
            for (int x = 1; x <= numOfBalls; x++)
            {
                var spawnedTile = Instantiate(this.tilePrefab, new Vector3(x, y), Quaternion.identity);
                var radius = (float)spawnedTile.GetComponent<CircleCollider2D>().radius;
                var xOffSet = y < 5 ? x - (radius * y) : x - (radius * (5 - y % 5));
                spawnedTile.transform.position = new Vector3(xOffSet * 1.05f, y * 1.05f);
                spawnedTile.name = $"Tile {y} {x}";
                spawnedTile.Init(x, y);

                // Save center tile pos
                if (xOffSet == 2.5f && y == 5)
                    center = spawnedTile.transform.position;

                this.tiles[new Vector2(x, y)] = spawnedTile;
            }
        }
        this.cam.position = new Vector3(center.x, center.y, -10f);
    }


    public Tile GetTile(Vector2 pos)
    {
        if (this.tiles.TryGetValue(pos, out var tile))
        {
            return tile;
        }
        else
        {
            return null;
        }
    }
}
