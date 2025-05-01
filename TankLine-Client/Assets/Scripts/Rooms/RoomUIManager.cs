using UnityEngine;
using UnityEngine.UI;
using FishNet;
using FishNet.Object;
using FishNet.Connection;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class RoomUIManager : MonoBehaviour
{
    public static RoomUIManager Instance { get; private set; }

    [Header("Buttons")]
    public Button CreateRoomBtn;
    public Button JoinRoomBtn;

    [Header("Input Fields")]
    public TMP_InputField codeInput;

    [Header("Create Room Selection")]
    public Text selectedNumberText;
    public Text selectedModeText;
    public List<Button> numberButtons; // players per room
    public List<Button> modeButtons; // public/private

    [Header("Room Manager")]
    [SerializeField] private RoomManager roomManagerPrefab;
    private RoomManager spawnedRoomManager;

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
        StartCoroutine(ConfirmClientConn());
        UpdateSelectionTexts();

        CreateRoomBtn.onClick.AddListener(HandleCreateRoom);
        JoinRoomBtn.onClick.AddListener(HandleJoinRoom);
        codeInput.onEndEdit.AddListener(HandlePrivateCodeEntered);
        Debug.Log("[RoomUI] Room UI initialized.");
    }

    private IEnumerator ConfirmClientConn()
    {
        yield return new WaitUntil(() =>
            InstanceFinder.IsClientStarted &&
            InstanceFinder.ClientManager.Connection.IsValid
        );
        Debug.Log("[RoomUI] Client connection confirmed.");
        
        yield return new WaitUntil(() =>
            spawnedRoomManager != null && spawnedRoomManager.isActiveAndEnabled
        );

        Debug.Log("[RoomUI] Client and RoomManager are ready.");
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
        if (!InstanceFinder.IsServer)
        {
            Debug.LogError("[RoomUI] Cannot create room. Not a server instance.");
            return;
        }

        if (spawnedRoomManager == null)
        {
            NetworkObject roomObj = Instantiate(roomManagerPrefab).GetComponent<NetworkObject>();
            InstanceFinder.ServerManager.Spawn(roomObj);
            spawnedRoomManager = roomObj.GetComponent<RoomManager>();
            Debug.Log("[RoomUI] RoomManager spawned.");
        }

        int maxPlayers = selectedNumber;
        bool isPublic = selectedMode == "Public";
        Debug.Log($"[RoomUI] Requesting room creation | Public: {isPublic} | Max Players: {maxPlayers}");

        spawnedRoomManager.RequestCreateRoom(maxPlayers, isPublic, InstanceFinder.ClientManager.Connection);
    }

    // === ROOM JOINING ===

    private void HandleJoinRoom()
    {
        bool isPublic = selectedMode == "Public";
        Debug.Log("JOINING");

        // if the room is public then:
        if (isPublic)
        {
            int? roomId = spawnedRoomManager.GetFirstAvaliablePublicRoom();

            if (roomId.HasValue)
            {
                Debug.Log($"[RoomUI] Joining public room {roomId.Value}");
                spawnedRoomManager.RequestJoinRoom(roomId.Value, InstanceFinder.ClientManager.Connection);
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
                spawnedRoomManager.RequestJoinRoom(roomId, InstanceFinder.ClientManager.Connection);
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
