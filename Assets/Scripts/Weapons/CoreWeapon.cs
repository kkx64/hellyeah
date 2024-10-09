using UnityEngine;

public class CoreWeapon : MonoBehaviour
{
    public bool rightHand = true;
    protected string fireKey = "Fire1";
    public int currentAmmo = 0;
    public int maxAmmo = 10;
    public string weaponName = "Bare Hands";
    public int ammoRefillAmount = 5;

    void Awake()
    {
        if (rightHand == false)
        {
            fireKey = "Fire2";
        }
    }

    public void SetHand(bool isRightHand)
    {
        rightHand = isRightHand;
        fireKey = isRightHand ? "Fire1" : "Fire2";
    }

    public virtual bool UseAmmo()
    {
        if (currentAmmo > 0)
        {
            currentAmmo--;
            return true;
        }
        return false;
    }

    public virtual void RefillAmmo()
    {
        currentAmmo = Mathf.Min(currentAmmo + ammoRefillAmount, maxAmmo);
    }
}
