using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] private ClientSingleton clientPrefab;
    [SerializeField] private HostSingleton hostPrefab;
    [SerializeField] private NetworkObject playerPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async void Start()
    {
        DontDestroyOnLoad(gameObject);
        await LaunchInMode(SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null);
    }

    private async Task LaunchInMode(bool isDedicatedServer)
    {
        if (isDedicatedServer)
        {
            
        }
        else
        {
            HostSingleton hostSingleton = Instantiate(hostPrefab);
            hostSingleton.CreateHost(playerPrefab);
            
            ClientSingleton clientSingleton = Instantiate(clientPrefab);
            bool authenticated = await clientSingleton.CreateClient();
            
            if (authenticated)
            {
                clientSingleton.GameManager.GoToMenu();
            }
        }
    }
}
