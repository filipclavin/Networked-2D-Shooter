using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float speed = 10f;

    private Rigidbody2D rb;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (IsHost)
            HostFixedUpdate();
    }

    private void HostFixedUpdate()
    {
        rb.MovePosition(rb.position + speed * Time.fixedDeltaTime * (Vector2)transform.up); // bullet movement is critical to gameplay, so we only want the host to handle it
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsHost) // Bullet collision is critical to gameplay, so we only want the host to handle it
        {
            if (collision.gameObject.TryGetComponent(out Player player))
            {
                NetworkObject playerNetworkObject = player.GetComponent<NetworkObject>();
                if (playerNetworkObject.OwnerClientId == OwnerClientId) return;

                player.Hit();
            }
            Destroy(gameObject);
        }
    }
}
