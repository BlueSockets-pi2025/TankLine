using UnityEngine;
using System.Collections.Generic;
using FishNet.Connection;

public class Room
{
  public int RoomId { get; private set; }
  private List<NetworkConnection> players = new List<NetworkConnection>();

  public int PlayerCount => players.Count;

  public Room (int roomId)
  {
    RoomId = roomId;
  }

  public void AddPlayer(NetworkConnection conn)
  {
    if (!players.Contains(conn))
    {
      players.Add(conn);
      
    }
  }

  public void RemovePlayer(NetworkConnection conn)
  {
    if (players.Contains(conn))
    {
      players.Remove(conn);
    }
  }

  public List<NetworkConnection> GetPlayers()
  {
    return players;
  }
}
