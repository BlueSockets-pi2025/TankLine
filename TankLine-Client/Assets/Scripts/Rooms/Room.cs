using UnityEngine;
using System.Collections.Generic;
using FishNet.Connection;
using System;

public class Room
{
  public int RoomId { get; private set; }
  public bool IsPublic { get; private set; }
  public int PlayerCount => players.Count;
  public bool IsFull => PlayerCount >= maxPlayersPerRoom;
  public NetworkConnection Host { get; private set; }


  private List<NetworkConnection> players = new List<NetworkConnection>();
  private int maxPlayersPerRoom;

  // Event triggered when a player joins the room. Useful for UI messages or other notifications.
  public event Action<NetworkConnection> OnPlayerJoined;
  public event Action<NetworkConnection> OnPlayerLeft;

  public Room(int roomId, bool isPublic = true, int maxPlayersPerRoom = 6)
  {
    RoomId = roomId;
    IsPublic = isPublic;
    this.maxPlayersPerRoom = maxPlayersPerRoom;
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
      if (players.Count == 1)
        Host = conn; // The first player to join becomes the host.

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

  public IReadOnlyList<NetworkConnection> GetPlayers() => players.AsReadOnly();

  public bool IsEmpty() => players.Count == 0;

  public bool HasPlayer(NetworkConnection conn) => players.Contains(conn);
  
  public bool IsHost(NetworkConnection conn) => Host == conn;
}
