using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class GridManager : MonoBehaviour, IOnEventCallback
{
    [SerializeField] private Tile tilePrefab;
    private List<int> blackOutsIndicesFilled = new List<int>();
    private List<int> whiteOutsIndicesFilled = new List<int>();
    public int player;
    private Tile selectedTile;
    [SerializeField] public BoardUI boardUI;
    [SerializeField] private BoardGenerator boardGenerator;
    [SerializeField] private TurnManager turnManager;

    private void Start()
    {
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == Events.CheckWinnerEvent)
        {
            // object[] data = (object[])photonEvent.CustomData;
            Debug.Log("Check Winner Event Handling...");
            this.HandleCheckWinnerEvent();
        }
        else if (photonEvent.Code == Events.RemoveTileEvent)
        {
            Debug.Log("Remove Tile Event Handling...");
            object[] data = (object[])photonEvent.CustomData;
            this.HandleRemoveTileEvent((int)data[0]);
        }
        else { }
    }

    private void HandleRemoveTileEvent(int i)
    {
        this.boardGenerator.InstantiateDeadTilePrefab(this.player, i);
        if (this.player == 2) this.blackOutsIndicesFilled.Add(i);
        else this.whiteOutsIndicesFilled.Add(i);
    }

    private void HandleCheckWinnerEvent()
    {
        if (this.blackOutsIndicesFilled.Count == 1)
        {
            // White wins
            this.boardUI.UpdateWinnerText("White");
        }
        else if (this.whiteOutsIndicesFilled.Count == 1)
        {
            // Black wins
            this.boardUI.UpdateWinnerText("Black");
        }
        else { }
    }

    private void RemoveTile()
    {
        // this.player, is updated because of the the toggle
        List<int> _temp = this.player == 1 ? this.whiteOutsIndicesFilled : this.blackOutsIndicesFilled;
        for (int i = 0; i < 6; i++)
        {
            if (_temp.IndexOf(i) == -1)
            {
                // Found an empty place
                Events.RaiseEventToAll(Events.RemoveTileEvent, new object[] { i });
                break;
            }
        }
    }

    private void CheckWinner()
    {
        Events.RaiseEventToAll(Events.CheckWinnerEvent, null);
    }

    // Game Mechanics
    public void SelectTile(Tile tile)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount > 1 && this.player == PhotonNetwork.LocalPlayer.ActorNumber)
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
                    this.turnManager.HandTurn();
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
                    // this.CalculateTiles();
                    this.RemoveTile();
                    this.CheckWinner();
                }
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
        if (this.boardGenerator.tiles.TryGetValue(pos, out var tile))
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
            this.turnManager.HandTurn();
        }
    }


    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}
