using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Slider healthBar;
    public Slider staminaBar;
    public TMP_Text leftHandAmmoText;
    public TMP_Text rightHandAmmoText;

    WeaponManager weaponManager;
    PlayerHealth playerHealth;
    PlayerStamina playerStamina;

    void Start()
    {
        weaponManager = FindAnyObjectByType<WeaponManager>();
        playerHealth = FindAnyObjectByType<PlayerHealth>();
        playerStamina = FindAnyObjectByType<PlayerStamina>();
    }

    void Update()
    {
        UpdateHealthBar();
        UpdateStaminaBar();
        UpdateAmmoText();
    }

    void UpdateHealthBar()
    {
        if (playerHealth != null)
        {
            healthBar.value = (float)playerHealth.health / (float)playerHealth.maxHealth;
        }
    }

    void UpdateStaminaBar()
    {
        if (playerStamina != null)
        {
            staminaBar.value = playerStamina.currentStamina / playerStamina.maxStamina;
        }
    }

    void UpdateAmmoText()
    {
        if (weaponManager == null)
            return;

        leftHandAmmoText.text = "" + weaponManager.GetCurrentAmmo(false);
        rightHandAmmoText.text = "" + weaponManager.GetCurrentAmmo(true);
    }
}
