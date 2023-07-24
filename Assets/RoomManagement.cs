using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // <= THIS
using Photon.Realtime;

public class RoomManagement : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public TMP_InputField joinInput;
    public TMP_Text errorMsg;
    public string roomName;
    public bool isRoomCreated;

    void SavePrefs()
    {
        PlayerPrefs.SetString("roomName", this.roomName);
        PlayerPrefs.SetInt("isRoomCreated", this.isRoomCreated ? 1 : 0);
    }

    public void CreateRoom()
    {
        this.roomName = GenerateRandomAlphaNumericStr(4).ToLower();
        this.isRoomCreated = true;
        this.SavePrefs();
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        roomOptions.PublishUserId = true;
        PhotonNetwork.CreateRoom(this.roomName, roomOptions);
    }

    public void JoinRoom()
    {
        this.roomName = this.joinInput.text.ToLower();
        this.isRoomCreated = false;
        this.SavePrefs();
        PhotonNetwork.JoinRoom(this.roomName);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined a room: {this.roomName}");
        PhotonNetwork.LoadLevel("SampleScene");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player: {newPlayer.UserId} has joined the room");
        GameObject.FindObjectOfType<BoardUI>().UpdatePlayerUI();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player: {otherPlayer.UserId} has left the room");
        GameObject.FindObjectOfType<BoardUI>().UpdatePlayerUI();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        errorMsg.text = message;
    }

    public void OnExit()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("LobbyScene");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("Master client changes, sync board and outs data to the new master");
        GameObject.FindObjectOfType<BoardGenerator>().SetBoardStateForNewMaster(newMasterClient);
    }


    public void OnQuit()
    {
        Debug.Log("Game is exiting");
        Application.Quit();
    }

    public static string GenerateRandomAlphaNumericStr(int desiredLength)
    {
        System.Text.StringBuilder codeSB = new System.Text.StringBuilder(""); // Requires @ top: using System.Text;
        char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        char singleChar;

        while (codeSB.Length < desiredLength)
        {
            singleChar = chars[UnityEngine.Random.Range(0, chars.Length)];
            codeSB.Append(singleChar);
        }

        Debug.Log("GenerateRandomAlphaNumericStr: " + codeSB.ToString());

        return codeSB.ToString();
    }
}

