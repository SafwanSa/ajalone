using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;
using Photon.Realtime;


public class TurnManager : MonoBehaviour, IOnEventCallback
{

    [SerializeField] private GridManager gridManager;
    [SerializeField] private int turn;

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            this.turn = 1;
            this.SetupPUNRoomProperties();
        }
        else
        {
            // Get the current turn from the room props
            this.GetTurnState();
        }
        this.HandleTogglePlayerTurnEvent(this.turn);
    }

    /// <summary>Call to switch the turn (used by both players).</summary>
    public void HandTurn()
    {
        int turn = this.turn == 1 ? 2 : 1;
        Debug.Log("Turn handed: " + this.turn);
        Events.RaiseEventToAll(Events.TogglePlayerTurnEvent, new object[] { turn });
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == Events.TogglePlayerTurnEvent)
        {
            object[] data = (object[])photonEvent.CustomData;
            this.HandleTogglePlayerTurnEvent((int)data[0]);
        }
    }

    private void HandleTogglePlayerTurnEvent(int _turn)
    {
        this.turn = _turn;
        // Update player in GridManager
        this.gridManager.player = _turn;
        // Update UI
        this.gridManager.boardUI.UpdatePlayerTurn(_turn);
        // Save the last turn in props
        if (PhotonNetwork.IsMasterClient) this.UpdateTurnState();
    }

    private void UpdateTurnState()
    {
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
        properties["gameTurn"] = this.turn;
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        Debug.Log("Turn state updated");
    }

    private void SetupPUNRoomProperties()
    {
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
        properties.Add("gameTurn", this.turn);
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        Debug.Log("Turn state set");
    }

    private void GetTurnState()
    {
        Debug.Log("Not master client, getting turn state...");
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
        this.turn = (int)properties["gameTurn"];
        Debug.Log("Turn data is loaded");
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