using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    public InputField playerName;
    public InputField createInput;
    public InputField joinInput;

    public void CreateRoom()
    {
        PhotonNetwork.NickName = playerName.text;
        PhotonNetwork.CreateRoom(createInput.text);       
    }

    public void JoinRoom()
    {
        PhotonNetwork.NickName = playerName.text;
        PhotonNetwork.JoinRoom(joinInput.text);       
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Game");
    }

    public void JoinOrCreateButton()
    {
        if (joinInput.text.Length < 1)
        return;
        PhotonNetwork.NickName = playerName.text;
        RoomOptions options = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.JoinOrCreateRoom(joinInput.text, options, default);

    }

}
