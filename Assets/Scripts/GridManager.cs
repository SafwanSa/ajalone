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
    public int player;
    private Tile selectedTile;
    public BoardUI boardUI;
    public BoardGenerator boardGenerator;
    [SerializeField] private TurnManager turnManager;

    private void Start() { }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == Events.CheckWinnerEvent)
        {
            // object[] data = (object[])photonEvent.CustomData;
            Debug.Log("Check Winner Event Handling...");
            this.HandleCheckWinnerEvent();
        }
        else { }
    }

    private void HandleCheckWinnerEvent()
    {
        if (this.boardGenerator.blackOutsIndicesFilled.Count == 6)
        {
            // White wins
            this.boardUI.UpdateWinnerText("White");
        }
        else if (this.boardGenerator.whiteOutsIndicesFilled.Count == 6)
        {
            // Black wins
            this.boardUI.UpdateWinnerText("Black");
        }
        else { }
    }

    private void CheckWinner()
    {
        Events.RaiseEventToAll(Events.CheckWinnerEvent, null);
    }

    // Game Mechanics
    public bool IsOnBoundaries(Tile tile)
    {
        int[] c = { 1, -1 };
        bool onBoundaries = false;
        foreach (int i in c)
        {
            if (this.GetTile(new Vector2(tile.y, tile.x + i)) == null ||
             this.GetTile(new Vector2(tile.y + i, tile.x)) == null ||
             this.GetTile(new Vector2(tile.y + i, tile.x + i)) == null)
            {
                onBoundaries = true;
                break;
            }
        }
        return onBoundaries;
    }

    public void SelectTile(Tile tile)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount > 1 && this.turnManager.IsMyTurn())
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
                bool removed = false;
                int oldTileValue = tile.value;
                bool onBoundaries = this.IsOnBoundaries(tile);

                if (onBoundaries)
                {
                    print("Move multiple to die");
                    Tile lastMovedTile = this.MoveMultipleTiles(newTile: tile);
                    removed = lastMovedTile && lastMovedTile == tile;
                }
                if (removed)
                {
                    this.boardGenerator.RemoveTile(oldTileValue);
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
        // Debug.Log($"Moving {tile.y},{tile.x} to {nextTile.y},{nextTile.x}");
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

    public bool IsOnAllowed(Tile tile, Tile otherTile)
    {
        int[] c = { 1, -1 };
        bool onAllowed = false;
        foreach (int i in c)
        {
            if (this.GetTile(new Vector2(tile.y, tile.x + i)) == otherTile ||
                this.GetTile(new Vector2(tile.y + i, tile.x)) == otherTile ||
                this.GetTile(new Vector2(tile.y + i, tile.x + i)) == otherTile)
            {
                onAllowed = true;
                break;
            }
        }
        return onAllowed;
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
            int start = dir > 0 ? minY : maxY;
            int stop = dir > 0 ? maxY : minY;
            bool condition = dir > 0 ? (start <= stop) : (start >= stop); // start <= stop
            bool foundEmpty = false;
            while (condition && !foundEmpty)
            {
                // print($"x={newTile.x}, y={start}");
                // print($"dir={dir}");
                // print($"start={start}");
                // print($"stop={stop}");
                // print($"condition={condition}");
                Tile t = this.GetTile(new Vector2(start, newTile.x));
                if (t.value == 0) foundEmpty = true;
                selectedTiles.Add(t);
                start += dir;
                // print($"start={start}");
                condition = dir > 0 ? (start <= stop) : (start >= stop); // start <= stop
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
            int start = dir > 0 ? minX : maxX;
            int stop = dir > 0 ? maxX : minX;
            bool condition = dir > 0 ? (start <= stop) : (start >= stop);
            bool foundEmpty = false;
            while (condition && !foundEmpty)
            {
                Tile t = this.GetTile(new Vector2(newTile.y, start));
                if (t.value == 0) foundEmpty = true;
                selectedTiles.Add(t);
                start += dir;
                condition = dir > 0 ? (start <= stop) : (start >= stop);
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
            int x = selectedTile.x;
            int y = selectedTile.y;
            bool condition = dir > 0 ? (x <= newTile.x && y <= newTile.y) : (x >= newTile.x && y >= newTile.y);
            bool foundEmpty = false;
            while (condition && !foundEmpty)
            {
                Tile t = this.GetTile(new Vector2(y, x));
                if (t.value == 0) foundEmpty = true;
                selectedTiles.Add(t);
                x += dir;
                y += dir;
                condition = dir > 0 ? (x <= newTile.x && y <= newTile.y) : (x >= newTile.x && y >= newTile.y);
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
        else if (Mathf.Abs(newTile.x - selectedTile.x) <= 3 && Mathf.Abs(newTile.y - selectedTile.y) <= 3)
        {
            // Sideways ! moves using different way
            // TODO: Refactor this spaghetti code!!
            print("Sideways");
            List<Tile> __tiles = new List<Tile>();
            __tiles.Add(selectedTile);
            int diffY = newTile.y - selectedTile.y;
            int diffX = newTile.x - selectedTile.x;
            // int dirX = diffX > 0 ? 1 : -1;
            int searchX = 0;
            int searchY = 0;

            // Where to search
            // Move next till newTile is on the allowed positions
            bool foundOther = false;
            int counter = 3;
            if (diffY >= 1)
            {
                // Top
                Debug.Log("On Top");
                if (diffX >= 1)
                {
                    Debug.Log("On Right and center");
                    // Right and center
                    // Search right-dig, left-dig, and right
                    // ! Searching right-dig
                    Debug.Log("Searching right-dig");
                    searchY = 1;
                    searchX = 1;
                    while (counter >= 0)
                    {
                        Tile temp = this.GetTile(new Vector2(selectedTile.y + searchY, selectedTile.x + searchX));
                        if (temp == null || temp.value != selectedTile.value) break;
                        __tiles.Add(temp);
                        if (this.IsOnAllowed(newTile, temp)) { foundOther = true; break; }
                        counter -= 1;
                        searchY += 1;
                        searchX += 1;
                    }
                    if (!foundOther)
                    {
                        counter = 3;
                        if (__tiles.Count > 1)
                            __tiles.RemoveRange(1, __tiles.Count - 1);
                        // ! Searching left-dig
                        Debug.Log("Searching left-dig");
                        searchY = 1;
                        while (counter >= 0)
                        {
                            Tile temp = this.GetTile(new Vector2(selectedTile.y + searchY, selectedTile.x));
                            if (temp == null || temp.value != selectedTile.value) break;
                            __tiles.Add(temp);
                            if (this.IsOnAllowed(newTile, temp)) { foundOther = true; break; }
                            counter -= 1;
                            searchY += 1;
                        }
                    }
                    if (!foundOther)
                    {
                        counter = 3;
                        if (__tiles.Count > 1)
                            __tiles.RemoveRange(1, __tiles.Count - 1);
                        // ! Searching right
                        Debug.Log("Searching right");
                        searchX = 1;
                        while (counter >= 0)
                        {
                            Tile temp = this.GetTile(new Vector2(selectedTile.y, selectedTile.x + searchX));
                            if (temp == null || temp.value != selectedTile.value) break;
                            __tiles.Add(temp);
                            if (this.IsOnAllowed(newTile, temp)) { foundOther = true; break; }
                            counter -= 1;
                            searchX += 1;
                        }
                    }
                }
                else
                {
                    Debug.Log("On Left");
                    // Left
                    // Search left-dig and left
                    // ! Searching left-dig
                    Debug.Log("Searching left-dig");
                    searchY = 1;
                    while (counter >= 0)
                    {
                        Tile temp = this.GetTile(new Vector2(selectedTile.y + searchY, selectedTile.x));
                        if (temp == null || temp.value != selectedTile.value) break;
                        __tiles.Add(temp);
                        if (this.IsOnAllowed(newTile, temp)) { foundOther = true; break; }
                        counter -= 1;
                        searchY += 1;
                    }
                    if (!foundOther)
                    {
                        counter = 3;
                        if (__tiles.Count > 1)
                            __tiles.RemoveRange(1, __tiles.Count - 1);
                        // ! Searching left
                        Debug.Log("Searching left");
                        searchX = -1;
                        while (counter >= 0)
                        {
                            Tile temp = this.GetTile(new Vector2(selectedTile.y, selectedTile.x + searchX));
                            if (temp == null || temp.value != selectedTile.value) break;
                            __tiles.Add(temp);
                            if (this.IsOnAllowed(newTile, temp)) { foundOther = true; break; }
                            counter -= 1;
                            searchX -= 1;
                        }
                    }
                }

            }
            else
            {
                // Bottom
                Debug.Log("On Bottom");
                if (diffX >= 1)
                {
                    Debug.Log("On Right");
                    // Right
                    // Search -left-dig and right
                    // ! Searching -left-dig
                    Debug.Log("Searching -left-dig");
                    searchY = -1;
                    while (counter >= 0)
                    {
                        Tile temp = this.GetTile(new Vector2(selectedTile.y + searchY, selectedTile.x));
                        if (temp == null || temp.value != selectedTile.value) break;
                        __tiles.Add(temp);
                        if (this.IsOnAllowed(newTile, temp)) { foundOther = true; break; }
                        counter -= 1;
                        searchY -= 1;
                    }
                    if (!foundOther)
                    {
                        counter = 3;
                        if (__tiles.Count > 1)
                            __tiles.RemoveRange(1, __tiles.Count - 1);
                        // ! Searching right
                        Debug.Log("Searching right");
                        searchX = 1;
                        while (counter >= 0)
                        {
                            Tile temp = this.GetTile(new Vector2(selectedTile.y, selectedTile.x + searchX));
                            if (temp == null || temp.value != selectedTile.value) break;
                            __tiles.Add(temp);
                            if (this.IsOnAllowed(newTile, temp)) { foundOther = true; break; }
                            counter -= 1;
                            searchX += 1;
                        }
                    }

                }
                else
                {
                    Debug.Log("On Left and Center");
                    // Left and center
                    // Search -right-dif, -left-dig, and left
                    // ! Searching -right-dig
                    Debug.Log("Searching -right-dig");
                    searchY = -1;
                    searchX = -1;
                    while (counter >= 0)
                    {
                        Tile temp = this.GetTile(new Vector2(selectedTile.y + searchY, selectedTile.x + searchX));
                        if (temp == null || temp.value != selectedTile.value) break;
                        __tiles.Add(temp);
                        if (this.IsOnAllowed(newTile, temp)) { foundOther = true; break; }
                        counter -= 1;
                        searchY -= 1;
                        searchX -= 1;
                    }
                    if (!foundOther)
                    {
                        counter = 3;
                        if (__tiles.Count > 1)
                            __tiles.RemoveRange(1, __tiles.Count - 1);
                        // ! Searching -left-dig
                        Debug.Log("Searching -left-dig");
                        searchY = -1;
                        while (counter >= 0)
                        {
                            Tile temp = this.GetTile(new Vector2(selectedTile.y + searchY, selectedTile.x));
                            if (temp == null || temp.value != selectedTile.value) break;
                            __tiles.Add(temp);
                            if (this.IsOnAllowed(newTile, temp)) { foundOther = true; break; }
                            counter -= 1;
                            searchY -= 1;
                        }
                    }
                    if (!foundOther)
                    {
                        counter = 3;
                        if (__tiles.Count > 1)
                            __tiles.RemoveRange(1, __tiles.Count - 1);
                        // ! Searching left
                        Debug.Log("Searching left");
                        searchX = -1;
                        while (counter >= 0)
                        {
                            Tile temp = this.GetTile(new Vector2(selectedTile.y, selectedTile.x + searchX));
                            if (temp == null || temp.value != selectedTile.value) break;
                            __tiles.Add(temp);
                            if (this.IsOnAllowed(newTile, temp)) { foundOther = true; break; }
                            counter -= 1;
                            searchX -= 1;
                        }
                    }
                }
            }
            if (foundOther)
            {
                // foreach (Tile t in __tiles) {
                //     Debug.Log($"y={t.y}, x={t.x}, value={t.value}");
                // }
                Tile lastTile = __tiles[__tiles.Count - 1];
                // Get the right directions
                int dirX = 0;
                int dirY = 0;
                bool canMove = true;
                if (newTile.x == lastTile.x)
                {
                    // Left dig
                    dirX = 0;
                    dirY = newTile.y - lastTile.y > 0 ? 1 : -1;
                    canMove = true;
                }
                else if (newTile.y == lastTile.y)
                {
                    // Left-right
                    dirX = newTile.x - lastTile.x > 0 ? 1 : -1;
                    dirY = 0;
                    canMove = true;
                }
                else if (newTile.x - lastTile.x == newTile.y - lastTile.y)
                {
                    // Right dig
                    dirX = newTile.x - lastTile.x > 0 ? 1 : -1;
                    dirY = newTile.y - lastTile.y > 0 ? 1 : -1;
                    canMove = true;
                }
                else
                {
                    dirX = 0;
                    dirY = 0;
                    canMove = false;
                }

                for (int i = 0; i < __tiles.Count; i++)
                {
                    Tile targetTile = this.GetTile(new Vector2(__tiles[i].y + dirY, __tiles[i].x + dirX));
                    if (targetTile.value != 0) { canMove = false; break; }
                }
                if (canMove)
                {
                    for (int i = 0; i < __tiles.Count; i++)
                    {
                        Tile targetTile = this.GetTile(new Vector2(__tiles[i].y + dirY, __tiles[i].x + dirX));
                        this.MoveTile(__tiles[i], targetTile);
                    }
                    this.UnSelectTile();
                    this.turnManager.HandTurn();
                    return;
                }
            }
            else
            {
                Debug.Log("not found");
                return;
            }
        }
        else
        {
            // Not a move 
            return;
        }
    }

    private Tile MoveMultipleTiles(Tile newTile)
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
        foreach (Tile var in selectedTiles)
        {
            print($"y={var.y}, x={var.x}, v={var.value} is selected");
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
            for (int i = selectedTiles.Count - 1; i > 0; i--)
            {
                this.MoveTile(selectedTiles[i - 1], selectedTiles[i]);
            }
            this.UnSelectTile();
            this.turnManager.HandTurn();
            return selectedTiles[selectedTiles.Count - 1];
        }
        return null;
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
