using Photon.Pun;
using UnityEngine;

public class PhotonGameManager : MonoBehaviourPunCallbacks
{
    public override void OnJoinedRoom()
    {
        PhotonNetwork.Instantiate("PlayerAvatar", Vector3.zero, Quaternion.identity);
        // CountTreasureText wiring removed — CountdownManager.Instance handles all score UI.
    }
}
