using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public GameObject bareHandsPrefab;
    public Transform leftHandTransform;
    public Transform rightHandTransform;

    private CoreWeapon leftHandWeapon;
    private CoreWeapon rightHandWeapon;

    void Start()
    {
        // Initialize with bare hands
        EquipWeapon(bareHandsPrefab, true);
        EquipWeapon(bareHandsPrefab, false);
    }

    public void EquipWeapon(GameObject weaponPrefab, bool isRightHand)
    {
        Transform handTransform = isRightHand ? rightHandTransform : leftHandTransform;

        // Remove current weapon
        foreach (Transform child in handTransform)
        {
            Destroy(child.gameObject);
        }

        // Equip new weapon
        GameObject instantiatedWeapon = Instantiate(weaponPrefab, handTransform);
        CoreWeapon weaponScript = instantiatedWeapon.GetComponent<CoreWeapon>();
        weaponScript.SetHand(isRightHand);

        if (isRightHand)
            rightHandWeapon = weaponScript;
        else
            leftHandWeapon = weaponScript;
    }

    void Update()
    {
        CheckAndUpdateWeaponStatus(rightHandWeapon, true);
        CheckAndUpdateWeaponStatus(leftHandWeapon, false);
    }

    private void CheckAndUpdateWeaponStatus(CoreWeapon weapon, bool isRightHand)
    {
        if (weapon != null && weapon.currentAmmo <= 0 && weapon.weaponName != "Bare Hands")
        {
            // Out of ammo, switch to bare hands
            EquipWeapon(bareHandsPrefab, isRightHand);
        }
    }

    public bool UseWeapon(bool isRightHand)
    {
        CoreWeapon weapon = isRightHand ? rightHandWeapon : leftHandWeapon;
        return weapon.UseAmmo();
    }

    public void PickupWeapon(GameObject weaponPrefab, bool isRightHand)
    {
        EquipWeapon(weaponPrefab, isRightHand);
    }

    public void RefillAmmo(bool isRightHand)
    {
        CoreWeapon weapon = isRightHand ? rightHandWeapon : leftHandWeapon;
        weapon.RefillAmmo();
    }

    public string GetCurrentWeaponName(bool isRightHand)
    {
        CoreWeapon weapon = isRightHand ? rightHandWeapon : leftHandWeapon;
        return weapon.weaponName;
    }

    public int GetCurrentAmmo(bool isRightHand)
    {
        CoreWeapon weapon = isRightHand ? rightHandWeapon : leftHandWeapon;
        return weapon.currentAmmo;
    }

    public bool HasWeaponInHand(bool isRightHand)
    {
        CoreWeapon weapon = isRightHand ? rightHandWeapon : leftHandWeapon;
        return weapon.weaponName != "Bare Hands";
    }
}
