using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;


public class BoardGenerator : MonoBehaviour, IOnEventCallback
{
    [SerializeField] private int width, height;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] public List<GameObject> rows;
    [SerializeField] public Dictionary<Vector2, Tile> tiles;
    [SerializeField] public List<int> blackOutsIndicesFilled = new List<int>();
    [SerializeField] public List<int> whiteOutsIndicesFilled = new List<int>();
    [SerializeField] private Dictionary<string, int> boardState;
    [SerializeField] private GameObject blackOuts;
    [SerializeField] private GameObject whiteOuts;
    // public int player;


    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Game created
            this.GenerateBoard();
            this.SetupPUNRoomProperties();
        }
        else
        {
            // Someone joins
            this.GeneratePreCreatedBoard();
            this.GeneratePreCreatedOuts();
        }
    }

    private void GenerateBoard()
    {
        this.tiles = new Dictionary<Vector2, Tile>();
        this.boardState = new Dictionary<string, int>();
        for (int i = 0; i < this.rows.Count; i++)
        {
            for (int j = 0; j < this.GetNumOfBallsInRow(i + 1); j++)
            {
                GameObject spot = this.rows[i].transform.GetChild(j).gameObject;
                string[] name = spot.name.Split(char.Parse(" "));
                int y = int.Parse(name[1]);
                int x = int.Parse(name[2]);
                int value = this.InstantiateTilePrefab(y, x, -1, spot, true);
                this.boardState[this.ToStr(y, x)] = value;
            }
        }
    }

    private void GeneratePreCreatedBoard()
    {
        Debug.Log("Not master client, getting room data...");
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
        this.tiles = new Dictionary<Vector2, Tile>();

        for (int i = 0; i < this.rows.Count; i++)
        {
            for (int j = 0; j < this.GetNumOfBallsInRow(i + 1); j++)
            {
                GameObject spot = this.rows[i].transform.GetChild(j).gameObject;
                string[] name = spot.name.Split(char.Parse(" "));
                int y = int.Parse(name[1]);
                int x = int.Parse(name[2]);
                string key = this.ToStr(y, x);
                int value = (int)properties[key];
                value = this.InstantiateTilePrefab(y, x, value, spot, false);
            }
        }

        Debug.Log("Board data is loaded");
    }

    private void GeneratePreCreatedOuts()
    {
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
        // Whites
        for (int i = 0; i < 6; i++)
        {
            bool isOut = (bool)properties[$"white_{i}"];
            if (isOut)
            {
                this.whiteOutsIndicesFilled.Add(i);
                this.InstantiateDeadTilePrefab(1, i);
            }
        }
        // Blacks
        for (int i = 0; i < 6; i++)
        {
            bool isOut = (bool)properties[$"black_{i}"];
            if (isOut)
            {
                this.blackOutsIndicesFilled.Add(i);
                this.InstantiateDeadTilePrefab(2, i);
            }
        }
        Debug.Log("Board outs data is loaded");
    }

    private int InstantiateTilePrefab(int y, int x, int value, GameObject spot, bool defaultColors)
    {
        Tile spawnedTile = Instantiate(
            this.tilePrefab,
            new Vector3(spot.transform.position.x, spot.transform.position.y, spot.transform.position.z),
            Quaternion.identity
        );
        spawnedTile.name = spot.name;
        spawnedTile.transform.SetParent(spot.transform);
        // spawnedTile.gameObject.transform.scale = new Vector3(1, 1, 1);
        RectTransform trans = spawnedTile.gameObject.AddComponent<RectTransform>();
        trans.anchorMin = new Vector2(0.5f, 0.5f);
        trans.anchorMax = new Vector2(0.5f, 0.5f);
        trans.pivot = new Vector2(0.5f, 0.5f);
        trans.localScale = new Vector3(1f, 1f, 1f);
        RectTransformExtensions.SetLeft(trans, 20);
        RectTransformExtensions.SetTop(trans, 25);
        RectTransformExtensions.SetRight(trans, 20);
        RectTransformExtensions.SetBottom(trans, 25);
        trans.localPosition = new Vector3(trans.localPosition.x, trans.localPosition.y, -1);
        int tileValue = -1;
        if (defaultColors)
        {
            tileValue = this.GetSetupTileColor(y, x);
        }
        else
        {
            tileValue = value;
        }
        spawnedTile.Init(x, y, tileValue);
        this.tiles[new Vector2(y, x)] = spawnedTile;
        return tileValue;
    }

    public void InstantiateDeadTilePrefab(int tileValue, int index)
    {
        GameObject _out = tileValue == 2 ? this.blackOuts.transform.GetChild(index).gameObject : this.whiteOuts.transform.GetChild(index).gameObject;
        Tile spawnedTile = Instantiate(
            this.tilePrefab,
            new Vector3(_out.transform.position.x, _out.transform.position.y, _out.transform.position.z),
            Quaternion.identity
        );
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
        spawnedTile.Init(-1, -1, tileValue);
    }

    private void SetupPUNRoomProperties()
    {
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;

        foreach (KeyValuePair<string, int> tileData in this.boardState)
        {
            properties.Add(tileData.Key, tileData.Value);
        }

        // Init outs
        for (int i = 0; i < 6; i++)
        {
            properties.Add($"black_{i}", false);
        }
        for (int i = 0; i < 6; i++)
        {
            properties.Add($"white_{i}", false);
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        Debug.Log("Board state set");
    }

    public void SetBoardStateForNewMaster(Player newMasterClient)
    {
        // Update board sate
        this.boardState = new Dictionary<string, int>();
        foreach (KeyValuePair<Vector2, Tile> tile in this.tiles)
        {
            this.boardState[this.ToStr(tile.Value.y, tile.Value.x)] = tile.Value.value;
        }
    }

    private void UpdatePUNRoomProperties()
    {
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;

        foreach (KeyValuePair<string, int> tileData in this.boardState)
        {
            properties[tileData.Key] = tileData.Value;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        Debug.Log("Board state set");
    }

    private void UpdateOutsRoomProperties(int tileValue, int i)
    {
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;

        // Init outs
        if (tileValue == 1)
        {
            properties[$"white_{i}"] = true;
        }
        else
        {
            properties[$"black_{i}"] = true;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        Debug.Log("Board outs state set");
    }

    private void HandleUpdateBoardState()
    {
        foreach (KeyValuePair<Vector2, Tile> tile in this.tiles)
        {
            this.boardState[this.ToStr(tile.Value.y, tile.Value.x)] = tile.Value.value;
        }
        this.UpdatePUNRoomProperties();
    }

    public void UpdateBoardState()
    {
        Events.RaiseEventToMaster(Events.UpdateBoardStateEvent, null);
    }

    private void HandleRemoveTileEvent(int tileValue, int i)
    {
        this.InstantiateDeadTilePrefab(tileValue, i);
        if (tileValue == 2) this.blackOutsIndicesFilled.Add(i);
        else this.whiteOutsIndicesFilled.Add(i);
        if (PhotonNetwork.IsMasterClient) this.UpdateOutsRoomProperties(tileValue, i);
    }

    public void RemoveTile(int tileValue)
    {
        // this.player, is updated because of the the toggle
        List<int> _temp = tileValue == 1 ? this.whiteOutsIndicesFilled : this.blackOutsIndicesFilled;
        for (int i = 0; i < 6; i++)
        {
            if (_temp.IndexOf(i) == -1)
            {
                // Found an empty place
                Events.RaiseEventToAll(Events.RemoveTileEvent, new object[] { tileValue, i });
                break;
            }
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == Events.UpdateBoardStateEvent)
        {
            this.HandleUpdateBoardState();
        }
        else if (photonEvent.Code == Events.RemoveTileEvent)
        {
            Debug.Log("Remove Tile Event Handling...");
            object[] data = (object[])photonEvent.CustomData;
            this.HandleRemoveTileEvent((int)data[0], (int)data[1]);
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

    private int GetSetupTileColor(int y, int x)
    {
        int value = 0;
        if (y < 3 || (y == 3 && x > 2 && x < 6))
            value = 1;
        else if (y >= 8 || (y == 7 && x > 4 && x < 8))
            value = 2;
        else
            value = 0;
        return value;
    }

    private void ToInt(string key, out int y, out int x)
    {
        string[] ket_str = key.Split(char.Parse(","));
        y = int.Parse(ket_str[0]);
        x = int.Parse(ket_str[1]);
    }

    private string ToStr(int y, int x)
    {
        return $"{y},{x}";
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