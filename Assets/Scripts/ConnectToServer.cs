using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine.UI;
public class ConnectToServer : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    bool isLoading = false;
    [SerializeField] private Text loadAmountText;
    [SerializeField] private Slider progressSlider;

    void Start()
    {
        Debug.Log("Connecting to server...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected. Try joining a lobby...");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined a lobby");
        StartCoroutine(LoadLevelAsync());
    }

    IEnumerator LoadLevelAsync()
    {
        PhotonNetwork.LoadLevel("LobbyScene");

        while (PhotonNetwork.LevelLoadingProgress < 1)
        {
            this.loadAmountText.text = "Loading: %" + (int)(PhotonNetwork.LevelLoadingProgress * 100);
            //loadAmount = async.progress;
            this.progressSlider.value = PhotonNetwork.LevelLoadingProgress;
            yield return new WaitForEndOfFrame();
        }
    }
}
