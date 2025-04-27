using UnityEngine;
using UnityEngine.UI;
using FishNet;
using FishNet.Object;
using FishNet.Connection;
using System.Collections.Generic;
using TMPro;

public class RoomUIManager : MonoBehaviour
{
    public static RoomUIManager Instance { get; private set; }

    [Header("Buttons")]
    public Button CreateRoomBtn;
    public Button JoinRoomBtn;
    public Button returnBtn;

    [Header("Input Fields")]
    public TMP_InputField codeInput;

    [Header("Create Room Selection")]
    public Text selectedNumberText;
    public Text selectedModeText;
    public List<Button> numberButtons; // players per room
    public List<Button> modeButtons; // public/private

    private int selectedNumber = 2;
    private string selectedMode = "Public";

    private Color normalColor = Color.white;
    private Color selectedColor = Color.red;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        SetRoomButtonsInteractable(true);
        StartCoroutine(ConfirmClientConn());
        UpdateSelectionTexts();

        if (RoomManager.Instance != null)
        {
            NetworkObject netObj = RoomManager.Instance.GetComponent<NetworkObject>();

            if (InstanceFinder.IsServer)
            {
                if (netObj != null && !netObj.IsSpawned)
                {
                    Debug.Log("[RoomUI] Forcing RoomManager spawn.");

                    if (netObj.IsSceneObject)
                        InstanceFinder.ServerManager.Spawn(netObj);
                    else
                        InstanceFinder.ServerManager.Spawn(netObj);
                }
            }
        }
    }

    private void SetRoomButtonsInteractable(bool value)
    {
        CreateRoomBtn.interactable = value;
        JoinRoomBtn.interactable = value;
        //returnBtn.interactable = value;
    }

    private System.Collections.IEnumerator ConfirmClientConn()
    {
        yield return new WaitUntil(() =>
            InstanceFinder.IsClientStarted &&
            InstanceFinder.ClientManager.Connection.IsValid
        );
        Debug.Log("[RoomUI] Client connection confirmed.");
        
        yield return new WaitUntil(() =>
            RoomManager.Instance != null && RoomManager.Instance.isActiveAndEnabled
        );

        Debug.Log("[RoomUI] Client and RoomManager are ready.");

        

        SetRoomButtonsInteractable(true);

        CreateRoomBtn.onClick.AddListener(HandleCreateRoom);
        JoinRoomBtn.onClick.AddListener(HandleJoinRoom);
        //returnBtn.onClick.AddListener(HandleReturn);
        codeInput.onEndEdit.AddListener(HandlePrivateCodeEntered);
    }

    // === SELECTION BUTTONS ===

    public void SelectNumber(int number)
    {
        selectedNumber = number;
        UpdateSelectionTexts();
        UpdateButtonColors(numberButtons, number);
    }

    public void SelectMode(string mode)
    {
        selectedMode = mode;
        UpdateSelectionTexts();
        UpdateButtonColors(modeButtons, mode);
    }

    private void UpdateSelectionTexts()
    {
        if (selectedNumberText != null)
            selectedNumberText.text = "Selected Number: " + selectedNumber;
        if (selectedModeText != null)
            selectedModeText.text = "Selected Mode: " + selectedMode;
    }

    private void UpdateButtonColors<T>(List<Button> buttons, T selectedValue)
    {
        foreach (Button btn in buttons)
        {
            bool isSelected = btn.name == selectedValue.ToString();

            if (btn.image != null)
            {
                btn.image.color = isSelected ? selectedColor : normalColor;
            }
        }
    }

    // === ROOM CREATION ===

    private void HandleCreateRoom()
    {
        int maxPlayers = selectedNumber;
        bool isPublic = selectedMode == "Public";
        Debug.Log($"[RoomUI] Requesting room creation | Public: {isPublic} | Max Players: {maxPlayers}");

        RoomManager.Instance.RequestCreateRoom(maxPlayers, isPublic, InstanceFinder.ClientManager.Connection);
    }

    // === ROOM JOINING ===

    private void HandleJoinRoom()
    {
        bool isPublic = selectedMode == "Public";
        Debug.Log("JOINING");

        // if the room is public then:
        if (isPublic)
        {
            int? roomId = RoomManager.Instance.GetFirstAvaliablePublicRoom();

            if (roomId.HasValue)
            {
                Debug.Log($"[RoomUI] Joining public room {roomId.Value}");
                RoomManager.Instance.RequestJoinRoom(roomId.Value, InstanceFinder.ClientManager.Connection);
            }
            else
            {
                Debug.Log("[RoomUI] No public rooms available.");
                HandleCreateRoom();
            }
        }

        // if the room is private then:
        else
        {
            if (IsValidRoomCode(codeInput.text, out int roomId))
            {
                Debug.Log($"[RoomUI] Joining private room {roomId}");
                RoomManager.Instance.RequestJoinRoom(roomId, InstanceFinder.ClientManager.Connection);
            }
            else
            {
                Debug.LogWarning("[RoomUI] Invalid room code.");
            }
        }
    }

    private void HandlePrivateCodeEntered(string input)
    {
        if (!IsValidRoomCode(input, out int roomId))
        {
            Debug.LogWarning("[RoomUI] Invalid room code entered.");
        }
    }

    private bool IsValidRoomCode(string input, out int roomId)
    {
        input = input.Trim();

        if (input.Length == 6 && int.TryParse(input, out roomId))
            return true;

        roomId = -1;
        return false;
    }

    private void HandleReturn()
    {
        Debug.Log("[RoomUI] Return button clicked. Returning to main menu.");
    }
}
