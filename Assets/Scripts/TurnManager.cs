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
    [SerializeField] private AudioSource audio;

    private void Start()
    {
        this.audio = GetComponent<AudioSource>();
        if (PhotonNetwork.IsMasterClient)
        {
            this.turn = 1;
            this.SetupPUNRoomProperties();
        }
        else
        {
            // Get the current turn from the room props
            this.GetTurnState();
            if (!IsRoomCreator(PhotonNetwork.LocalPlayer))
            {
                this.gridManager.boardUI.RotateCamera();
            }
        }
        this.HandleTogglePlayerTurnEvent(this.turn);
        this.gridManager.boardUI.UpdatePlayerUI();
    }

    /// <summary>Call to switch the turn (used by both players).</summary>
    public void HandTurn()
    {
        int turn = this.turn == 1 ? 2 : 1;
        // turn = turn == 1 ? 2 : 1;
        Debug.Log("Turn handed: " + this.turn);
        Events.RaiseEventToAll(Events.TogglePlayerTurnEvent, new object[] { turn });
        this.gridManager.boardGenerator.UpdateBoardState(); // This raises an event to the master to update BoardState
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
        // Play sound
        this.audio.Play(0);
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
        properties.Add("masterUserId", PhotonNetwork.LocalPlayer.UserId);
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

    private string GetCreatorId()
    {
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
        return (string)properties["masterUserId"];
    }

    private bool IsRoomCreator(Player player)
    {
        return this.GetCreatorId() == player.UserId.ToString();
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