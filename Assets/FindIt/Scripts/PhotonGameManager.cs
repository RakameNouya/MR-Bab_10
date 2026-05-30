using Photon.Pun;
using TMPro;
using UnityEngine;

public class PhotonGameManager : MonoBehaviourPunCallbacks
{
    public override void OnJoinedRoom()
    {
        var avatar = PhotonNetwork.Instantiate("PlayerAvatar", Vector3.zero, Quaternion.identity);

        // Wire scene reference that cannot be stored in the prefab asset
        var tc = avatar.GetComponent<TreasureClick>();
        if (tc != null)
        {
            var countGO = GameObject.Find("TreasureCountText");
            if (countGO != null)
                tc.CountTreasureText = countGO.GetComponent<TMP_Text>();
        }
    }
}
