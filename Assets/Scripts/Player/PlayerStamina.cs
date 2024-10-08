using System.Collections;
using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float staminaRegenRate = 10f;
    public float staminaRegenDelay = 2f;
    public float staminaDepletionRate = 20f;

    [Header("Current State")]
    public float currentStamina;

    public delegate void StaminaChangeHandler(float currentStamina, float maxStamina);
    public event StaminaChangeHandler OnStaminaChanged;

    private Coroutine regenCoroutine;
    private WaitForSeconds regenDelay;
    private bool isRegenerating = false;

    private void Start()
    {
        currentStamina = maxStamina;
        regenDelay = new WaitForSeconds(staminaRegenDelay);
    }

    public bool UseStamina(float amount)
    {
        if (currentStamina - amount < 0)
            return false;

        ModifyStamina(-amount);
        if (regenCoroutine != null)
            StopCoroutine(regenCoroutine);

        regenCoroutine = StartCoroutine(RegenerateStamina());
        return true;
    }

    public void ModifyStamina(float amount)
    {
        currentStamina = Mathf.Clamp(currentStamina + amount, 0, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    private IEnumerator RegenerateStamina()
    {
        yield return regenDelay;
        isRegenerating = true;

        while (currentStamina < maxStamina)
        {
            ModifyStamina(staminaRegenRate * Time.deltaTime);
            yield return null;
        }

        isRegenerating = false;
    }

    public bool HasStamina(float amount)
    {
        return currentStamina >= amount;
    }

    public float GetStaminaPercentage()
    {
        return currentStamina / maxStamina;
    }

    public bool IsRegenerating()
    {
        return isRegenerating;
    }
}
