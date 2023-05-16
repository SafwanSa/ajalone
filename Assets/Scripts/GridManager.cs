using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GridManager : MonoBehaviour
{
    [SerializeField] private int width, height;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Transform cam;
    private Dictionary<Vector2, Tile> tiles;
    public int player;
    private Tile selectedTile;

    private void Start()
    {
        this.player = 2;
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
            int x = 0;
            if (y <= 5)
                x = 1;
            else
            {

                x = 5 - (9 - y);
                numOfBalls += (x - 1);
            }
            // Cols
            for (; x <= numOfBalls; x++)
            {
                var spawnedTile = Instantiate(this.tilePrefab, new Vector3(x, y), Quaternion.identity);
                var radius = (float)spawnedTile.GetComponent<CircleCollider2D>().radius;
                var xOffSet = x - (radius * y);
                spawnedTile.transform.position = new Vector3(xOffSet * 1.05f, y * 1.05f);
                spawnedTile.name = $"Tile {y} {x}";
                spawnedTile.Init(x, y, this);

                // Save center tile pos
                if (xOffSet == 2.5f && y == 5)
                    center = spawnedTile.transform.position;

                this.tiles[new Vector2(x, y)] = spawnedTile;
            }
        }
        this.cam.position = new Vector3(center.x, center.y, -10f);
    }

    public void SelectTile(Tile tile)
    {
        if (tile.value == this.player && this.selectedTile == null)
        {
            this.selectedTile = tile;
            this.selectedTile.selected.SetActive(true);
        }
        else if (tile.value != this.player && this.AllowedPos(tile))
        {
            // Move
            var tempVal = tile.value;
            tile.value = this.selectedTile.value;
            this.selectedTile.value = tempVal;
            this.selectedTile.UpdateColor();
            tile.UpdateColor();
            this.UnSelectTile();
        }
        else
        {

        }
    }

    public void UnSelectTile()
    {
        if (this.selectedTile)
        {
            this.selectedTile.selected.SetActive(false);
            this.selectedTile = null;
        }
    }

    public bool IsSelected(Tile tile)
    {
        return this.selectedTile == tile;
    }

    public bool OnSelection()
    {
        return this.selectedTile != null;
    }

    public bool AllowedPos(Tile tile)
    {
        bool otherTile = this.selectedTile && this.selectedTile.value != tile.value;
        if (otherTile)
        {
            bool d1 = this.selectedTile.y - tile.y == 0 && this.selectedTile.x - tile.x == -1;
            bool d2 = this.selectedTile.y - tile.y == 1 && this.selectedTile.x - tile.x == 0;
            bool d3 = this.selectedTile.y - tile.y == 1 && this.selectedTile.x - tile.x == 1;
            bool d4 = this.selectedTile.y - tile.y == 0 && this.selectedTile.x - tile.x == 1;
            bool d5 = this.selectedTile.y - tile.y == -1 && this.selectedTile.x - tile.x == 0;
            bool d6 = this.selectedTile.y - tile.y == -1 && this.selectedTile.x - tile.x == -1;

            return d1 || d2 || d3 || d4 || d5 || d6;
        }
        return false;
    }
    // public Tile GetTile(Vector2 pos)
    // {
    //     if (this.tiles.TryGetValue(pos, out var tile))
    //     {
    //         return tile;
    //     }
    //     else
    //     {
    //         return null;
    //     }
    // }
}
