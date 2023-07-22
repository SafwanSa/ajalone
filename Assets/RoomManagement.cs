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

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
    public void CreateRoom()
    {

        this.roomName = GenerateRandomAlphaNumericStr(4).ToLower();
        this.isRoomCreated = true;
        PhotonNetwork.CreateRoom(this.roomName);
    }

    public void JoinRoom()
    {
        this.roomName = this.joinInput.text.ToLower();
        this.isRoomCreated = false;
        PhotonNetwork.JoinRoom(this.joinInput.text);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined a room: {this.roomName}");
        PhotonNetwork.LoadLevel("SampleScene");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        GameObject.FindObjectOfType<GridManager>().OnPlayerJoin();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        errorMsg.text = message;
    }

    public void DestroyScene()
    {
        Destroy(this);
    }

    public void OnQuit()
    {
        Debug.Log("Game is exiting");
        Application.Quit();
    }

    public static string GenerateRandomAlphaNumericStr(int desiredLength)
    {
        System.Text.StringBuilder codeSB = new System.Text.StringBuilder(""); // Requires @ top: using System.Text;
        char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
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

