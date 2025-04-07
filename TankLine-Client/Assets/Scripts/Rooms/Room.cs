using UnityEngine;
using System.Collections.Generic;
using FishNet.Connection;
using System;

public class Room
{
  public int RoomId { get; private set; }
  private List<NetworkConnection> players = new List<NetworkConnection>();
  private const int maxPlayersPerRoom = 6;
  public bool IsPublic { get; private set; }

  public int PlayerCount => players.Count;
  public bool IsFull => PlayerCount >= maxPlayersPerRoom;

  // Event triggered when a player joins the room. Useful for UI messages or other notifications.
  public event Action<NetworkConnection> OnPlayerJoined;
  public event Action<NetworkConnection> OnPlayerLeft;

  public Room (int roomId, bool isPublic = true)
  {
    RoomId = roomId;
    IsPublic = isPublic;
  }

  /// <summary>
  /// Add a player to the room. 
  /// </summary>
  /// <param name="conn"></param>
  public void AddPlayer(NetworkConnection conn)
  {
    if (!players.Contains(conn))
    {
      players.Add(conn);
      OnPlayerJoined?.Invoke(conn);
    }
  }

  /// <summary>
  /// Remove a player from the room. 
  /// </summary>
  /// <param name="conn"></param>
  public void RemovePlayer(NetworkConnection conn)
  {
    if (players.Remove(conn))
    {
      OnPlayerLeft?.Invoke(conn);
    }
  }

  /// <summary>
  /// Get the list of players in the room.
  /// </summary>
  /// <returns></returns>
  public IReadOnlyList<NetworkConnection> GetPlayers()
  {
    return players.AsReadOnly();
  }

  /// <summary>
  /// Check if the room is empty.
  /// </summary>
  /// <returns></returns>
  public bool IsEmpty()
  {
    return players.Count == 0;
  }
}
