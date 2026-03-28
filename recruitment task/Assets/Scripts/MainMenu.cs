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

    private void Update()
    {
        
    }

    private void OnScoreboardPressed(InputAction.CallbackContext context)
    {
        TogglePlayerList();
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
        playerList.SetActive(!playerList.activeSelf);
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
    }
}
