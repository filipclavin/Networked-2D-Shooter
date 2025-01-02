using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatDisplay : MonoBehaviour
{
    [SerializeField] private List<Image> playerOneHearts = new();
    [SerializeField] private TextMeshProUGUI playerOneScore;

    [SerializeField] private List<Image> playerTwoHearts = new();
    [SerializeField] private TextMeshProUGUI playerTwoScore;

    public void SetPlayerHealth(PlayerNumber player, int health)
    {
        switch (player)
        {
            case PlayerNumber.One:
                for (int i = 0; i < playerOneHearts.Count; i++)
                {
                    playerOneHearts[i].color = new Color(1f, 1f, 1f, i < health ? 1f : 0.5f);
                }
                break;
            case PlayerNumber.Two:
                for (int i = 0; i < playerTwoHearts.Count; i++)
                {
                    playerTwoHearts[i].color = new Color(1f, 1f, 1f, i < health ? 1f : 0.5f);
                }
                break;
        }
    }

    public void IncreaseScoreAndResetHealth(PlayerNumber player)
    {
        if (player == PlayerNumber.One)
        {
            playerOneScore.text = (int.Parse(playerOneScore.text) + 1).ToString();
            SetPlayerHealth(player, 3);
        }
        else
        {
            playerTwoScore.text = (int.Parse(playerTwoScore.text) + 1).ToString();
            SetPlayerHealth(player, 3);
        }

        for (int i = 0; i < playerOneHearts.Count; i++)
        {
            playerOneHearts[i].color = new Color(1f, 1f, 1f, 1f);
            playerTwoHearts[i].color = new Color(1f, 1f, 1f, 1f);
        }
    }
}
