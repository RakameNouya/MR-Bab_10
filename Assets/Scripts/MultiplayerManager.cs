using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class MultiplayerManager : MonoBehaviourPunCallbacks
{
    public static MultiplayerManager Instance { get; private set; }

    [Header("Player Avatar Prefab")]
    public string avatarPrefabName = "PlayerAvatar";

    [Header("Debug UI")]
    public TextMeshProUGUI roomInfoText;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        PhotonNetwork.OfflineMode = false;
        PhotonNetwork.AutomaticallySyncScene = true;
        Connect();
    }

    void Connect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("[MP] Connecting to Photon...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[MP] Connected. Joining room...");
        var opts = new RoomOptions { MaxPlayers = 10, IsVisible = true, IsOpen = true };
        PhotonNetwork.JoinOrCreateRoom("FindIt_Room_1", opts, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[MP] Joined room: " + PhotonNetwork.CurrentRoom.Name);
        UpdateRoomInfo();

        Vector3 spawnPos = new Vector3(PhotonNetwork.LocalPlayer.ActorNumber * 0.5f, 0, 0);
        if (Resources.Load(avatarPrefabName) != null)
            PhotonNetwork.Instantiate(avatarPrefabName, spawnPos, Quaternion.identity);
        else
            Debug.LogWarning("[MP] PlayerAvatar prefab not found in Resources. Skipping spawn.");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("[MP] Player joined: " + newPlayer.NickName);
        UpdateRoomInfo();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("[MP] Player left: " + otherPlayer.NickName);
        UpdateRoomInfo();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("[MP] Disconnected: " + cause);
        if (cause != DisconnectCause.ApplicationQuit)
            Invoke(nameof(Connect), 3f);
    }

    void UpdateRoomInfo()
    {
        if (roomInfoText == null) return;
        var room = PhotonNetwork.CurrentRoom;
        if (room != null)
            roomInfoText.text = string.Format("Room: {0}  |  Players: {1}/{2}",
                room.Name, room.PlayerCount, room.MaxPlayers);
    }
}
