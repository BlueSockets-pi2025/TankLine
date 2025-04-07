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

  public override void OnStopServer()
  {
    base.OnStopServer();
    rooms.Clear();
    playerRooms.Clear();
  }

  /// <summary>
  /// Request to join a room. This method is called on the client when a player wants to join a room.
  /// </summary>
  /// <param name="roomId"></param>
  /// <param name="conn"></param>
  [ServerRpc(RequireOwnership = false)]
  public void RequestJoinRoom (int roomId, NetworkConnection conn = null)
  {
    JoinRoom(conn, roomId);
  }

  /// <summary>
  /// Join the room. This method is called on the server when a player joins a room.
  /// </summary>
  /// <param name="conn"></param>
  /// <param name="roomId"></param>
  public void JoinRoom(NetworkConnection conn, int roomId)
  {
    if (!rooms.ContainsKey(roomId))
    {
      rooms[roomId] = new Room(roomId, true);
    }

    Room room = rooms[roomId];
    room.AddPlayer(conn);
    playerRooms[conn] = roomId;

    Debug.Log($"[RoomManager] {conn.ClientId} joined room {roomId}");

    TargetConfirmRoomJoin(conn, roomId);
  }

  /// <summary>
  /// Request to leave a room. This method is called on the client when a player wants to leave a room.
  /// </summary>
  /// <param name="conn"></param>
  [ServerRpc(RequireOwnership = false)]
  public void RequestLeaveRoom (NetworkConnection conn = null)
  {
    LeaveRoom(conn);
  }

  /// <summary>
  /// Leave the room. This method is called on the server when a player leaves a room.
  /// </summary>
  /// <param name="conn"></param>
  public void LeaveRoom(NetworkConnection conn)
  {
    if (playerRooms.TryGetValue(conn, out int roomId))
    {
      rooms[roomId].RemovePlayer(conn);
      playerRooms.Remove(conn);

      if (rooms[roomId].IsEmpty())
      {
        rooms.Remove(roomId);
      }

      Debug.Log($"[RoomManager] {conn.ClientId} left room {roomId}");
    }
  }

  /// <summary>
  /// Confirmation of room join. This method is called on the client to confirm that the player has joined the room.
  /// </summary>
  /// <param name="conn"></param>
  /// <param name="roomId"></param>
  [TargetRpc]
  private void TargetConfirmRoomJoin(NetworkConnection conn, int roomId)
  {
    Debug.Log("You have joined room " + roomId);
  }

  public bool CanJoinRoom(int roomId)
  {
    return !rooms.ContainsKey(roomId) || rooms[roomId].IsFull == false;
  }

  public int? GetFirstAvaliablePublicRoom()
  {
    foreach (var kvp in rooms)
    {
      var room = kvp.Value;
      if (room.IsPublic && !room.IsFull)
      {
        return room.RoomId;
      }
    }
    return null;  
  }
}
