using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonRoomManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom(
            "FindIt_Room_1",
            new RoomOptions { MaxPlayers = 10 },
            TypedLobby.Default);
    }
}
