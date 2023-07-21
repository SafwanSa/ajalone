using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GridManager : MonoBehaviour
{
    [SerializeField] private int width, height;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Transform cam;
    [SerializeField] private Dictionary<Vector2, Tile> tiles;
    public int player;
    private Tile selectedTile;

    private void Start()
    {
        this.player = 1;
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

                this.tiles[new Vector2(y, x)] = spawnedTile;
            }
        }
        this.cam.position = new Vector3(center.x, center.y, -10f);
    }

    public void SelectTile(Tile tile)
    {
        if (tile.value == this.player && this.selectedTile == null)
        {
            // Select a tile
            this.selectedTile = tile;
            this.selectedTile.selected.SetActive(true);
        }
        else if (tile.value != this.player)
        {
            if (this.AllowedPos(tile))
            {
                // TODO: Check tiles forward to allow movement
                // Move single tile
                this.MoveTile(this.selectedTile, tile);
                this.UnSelectTile();
            }
            else
            {
                // Move multiple tiles
                this.MoveMultipleTiles(tile);
            }
        }
        else
        {
            // Move more than one tiles
            // Detect how many ones in between. If valid, move them


        }
    }

    public void MoveTile(Tile tile, Tile nextTile)
    {
        nextTile.value = tile.value;
        nextTile.UpdateColor();
        tile.value = 0;
        tile.UpdateColor();
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


    private List<Tile> GetInvolvedTiles(Tile newTile)
    {
        int maxX = Mathf.Max(newTile.x, selectedTile.x);
        int maxY = Mathf.Max(newTile.y, selectedTile.y);
        int minX = Mathf.Min(newTile.x, selectedTile.x);
        int minY = Mathf.Min(newTile.y, selectedTile.y);
        List<Tile> selectedTiles = new List<Tile>();
        // selectedTiles.Add(newTile);
        // Add all the effected tiles
        if (newTile.x == selectedTile.x)
        {
            // Left-dig
            print("Left dig");
            // Loop through the tiles between [newTile, selectedTile]
            int dir = (newTile.y - selectedTile.y);
            dir = dir < 0 ? -1 : 1;
            int start = dir > 0 ? maxY : minY;
            int stop = dir > 0 ? minY : maxY;
            bool condition = dir > 0 ? (start >= stop) : (start <= stop); // start >= stop
            while (condition)
            {
                // print($"x={newTile.x}, y={start}");
                // print($"dir={dir}");
                // print($"start={start}");
                // print($"stop={stop}");
                // print($"condition={condition}");
                selectedTiles.Add(this.GetTile(new Vector2(start, newTile.x)));
                start += (dir * -1);
                // print($"start={start}");
                condition = dir > 0 ? (start >= stop) : (start <= stop); // start >= stop
            }
        }
        else if (newTile.y == selectedTile.y)
        {
            // Left-Right
            print("Left-Right");
            // Loop through the tiles between [newTile, selectedTile]
            int dir = (newTile.x - selectedTile.x);
            dir = dir < 0 ? -1 : 1;
            int start = dir > 0 ? maxX : minX;
            int stop = dir > 0 ? minX : maxX;
            bool condition = dir > 0 ? (start >= stop) : (start <= stop); // start <= stop
            while (condition)
            {
                selectedTiles.Add(this.GetTile(new Vector2(newTile.y, start)));
                start += (dir * -1);
                condition = dir > 0 ? (start >= stop) : (start <= stop); // start <= stop
            }

        }
        else if (newTile.x - selectedTile.x == newTile.y - selectedTile.y)
        {
            // Right-dig
            print("Right dig");
            // Loop through the tiles between [newTile, selectedTile]
            int dir = (newTile.x - selectedTile.x);
            dir = dir < 0 ? -1 : 1;
            int x = newTile.x;
            int y = newTile.y;
            bool condition = dir > 0 ? (x >= selectedTile.x && y >= selectedTile.y) : (x <= selectedTile.x && y <= selectedTile.y);
            while (condition)
            {
                selectedTiles.Add(this.GetTile(new Vector2(y, x)));
                x += (dir * -1);
                y += (dir * -1);
                condition = dir > 0 ? (x >= selectedTile.x && y >= selectedTile.y) : (x <= selectedTile.x && y <= selectedTile.y);
            }

        }
        else
        {
            // Sideways
        }
        return selectedTiles;
    }

    private void MoveMultipleTiles(Tile newTile)
    {
        List<Tile> selectedTiles = this.GetInvolvedTiles(newTile);
        if (newTile.value == 0)
        {
            // Empty tile. Alow movement
            for (int i = 0; i < selectedTiles.Count - 1; i++)
            {
                this.MoveTile(selectedTiles[i + 1], selectedTiles[i]);
            }
            this.UnSelectTile();
        }
        else if (newTile.value != this.player)
        {
            // Look forward. Either allow, then move all, or prevent
            // Get the selectedTile[0]
            // Search from selectedTile[0] to the same dir of movements
            // ! if found value=0 at selectedTile[0] + 1, move right away
            // if selectedTile[0] + 1 value != player, count forward until found null or value=0
            // IF count < selectedTiles.Count - 1, move them
        }
    }

    private int CalculateEnemyTiles()
    {

    }
}
