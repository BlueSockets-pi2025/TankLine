using UnityEngine;
using UnityEngine.UI;
using FishNet;
using FishNet.Connection;

public class RoomUIManager : MonoBehaviour
{
  [Header("Buttons")]
  public Button CreateRoomBtn;
  public Button JoinRoomBtn;
  public Button publicBtn;
  public Button privateBtn;
  public Button returnBtn;
  public Button okBtn;

  [Header("Input Fields")]
  public InputField codeInput;

  private bool joiningPrivate = false;

  void Start()
  {
    CreateRoomBtn.onClick.AddListener(OnCreateRoomClicked);
    JoinRoomBtn.onClick.AddListener(OnJoinRoomClicked);
    publicBtn.onClick.AddListener(OnPublicBtnClicked);
    privateBtn.onClick.AddListener(OnPrivateBtnClicked);
    returnBtn.onClick.AddListener(OnReturnBtnClicked);
    okBtn.onClick.AddListener(OnOkBtnClicked);
  }

  void OnCreateRoomClicked()
  {
    int newRoomId = Random.Range(1000, 9999);
    RoomManager.Instance.RequestJoinRoom(newRoomId, InstanceFinder.ClientManager.Connection);
    Debug.Log("Room created with ID: " + newRoomId);
  }

  void OnJoinRoomClicked()
  {
    Debug.Log("Choose public or private room to join.");
  }

  void OnPublicBtnClicked()
  {
    var conn = InstanceFinder.ClientManager.Connection;
    int? availableRoom = RoomManager.Instance.GetFirstAvaliablePublicRoom();

    if (availableRoom.HasValue)
    {
      RoomManager.Instance.RequestJoinRoom(availableRoom.Value, conn);  
    }
    else
    {
      Debug.Log("No available public rooms, Creating one...");
      int newRoomId = Random.Range(1000, 9999);
      RoomManager.Instance.RequestJoinRoom(newRoomId, conn);
    }
  }

  void OnPrivateBtnClicked()
  {
    joiningPrivate = true;
    Debug.Log("Joining private room, please enter the room code.");
  }

  void OnOkBtnClicked()
  {
    if (joiningPrivate && int.TryParse(codeInput.text, out int roomId))
    {
      RoomManager.Instance.RequestJoinRoom(roomId, InstanceFinder.ClientManager.Connection);
      joiningPrivate = false;
    }
    else
    {
      Debug.Log("Invalid room code. Please try again.");
    }
  }

  void OnReturnBtnClicked()
  {
    Debug.Log("Returning to main menu.");
    // Implement logic to return to the main menu here.
  }
}
