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
    [SerializeField] private int white;
    [SerializeField] private int black;

    private void Start()
    {
        this.player = 1;
        GenerateGrid();
        CalculateTiles();
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
        this.cam.position = new Vector3(center.x, center.y, -30f);
    }

    private void CalculateTiles()
    {
        // Tile[] _tiles = this.tiles.Values.ToArray();
        this.white = 0;
        this.black = 0;
        foreach (KeyValuePair<Vector2, Tile> pair in this.tiles)
        {
            if (pair.Value.value == 1) this.white++;
            if (pair.Value.value == 2) this.black++;
        }
    }

    private void TogglePlayerTurn()
    {
        this.player = this.player == 1 ? 2 : 1;
    }

    public void SelectTile(Tile tile)
    {
        if (this.selectedTile == null && tile.value == this.player)
        {
            // Select a tile
            this.selectedTile = tile;
            this.selectedTile.highlight.SetActive(false);
            this.selectedTile.selected.SetActive(true);
            this.selectedTile.highlight.SetActive(true);
        }
        else if (this.selectedTile != null && tile.value == 0)
        {
            // Move a tile not in boundaries
            if (this.AllowedPos(tile))
            {
                // Move single tile
                print("Move single");
                this.MoveTile(this.selectedTile, tile);
                this.UnSelectTile();
                this.TogglePlayerTurn();
            }
            else
            {
                // Move multiple tiles
                print("Move multiple");
                this.MoveMultipleTiles(tile);
            }
        }
        else if (this.selectedTile != null && tile.value != this.player)
        {
            // Move a tile in boundaries
            int[] c = { 1, -1 };
            foreach (int i in c)
            {
                if (this.GetTile(new Vector2(tile.y, tile.x + i)) == null ||
                 this.GetTile(new Vector2(tile.y, tile.x + i)) == null ||
                 this.GetTile(new Vector2(tile.y + i, tile.x)) == null ||
                 this.GetTile(new Vector2(tile.y + i, tile.x)) == null)
                {
                    print("Move multiple to die");
                    this.MoveMultipleTiles(tile);
                    break;
                }
            }
            this.CalculateTiles();
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


    private void GetInvolvedTiles(Tile newTile, ref List<Tile> selectedTiles, ref List<int> directionalValues)
    {
        int maxX = Mathf.Max(newTile.x, selectedTile.x);
        int maxY = Mathf.Max(newTile.y, selectedTile.y);
        int minX = Mathf.Min(newTile.x, selectedTile.x);
        int minY = Mathf.Min(newTile.y, selectedTile.y);

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
            int y = selectedTile.y;
            while (y >= 1 && y <= 9)
            {
                Tile t = this.GetTile(new Vector2(y, selectedTile.x));
                if (t == null) break;
                directionalValues.Add(t.value);
                // print($"x={selectedTile.x}   y={y}       value={t.value}");
                y += dir;
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
            int x = selectedTile.x;
            while (x >= 1 && x <= 9)
            {
                Tile t = this.GetTile(new Vector2(selectedTile.y, x));
                if (t == null) break;
                directionalValues.Add(t.value);
                // print($"x={x}   y={selectedTile.y}       value={t.value}");
                x += dir;
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
            x = selectedTile.x;
            y = selectedTile.y;
            while (y >= 1 && y <= 9 && x >= 1 && x <= 9)
            {
                Tile t = this.GetTile(new Vector2(y, x));
                if (t == null) break;
                directionalValues.Add(t.value);
                // print($"x={x}   y={y}       value={t.value}");
                y += dir;
                x += dir;
            }
        }
        else
        {
            // Sideways
        }
        // return selectedTiles;
    }

    private void MoveMultipleTiles(Tile newTile)
    {
        List<Tile> selectedTiles = new List<Tile>();
        List<int> directionalValues = new List<int>(); // All values in the dir of the move [selectedTile, last bounded tile]
        this.GetInvolvedTiles(newTile, ref selectedTiles, ref directionalValues);


        // Look forward. Either allow, then move all, or prevent
        // ! if found value=0 at selectedTile[0] + 1, move right away
        // Calculate all enemies tiles ahead of movement dir
        int enemyCounter = 0;
        int friendlyCounter = 0;
        bool friendlyOnTheWay = false;
        bool startCountingEnemy = false;
        bool startCountingFriendly = false;
        foreach (int var in directionalValues)
        {
            print(var);
        }
        for (int i = 0; i < directionalValues.Count; i++)
        {
            if (directionalValues[i] == 0) break;
            else if (directionalValues[i] != this.player)
            {
                if (!startCountingEnemy) startCountingEnemy = true;
                enemyCounter++;
            }
            else
            {
                if (!startCountingFriendly) startCountingFriendly = true;
                if (startCountingEnemy)
                {
                    friendlyOnTheWay = true;
                    break;
                }
                friendlyCounter++;
            }
        }
        print($"{enemyCounter} enemy");
        print($"{friendlyCounter} friend");
        print($"is friend on way? {friendlyOnTheWay}");
        // If counter < selectedTiles.Count - 1 and no friendly tiles ahead, move them
        if (enemyCounter < friendlyCounter && !friendlyOnTheWay)
        {
            for (int i = 0; i < selectedTiles.Count - 1; i++)
            {
                this.MoveTile(selectedTiles[i + 1], selectedTiles[i]);
            }
            this.UnSelectTile();
            this.TogglePlayerTurn();
        }


    }

    // private int CalculateEnemyTiles()
    // {
    //     return 0;
    // }
}
