using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;


public class BoardUI : MonoBehaviour
{
    [SerializeField] private Transform cam;
    [SerializeField] private BoardGenerator boardGenerator;
    public Text roomName;
    public Text winnerText;
    public GameObject player1Turn;
    public GameObject player2Turn;
    public GameObject resultLayout;
    public GameObject backgroundsContainer;
    public GameObject rowsContainer;
    public GameObject detailsContainer;
    public GameObject outsContainer;

    private void Start()
    {
        this.roomName.text = $"#Room: {PhotonNetwork.CurrentRoom.Name.ToUpper()}";
        if (PhotonNetwork.IsMasterClient)
        {
            this.player1Turn.transform.GetChild(0).GetComponent<Text>().text = $"Player 1: Playing";
            this.player2Turn.transform.GetChild(0).GetComponent<Text>().text = $"Player 2: Waiting";
        }
        else
        {
            this.player1Turn.transform.GetChild(0).GetComponent<Text>().text = $"Player 1: Playing";
            this.player2Turn.transform.GetChild(0).GetComponent<Text>().text = $"Player 2: Playing";
            this.UpdatePlayerTurn(1);
            this.RotateCamera();
        }
    }

    private void RotateCamera()
    {
        this.cam.transform.rotation *= Quaternion.Euler(0, 0, 180);
        this.player1Turn.transform.GetChild(0).transform.rotation *= Quaternion.Euler(180, 0, 0);
        this.player1Turn.transform.GetChild(0).GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
        this.player2Turn.transform.GetChild(0).transform.rotation *= Quaternion.Euler(180, 0, 0);
        this.player2Turn.transform.GetChild(0).GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
        this.detailsContainer.transform.rotation *= Quaternion.Euler(180, 0, 180);
        this.resultLayout.transform.rotation *= Quaternion.Euler(0, 0, 180);
        this.backgroundsContainer.transform.rotation *= Quaternion.Euler(0, 0, 180);
    }


    public void OnPlayerJoin(Player newPlayer)
    {

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {

        }
        else
        {
            this.player1Turn.transform.GetChild(0).GetComponent<Text>().text = $"Player 1: Playing";
            this.player2Turn.transform.GetChild(0).GetComponent<Text>().text = $"Player 2: Playing";
            this.UpdatePlayerTurn(1);
        }
    }


    public void UpdateWinnerText(string winner)
    {
        this.winnerText.text = $"{winner} wins...";
        this.resultLayout.SetActive(true);
        this.backgroundsContainer.SetActive(false);
        this.rowsContainer.SetActive(false);
        this.detailsContainer.SetActive(false);
        this.outsContainer.SetActive(false);
        for (int i = 0; i < this.boardGenerator.rows.Count; i++)
        {
            this.boardGenerator.rows[i].SetActive(false);
        }
        // foreach (KeyValuePair<Vector2, Tile> pair in this.tiles)
        // {
        //     pair.Value.gameObject.SetActive(false);
        // }
        // Clear any remaining tiles
        Tile[] ts = GameObject.FindObjectsOfType<Tile>();
        for (int i = 0; i < ts.Length; i++)
        {
            ts[i].gameObject.SetActive(false);
        }
    }

    public void UpdatePlayerTurn(int player)
    {
        if (player == 1)
        {
            this.player1Turn.transform.GetChild(1).GetComponent<Tile>().highlight.SetActive(false);
            this.player1Turn.transform.GetChild(1).GetComponent<Tile>().selected.SetActive(true);

            this.player2Turn.transform.GetChild(1).GetComponent<Tile>().highlight.SetActive(true);
            this.player2Turn.transform.GetChild(1).GetComponent<Tile>().selected.SetActive(false);
        }
        else
        {
            this.player2Turn.transform.GetChild(1).GetComponent<Tile>().highlight.SetActive(false);
            this.player2Turn.transform.GetChild(1).GetComponent<Tile>().selected.SetActive(true);

            this.player1Turn.transform.GetChild(1).GetComponent<Tile>().highlight.SetActive(true);
            this.player1Turn.transform.GetChild(1).GetComponent<Tile>().selected.SetActive(false);
        }

    }

}