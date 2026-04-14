using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image healthBar;
    [SerializeField] private PlayerController player;

    public void SetPlayer(PlayerController p)
    {
        player = p;
    }

    private void Update()
    {
        if (player != null)
        {
            healthBar.fillAmount = player.CurrentHealth / player.MaxHealth;
        }
    }
}
