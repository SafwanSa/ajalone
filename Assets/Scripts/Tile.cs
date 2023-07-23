using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class Tile : MonoBehaviour, IOnEventCallback
{
    [SerializeField] SpriteRenderer _renderer;
    public GameObject highlight;
    public GameObject selected;
    private GridManager grid;
    [SerializeField] Material black;
    [SerializeField] Material white;
    [SerializeField] Material gray;
    public int x, y;
    public int value = 0;
    [SerializeField] private GridManager gridManager;

    public void Init(int x, int y, int value)
    {
        this.gridManager = FindObjectOfType<GridManager>();
        this.x = x;
        this.y = y;
        this.value = value;
        this.SetColor();
    }

    public void SetColor()
    {
        if (this.value == 0)
        {
            this.highlight.SetActive(false);
        }
        else if (this.value == 1)
        {
            this.highlight.SetActive(true);
            this.highlight.GetComponent<MeshRenderer>().material = this.white;
        }
        else
        {
            this.highlight.SetActive(true);
            this.highlight.GetComponent<MeshRenderer>().material = this.black;
        }
    }

    public void UpdateColor()
    {
        SetColor();
        Events.RaiseEventToAll(Events.UpdateColorEvent, new object[] { this.y, this.x, this.value });
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == Events.UpdateColorEvent)
        {
            object[] data = (object[])photonEvent.CustomData;
            int y = (int)data[0];
            int x = (int)data[1];
            int value = (int)data[2];
            if (this.x == x && this.y == y)
            {
                this.value = value;
                this.SetColor();
            }
        }
    }

    void OnMouseDown()
    {
        if (this.gridManager.IsSelected(this))
        {
            this.gridManager.UnSelectTile();
        }
        else
        {
            this.gridManager.SelectTile(this);
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
