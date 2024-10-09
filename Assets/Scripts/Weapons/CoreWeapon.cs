using UnityEngine;

public class CoreWeapon : MonoBehaviour
{
    string leftHandFireKey = "Fire1";
    string rightHandFireKey = "Fire2";

    public bool rightHand = true;
    protected string fireKey = "Fire1";
    public int currentAmmo = 0;
    public int maxAmmo = 10;
    public string weaponName = "Bare Hands";
    public int ammoRefillAmount = 5;

    void Awake()
    {
        fireKey = rightHand ? rightHandFireKey : leftHandFireKey;
    }

    public void SetHand(bool isRightHand)
    {
        rightHand = isRightHand;
        fireKey = isRightHand ? rightHandFireKey : leftHandFireKey;
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
