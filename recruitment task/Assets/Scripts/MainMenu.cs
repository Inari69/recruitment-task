using System;
using System.Linq;
using TMPro;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button hostGameButton;
    [SerializeField] private Button joinGameButton;
    [SerializeField] private Button insertNicknameButton;
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject playerList;
    [SerializeField] private GameObject insertNicknameWindow;
    [SerializeField] private GameObject nicknameText;
    [SerializeField] private GameObject chatInputTextGameObject;
    [SerializeField] private GameObject chatOutputTextGameObject;
    [SerializeField] private GameObject chatWindow;

    [SerializeField] private GameObject players;
    [SerializeField] private GameObject playerListItemPrefab;
    private int _chatTargetId = -1;
    private bool _isHost;
    private InputSystem_Actions _inputActions;

    private void Awake()
    {
        //menu.SetActive(true);
        hostGameButton.onClick.AddListener(() => OpenInsertNicknameWindow(true));
        joinGameButton.onClick.AddListener(() => OpenInsertNicknameWindow(false));
        insertNicknameButton.onClick.AddListener(OnNicknameConfirmed);
        _inputActions = new InputSystem_Actions();
    }

    private void OnNicknameConfirmed()
    {
        string nickname = nicknameText.GetComponent<TMP_Text>().text;
        PlayerDataBridge.Nickname = nickname;

        if (_isHost)
        {
            StartServer(nickname);
        }
        else
        {
            JoinGame(nickname);
        }
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        _inputActions.UI.Scoreboard.performed += OnScoreboardPressed;
    }

    private void OnDisable()
    {
        _inputActions.UI.Scoreboard.performed -= OnScoreboardPressed;
        _inputActions.Disable();
    }

    private void ClearPlayerList()
    {
        for (int i = 1; i < players.transform.childCount; i++)
        {
            Destroy(players.transform.GetChild(i).gameObject);
        }
    }

    public void AddPlayerToList(string name, int Id)
    {
        GameObject player = Instantiate(playerListItemPrefab, players.transform);
        player.GetComponent<PlayerListItem>().SetNickname(name);
        player.GetComponent<PlayerListItem>().SetNetworkId(Id);
    }

    public void SetChatTarget(int networkId)
    {
        _chatTargetId = networkId;
        Debug.Log($"SetChatTarget {_chatTargetId}");
    }

    public void SendChat()
    {
        var world = World.DefaultGameObjectInjectionWorld;

        if (world == null)
        {
            Debug.LogError("No client world!");
            return;
        }

        var em = world.EntityManager;

        Entity rpc = em.CreateEntity();

        var msg = new ChatMessageRpc
        {
            SenderNickname =  nicknameText.GetComponent<TMP_Text>().text,
            TargetID = _chatTargetId,
            Message = chatInputTextGameObject.GetComponent<TMP_Text>().text
        };

        em.AddComponentData(rpc, msg);
        em.AddComponentData(rpc, new SendRpcCommandRequest());

        Debug.Log("Chat RPC sent");
    }

    public void UpdateChatOutput(string senderNickname, string message, int receiverId)
    {
        chatOutputTextGameObject.GetComponent<TMP_Text>().text += "[" + senderNickname + "] -> [" + (receiverId == -1 ? "Everyone" : nicknameText.GetComponent<TMP_Text>().text) + "]: " + message + "\n";
    }

    private void OnScoreboardPressed(InputAction.CallbackContext context)
    {
        TogglePlayerList();
        //RequestPlayerList();
    }
    
    public static void RequestPlayerList()
    {
        World clientWorld = null;

        foreach (var world in World.All)
        {
            if (world.Flags == WorldFlags.GameClient)
            {
                clientWorld = world;
                break;
            }
        }

        if (clientWorld == null)
        {
            Debug.LogError("ClientWorld not found!");
            return;
        }

        var em = clientWorld.EntityManager;

        Entity rpc = em.CreateEntity();
        em.AddComponentData(rpc, new RequestPlayerListRpc());
        em.AddComponentData(rpc, new SendRpcCommandRequest());

        Debug.Log("RequestPlayerList SENT");
    }

    private void OpenInsertNicknameWindow(bool hostGame)
    {
        _isHost = hostGame;
        hostGameButton.gameObject.SetActive(false);
        joinGameButton.gameObject.SetActive(false);
        insertNicknameWindow.SetActive(true);
    }
    
    private void JoinGame(string nickname)
    {
        World clientWorld = ClientServerBootstrap.CreateClientWorld("ClientWorld");

        foreach (World world in World.All)
        {
            if (world.Flags == WorldFlags.Game)
            {
                world.Dispose();
                break;
            }
        }

        if (World.DefaultGameObjectInjectionWorld == null)
        {
            World.DefaultGameObjectInjectionWorld = clientWorld;
        }

        SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Additive);

        ushort port = 7979;
        string ip = "127.0.0.1";

        NetworkEndpoint connectNetworkEndpoint = NetworkEndpoint.Parse(ip, port);
        RefRW<NetworkStreamDriver> networkStreamDriver = clientWorld.EntityManager.CreateEntityQuery(typeof(NetworkStreamDriver)).GetSingletonRW<NetworkStreamDriver>();
        networkStreamDriver.ValueRW.Connect(clientWorld.EntityManager, connectNetworkEndpoint);
        
        menu.SetActive(false);
    }

    private void TogglePlayerList()
    {
        if (playerList.activeSelf == true)
        {
            playerList.SetActive(false);
        }
        else
        {
            playerList.SetActive(true);
            ClearPlayerList();
            RequestPlayerList();
        }
    }

    private void StartServer(string nickname)
    {
        World serverWorld = ClientServerBootstrap.CreateServerWorld("ServerWorld");
        World clientWorld = ClientServerBootstrap.CreateClientWorld("ClientWorld");

        foreach (World world in World.All)
        {
            if (world.Flags == WorldFlags.Game)
            {
                world.Dispose();
                break;
            }
        }

        if (World.DefaultGameObjectInjectionWorld == null)
        {
            World.DefaultGameObjectInjectionWorld = serverWorld;
        }

        SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Additive);

        ushort port = 7979;

        RefRW<NetworkStreamDriver> networkStreamDriver = serverWorld.EntityManager
            .CreateEntityQuery(typeof(NetworkStreamDriver))
            .GetSingletonRW<NetworkStreamDriver>();

        networkStreamDriver.ValueRW.Listen(NetworkEndpoint.AnyIpv4.WithPort(port));

        networkStreamDriver = clientWorld.EntityManager
            .CreateEntityQuery(typeof(NetworkStreamDriver))
            .GetSingletonRW<NetworkStreamDriver>();

        networkStreamDriver.ValueRW.Connect(clientWorld.EntityManager, NetworkEndpoint.LoopbackIpv4.WithPort(port));
        
        menu.SetActive(false);
        chatWindow.SetActive(true);
    }
}
