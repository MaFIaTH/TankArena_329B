using System.Threading.Tasks;
using UnityEngine;

public class ClientSingleton : MonoBehaviour
{
    private static ClientSingleton _instance;

    public ClientGameManager GameManager { get; private set; }

    public static ClientSingleton Instance
    {
        get
        {
            if (_instance != null) return _instance;
            _instance = FindFirstObjectByType<ClientSingleton>();
            if (_instance != null) return _instance;
            //Debug.Log($"No singleton {_instance.GetType()} in scene!");
            return null;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public async Task<bool> CreateClient()
    {
        GameManager = new ClientGameManager();
        return await GameManager.InitAsync();
    }
    
    private void OnDestroy()
    {
        GameManager?.Dispose();
    }
}
