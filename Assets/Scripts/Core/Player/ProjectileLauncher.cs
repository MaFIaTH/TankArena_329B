using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private CoinWallet coinWallet;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private GameObject clientProjectilePrefab;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private Collider2D playerCollider;

    [Header("Settings")] 
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float muzzleFlashDuration;
    [SerializeField] private int costToFire;

    private bool _isPointerOverUI;
    private bool _shouldFire;
    //private float _previousFireTime;
    private float _timer;
    private float _muzzleFlashTimer;
    
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        inputReader.PrimaryFireEvent += HandlePrimaryFire;
    }
    
    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        inputReader.PrimaryFireEvent -= HandlePrimaryFire;
    }

    private void HandlePrimaryFire(bool shouldFire)
    {
        if (_shouldFire && _isPointerOverUI) return;
        _shouldFire = shouldFire;
    }

    private void SpawnDummyProjectile(Vector3 position, Vector3 direction)
    {
        muzzleFlash.SetActive(true);
        _muzzleFlashTimer = muzzleFlashDuration;
        var projectile = Instantiate(clientProjectilePrefab, position, Quaternion.identity);
        projectile.transform.up = direction;
        Physics2D.IgnoreCollision(playerCollider, projectile.GetComponent<Collider2D>());
        
        if (projectile.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = rb.transform.up * projectileSpeed;
        }
    }

    private void Update()
    {
        UpdateFlash();
        if (!IsOwner) return;
        _isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
            return;
        }
        if (coinWallet.TotalCoins.Value < costToFire) return;
        if (!_shouldFire) return;
        PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);
        SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up);
        _timer = 1 / fireRate;
    }
    
    private void UpdateFlash()
    {
        if (_muzzleFlashTimer <= 0) return;
        _muzzleFlashTimer -= Time.deltaTime;
        if (_muzzleFlashTimer <= 0)
        {
            muzzleFlash.SetActive(false);
        }
    }
    
    [Rpc(SendTo.Server)]
    private void PrimaryFireServerRpc(Vector3 position, Vector3 direction)
    {
        if (coinWallet.TotalCoins.Value < costToFire) return;
        coinWallet.SpendCoins(costToFire);
        var projectile = Instantiate(serverProjectilePrefab, position, Quaternion.identity);
        projectile.transform.up = direction;
        Physics2D.IgnoreCollision(playerCollider, projectile.GetComponent<Collider2D>());


        if (projectile.TryGetComponent(out DealDamageOnContact dealDamage))
        {
            dealDamage.SetOwner(OwnerClientId);
        }
        if (projectile.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = rb.transform.up * projectileSpeed;
        }
        
        SpawnDummyProjectileClientRpc(position, direction);
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnDummyProjectileClientRpc(Vector3 position, Vector3 direction)
    {
        if (IsOwner) return;
        SpawnDummyProjectile(position, direction);
    }
}
