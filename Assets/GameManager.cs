using UnityEngine;
using Net;
using Utils;
using System.Reflection;
using Services;

public class GameManager : MonoSingleton<GameManager>, INetworkHandler
{
    public GameObject ShootLine;

    public GameObject ReloadSound;

    protected override void Init()
    {

    }

    private void Start()
    {
        GameNetwork.Instance.Connect(this, Assembly.GetExecutingAssembly(), "192.168.0.141", 8888);
    }

    #region NetworkHandler

    // Callback when the player (as client) was able to connect to a server.
    public void OnConnected()
    {
        Debug.Log("Connected as client");
    }

    public void OnHost()
    {
        Debug.Log("Connected as host");
        GameNetwork.Instance.Connect(this, Assembly.GetExecutingAssembly(), "192.168.0.141", 8888);
    }

    // Callback when a player (as client) failed to connect to a server.
    public void OnConnectionFailed()
    {
        GameNetwork.Instance.Host<ServerGameService>(this, 8888);
    }


    // Callback when a player (as server) failed to create a server.
    public void OnHostFailed()
    {
        Debug.LogError("Failed to host a server.");
        // Send info to master server
    }

    #endregion

    protected override bool OverrideInstance()
    {
        return false;
    }
}
