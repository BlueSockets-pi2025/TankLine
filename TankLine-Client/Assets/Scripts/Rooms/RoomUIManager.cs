using UnityEngine;
using UnityEngine.UI;
using FishNet;
using FishNet.Object;
using FishNet.Connection;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;

public class RoomUIManager : MonoBehaviour
{
    public static RoomUIManager Instance { get; private set; }

    [Header("Buttons")]
    public Button CreateRoomBtn;
    public Button JoinRoomBtn;
    public Button MatchmakingBtn;

    [Header("Input Fields")]
    public TMP_InputField codeInput;

    [Header("Create Room Selection")]
    public Text selectedNumberText;
    public Text selectedModeText;
    public List<Button> numberButtons; // players per room
    public List<Button> modeButtons; // public/private

    [Header("Room Manager")]
    private RoomManager spawnedRoomManager;

    private int selectedNumber = 2;
    private string selectedMode = "Public";

    private Color normalColor = Color.white;
    private Color selectedColor = Color.red;

    private string isDedicatedServer = Environment.GetEnvironmentVariable("IS_DEDICATED_SERVER");

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        StartCoroutine(EnsureRoomManagerAndClientReady());
        UpdateSelectionTexts();
        Debug.Log("[RoomUI] Room UI initialized.");
    }

    private IEnumerator EnsureRoomManagerAndClientReady()
    {
        Debug.Log("[RoomUI] Coroutine started");
        // Wait for network client to be ready
        yield return new WaitUntil(() =>
            InstanceFinder.IsClientStarted &&
            InstanceFinder.ClientManager.Connection != null &&
            InstanceFinder.ClientManager.Connection.IsValid
        );
        Debug.Log("[RoomUI] Client is ready.");

        // Wait until the manager is properly spawned
        yield return new WaitUntil(() =>
            RoomManager.Instance != null && RoomManager.Instance.isActiveAndEnabled
        );

        spawnedRoomManager = RoomManager.Instance;

        CreateRoomBtn.onClick.AddListener(HandleCreateRoom);
        JoinRoomBtn.onClick.AddListener(HandleJoinRoom);
        MatchmakingBtn.onClick.AddListener(HandleMatchmaking);
        codeInput.onEndEdit.AddListener(HandlePrivateCodeEntered);

        Debug.Log($"[RoomUI] ClientStarted: {InstanceFinder.IsClientStarted}");
        Debug.Log($"[RoomUI] ClientManager exists: {InstanceFinder.ClientManager != null}");
        Debug.Log($"[RoomUI] Connection exists: {InstanceFinder.ClientManager.Connection != null}");
        Debug.Log($"[RoomUI] Connection is valid: {InstanceFinder.ClientManager.Connection?.IsValid}");

        Debug.Log("[RoomUI] RoomManager is ready.");
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
        if (isDedicatedServer == "true") return;

        int maxPlayers = selectedNumber;
        bool isPublic = selectedMode == "Public";
        Debug.Log($"[RoomUI] Requesting room creation | Public: {isPublic} | Max Players: {maxPlayers}");

        spawnedRoomManager.RequestCreateRoom(maxPlayers, isPublic, InstanceFinder.ClientManager.Connection);
    }

    // === ROOM JOINING ===

    private void HandleMatchmaking()
    {
        if (isDedicatedServer == "true") return;

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

    private void HandleJoinRoom()
    {
        if (isDedicatedServer == "true") return;
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
