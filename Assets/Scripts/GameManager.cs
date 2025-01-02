using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum PlayerNumber
{
    One = 0,
    Two = 1,
}

public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameObject canvas;
    [SerializeField] private List<Transform> playerSpawns;
    [SerializeField] private PlayerStatDisplay statDisplay;

    static public GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Host()
    {
        NetworkManager.Singleton.StartHost();
        canvas.SetActive(false);
    }

    public void Join()
    {
        NetworkManager.Singleton.StartClient();
        canvas.SetActive(false);
    }

    public Transform GetPlayerSpawn(PlayerNumber player)
    {
        Transform spawn = playerSpawns[(int)player];
        return spawn;
    }

    [Rpc(SendTo.Everyone)]
    public void EndRoundRpc(PlayerNumber winner)
    {
        statDisplay.IncreaseScoreAndResetHealth(winner);

        if (IsHost)
        {
            foreach (Player player in FindObjectsOfType<Player>())
            {
                player.ResetPlayer(); // Positions and health are critical to gameplay, so we only want the host to handle it
            }

            foreach (Bullet bullet in FindObjectsOfType<Bullet>())
            {
                Destroy(bullet.gameObject); // Destruction of objects is critical to gameplay, so we only want the host to handle it
            }
        }
    }
}
