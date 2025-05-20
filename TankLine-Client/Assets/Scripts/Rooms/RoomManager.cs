using UnityEngine;
using System.Collections.Generic;
using System;
using FishNet;
using FishNet.Connection;
using FishNet.Object;

public class RoomManager : NetworkBehaviour
{
  public static RoomManager Instance { get; private set; }
  private Dictionary<int, Room> rooms = new Dictionary<int, Room>();
  private Dictionary<NetworkConnection, int> playerRooms = new Dictionary<NetworkConnection, int>();


  private void Awake()
  {
    if (Instance == null)
      Instance = this;
    else
      Destroy(gameObject);
  }

  public override void OnStartServer()
  {
    base.OnStartServer();
  }

  [ServerRpc(RequireOwnership = false)]
  public void RequestJoinRoom(NetworkConnection conn, int roomId)
  {
    if (!rooms.ContainsKey(roomId))
    {
      rooms[roomId] = new Room(roomId);
    }

    Room room = rooms[roomId];
    room.AddPlayer(conn);

    playerRooms[conn] = roomId;
  }

  [ServerRpc(RequireOwnership = false)]
  public void RequestLeaveRoom(NetworkConnection conn)
  {
    if (playerRooms.TryGetValue(conn, out int roomId))
    {
      rooms[roomId].RemovePlayer(conn);
      playerRooms.Remove(conn);
    }
  }

  [TargetRpc]
  private void TargetConfirmRoomJoin(NetworkConnection conn, int roomId)
  {
    Debug.Log("You have joined room " + roomId);
  }
}
