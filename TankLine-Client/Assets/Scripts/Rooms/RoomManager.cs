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

  public event Action<NetworkConnection, int> OnRoomCreated;
  
  private void Awake()
  {
    if (Instance == null)
      Instance = this;
    else
      Destroy(gameObject);
  }

  public override void OnStopServer()
  {
    base.OnStopServer();
    rooms.Clear();
    playerRooms.Clear();
  }

  [ServerRpc(RequireOwnership = false)]
  public void RequestCreateRoom (int maxPlayers, bool isPublic, NetworkConnection conn = null)
  {
    int roomId = GenerateUniqueRoomId();
    CreateRoom(conn, roomId, maxPlayers, isPublic);
  }

  private void CreateRoom(NetworkConnection conn, int roomId, int maxPlayers, bool isPublic)
  {
    if (rooms.ContainsKey(roomId))
    {
      TargetJoinFailed(conn, "Room already exists.");
      return;
    }

    Room room = new Room(roomId, isPublic, maxPlayers);
    rooms[roomId] = room;
    Debug.Log($"[RoomManager] Created room {roomId} | Public: {isPublic} | Max Players: {maxPlayers}");

    OnRoomCreated?.Invoke(conn, roomId);
    JoinRoom(conn, roomId);
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
  private void JoinRoom(NetworkConnection conn, int roomId)
  {
    if (!rooms.ContainsKey(roomId))
    {
      TargetJoinFailed(conn, "Room does not exist.");
      return;
    }

    if (playerRooms.ContainsKey(conn))
    {
      TargetJoinFailed(conn, "You are already in a room.");
      return;
    }

    Room room = rooms[roomId];

    if (room.IsFull)
    {
      TargetJoinFailed(conn, "Room is full.");
      return;
    }

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
        Debug.Log($"[RoomManager] Room {roomId} is empty and has been removed.");
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

  [TargetRpc]
  private void TargetJoinFailed(NetworkConnection conn, string message)
  {
    Debug.LogWarning($"[RoomManager] Join failed: {message}");
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

  private int GenerateUniqueRoomId()
  {
    int roomId;
    do
    {
      roomId = UnityEngine.Random.Range(100000, 999999);
    } while (rooms.ContainsKey(roomId));
    return roomId;
  }
}
