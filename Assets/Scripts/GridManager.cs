using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GridManager : MonoBehaviour
{
    [SerializeField] private int width, height;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Transform cam;
    [SerializeField] private List<GameObject> rows;
    [SerializeField] private Dictionary<Vector2, Tile> tiles;
    [SerializeField] private GameObject blackOuts;
    [SerializeField] private GameObject whiteOuts;
    private List<int> blackOutsIndicesFilled = new List<int>();
    private List<int> whiteOutsIndicesFilled = new List<int>();
    [SerializeField] private int white;
    [SerializeField] private int black;
    public int player;
    private Tile selectedTile;

    private void Start()
    {
        this.player = 1;
        GenerateGrid();
        CalculateTiles();
    }

    private void RemoveTile()
    {
        List<int> _temp = this.player == 1 ? this.whiteOutsIndicesFilled : this.blackOutsIndicesFilled;
        int indx = this.player == 1 ? 1 : 9;

        for (int i = 0; i < 6; i++)
        {
            if (_temp.IndexOf(i) == -1)
            {
                GameObject _out = this.player == 2 ? this.blackOuts.transform.GetChild(i).gameObject : this.whiteOuts.transform.GetChild(i).gameObject;
                var spawnedTile = Instantiate(this.tilePrefab, new Vector3(_out.transform.position.x, _out.transform.position.y, _out.transform.position.z), Quaternion.identity);
                spawnedTile.GetComponent<CircleCollider2D>().radius = 0f;
                spawnedTile.transform.parent = _out.transform;
                RectTransform trans = spawnedTile.gameObject.AddComponent<RectTransform>();
                trans.anchorMin = new Vector2(0f, 0f);
                trans.anchorMax = new Vector2(1f, 1f);
                trans.pivot = new Vector2(0.5f, 0.5f);
                RectTransformExtensions.SetLeft(trans, 20);
                RectTransformExtensions.SetTop(trans, 25);
                RectTransformExtensions.SetRight(trans, 20);
                RectTransformExtensions.SetBottom(trans, 25);
                trans.localPosition = new Vector3(trans.localPosition.x, trans.localPosition.y, -1);
                spawnedTile.Init(indx, indx, this);
                if (this.player == 2) this.blackOutsIndicesFilled.Add(i);
                else this.whiteOutsIndicesFilled.Add(i);
                break;
            }
        }
    }

    private void GenerateGrid()
    {
        this.tiles = new Dictionary<Vector2, Tile>();
        for (int i = 0; i < this.rows.Count; i++)
        {
            for (int j = 0; j < GetNumOfBallsInRow(i + 1); j++)
            {
                GameObject spot = this.rows[i].transform.GetChild(j).gameObject;
                string[] name = spot.name.Split(char.Parse(" "));
                int y = int.Parse(name[1]);
                int x = int.Parse(name[2]);
                var spawnedTile = Instantiate(this.tilePrefab, new Vector3(spot.transform.position.x, spot.transform.position.y, spot.transform.position.z), Quaternion.identity);
                spawnedTile.transform.parent = spot.transform;
                RectTransform trans = spawnedTile.gameObject.AddComponent<RectTransform>();
                trans.anchorMin = new Vector2(0f, 0f);
                trans.anchorMax = new Vector2(1f, 1f);
                trans.pivot = new Vector2(0.5f, 0.5f);
                RectTransformExtensions.SetLeft(trans, 20);
                RectTransformExtensions.SetTop(trans, 25);
                RectTransformExtensions.SetRight(trans, 20);
                RectTransformExtensions.SetBottom(trans, 25);
                trans.localPosition = new Vector3(trans.localPosition.x, trans.localPosition.y, -1);
                spawnedTile.Init(x, y, this);
                this.tiles[new Vector2(y, x)] = spawnedTile;
            }
        }
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
            bool removed = false;
            foreach (int i in c)
            {
                if (this.GetTile(new Vector2(tile.y, tile.x + i)) == null ||
                 this.GetTile(new Vector2(tile.y, tile.x + i)) == null ||
                 this.GetTile(new Vector2(tile.y + i, tile.x)) == null ||
                 this.GetTile(new Vector2(tile.y + i, tile.x)) == null)
                {
                    print("Move multiple to die");
                    this.MoveMultipleTiles(tile);
                    removed = true;
                    break;
                }
            }
            if (removed)
            {
                this.CalculateTiles();
                this.RemoveTile();
            }
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
        if (enemyCounter < friendlyCounter && !friendlyOnTheWay && friendlyCounter <= 3)
        {
            for (int i = 0; i < selectedTiles.Count - 1; i++)
            {
                this.MoveTile(selectedTiles[i + 1], selectedTiles[i]);
            }
            this.UnSelectTile();
            this.TogglePlayerTurn();
        }


    }

}
