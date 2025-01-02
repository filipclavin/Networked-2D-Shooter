using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float fireRate = 0.1f;
    [SerializeField] private InputActionAsset controls;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int maxHealth = 3;

    private NetworkVariable<int> health = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<Vector2> movementInput = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> aimInput = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private bool isFiring;
    private float fireTimer = 0f;

    private SpriteRenderer spriteRenderer;
    
    private PlayerStatDisplay playerStatDisplay;

    private Rigidbody2D rb;

    public override void OnNetworkSpawn()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = IsLocalPlayer ? Color.green : Color.red;

        playerStatDisplay = FindObjectOfType<PlayerStatDisplay>();

        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = false;

        if (IsHost)
        {
            ResetPlayer();
        }

        if (IsLocalPlayer)
        {
            InputActionMap actionMap = controls.FindActionMap("Player");
            actionMap.FindAction("Move").performed += OnMoveInput;
            actionMap.FindAction("Move").canceled += OnMoveInput;

            actionMap.FindAction("Aim").performed += OnAimInput;

            actionMap.FindAction("Fire").performed += OnFire;
            actionMap.FindAction("Fire").canceled += OnStopFire;

            controls.Enable();
        }

        health.OnValueChanged += (previous, current) =>
        {
            if (current <= 0)
            {
                if (IsHost)
                {
                    GameManager.Instance.EndRoundRpc(OwnerClientId == 0 ? PlayerNumber.Two : PlayerNumber.One); // Game state and scoring is critical, so we only want the host to handle it
                }
            }
            else
            {
                playerStatDisplay.SetPlayerHealth((PlayerNumber)OwnerClientId, current); // Health UI is not critical, so we can let all clients handle it
            }
        };

    }

    private void Update()
    {
        if (IsHost)
            HostUpdate();
    }

    private void HostUpdate()
    {
        // Handling firing on host to prevent cheating
        fireTimer -= Time.deltaTime;
        if (isFiring && fireTimer <= 0)
        {
            fireTimer = fireRate;
            GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
            NetworkObject bulletNetworkObject = bullet.GetComponent<NetworkObject>();
            bulletNetworkObject.Spawn();
            bulletNetworkObject.ChangeOwnership(OwnerClientId);
        }
    }

    private void FixedUpdate()
    {
        if (IsHost)
            HostFixedUpdate();
    }

    void HostFixedUpdate()
    {
        // Handling movement on host to prevent cheating
        rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * Vector2.ClampMagnitude(movementInput.Value, 1f));
        rb.MoveRotation(aimInput.Value - 90f);
    }

    private void OnMoveInput(InputAction.CallbackContext context)
    {
        movementInput.Value = context.ReadValue<Vector2>();
    }

    private void OnAimInput(InputAction.CallbackContext context)
    {
        Vector2 mousePosition = context.ReadValue<Vector2>();
        Vector2 aimDirection = (Camera.main.ScreenToWorldPoint(mousePosition) - transform.position).normalized;
        aimInput.Value = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
    }

    private void OnFire(InputAction.CallbackContext context)
    {
        FireRpc();
    }

    private void OnStopFire(InputAction.CallbackContext context)
    {
        StopFireRpc();
    }

    // Running firing logic on the server to prevent cheating
    [Rpc(SendTo.Server)]
    private void FireRpc()
    {
        isFiring = true;
    }

    [Rpc(SendTo.Server)]
    private void StopFireRpc()
    {
        isFiring = false;
    }

    public void Hit()
    {
        if (!IsHost) // This shoudn't happen, but just in case, also for debugging
        {
            Debug.LogWarning("Hit called on non-host player");
            return;
        }

        health.Value--;
    }

    public void ResetPlayer()
    {
        if (!IsHost) // This shoudn't happen, but just in case, also for debugging
        {
            Debug.LogWarning("ResetPlayer called on non-host player");
            return;
        }

        Transform playerSpawn = GameManager.Instance.GetPlayerSpawn((PlayerNumber)OwnerClientId);
        transform.position = playerSpawn.position;
        transform.rotation = playerSpawn.rotation;

        health.Value = maxHealth;
    }

}
