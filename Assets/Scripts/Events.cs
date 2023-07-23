using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
public static class Events
{
    public static readonly byte UpdateColorEvent = 0;
    public static readonly byte TogglePlayerTurnEvent = 1;
    public static readonly byte CheckWinnerEvent = 2;
    public static readonly byte RemoveTileEvent = 3;


    public static void RaiseEventToAll(byte evCode, object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, SendOptions.SendReliable);
    }

}