using System.Linq;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button hostGameButton;
    [SerializeField] private Button joinGameButton;
    [SerializeField] private GameObject mainMenu;

    private void Awake()
    {
        hostGameButton.onClick.AddListener(StartServer);
        joinGameButton.onClick.AddListener(JoinGame);
    }

    private void JoinGame()
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

        SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Single);

        ushort port = 7979;
        string ip = "127.0.0.1";

        NetworkEndpoint connectNetworkEndpoint = NetworkEndpoint.Parse(ip, port);
        RefRW<NetworkStreamDriver> networkStreamDriver = clientWorld.EntityManager.CreateEntityQuery(typeof(NetworkStreamDriver)).GetSingletonRW<NetworkStreamDriver>();
        networkStreamDriver.ValueRW.Connect(clientWorld.EntityManager, connectNetworkEndpoint);
        
        //mainMenu.SetActive(false);
    }

    private void StartServer()
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

        SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Single);

        ushort port = 7979;

        RefRW<NetworkStreamDriver> networkStreamDriver = serverWorld.EntityManager
            .CreateEntityQuery(typeof(NetworkStreamDriver))
            .GetSingletonRW<NetworkStreamDriver>();

        networkStreamDriver.ValueRW.Listen(NetworkEndpoint.AnyIpv4.WithPort(port));

        networkStreamDriver = clientWorld.EntityManager
            .CreateEntityQuery(typeof(NetworkStreamDriver))
            .GetSingletonRW<NetworkStreamDriver>();

        networkStreamDriver.ValueRW.Connect(clientWorld.EntityManager, NetworkEndpoint.LoopbackIpv4.WithPort(port));
        
        //mainMenu.SetActive(false);
    }
}
