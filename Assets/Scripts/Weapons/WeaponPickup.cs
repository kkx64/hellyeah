using System.Collections;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public GameObject weaponPrefab;
    public string weaponName;

    void Update()
    {
        transform.Rotate(new Vector3(0, 30, 0) * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WeaponManager weaponManager = other.GetComponent<WeaponManager>();
            if (weaponManager != null)
            {
                bool hasWeaponInRightHand = weaponManager.HasWeaponInHand(true);
                bool hasWeaponInLeftHand = weaponManager.HasWeaponInHand(false);

                if (hasWeaponInRightHand && hasWeaponInLeftHand)
                {
                    // Wait for key press for left or right hand pickup
                    StartCoroutine(WaitForKeyPress(weaponManager));
                }
                else
                {
                    bool pickupWithRightHand = !hasWeaponInRightHand; // If right hand is empty, pick up with right hand
                    string currentWeaponName = weaponManager.GetCurrentWeaponName(
                        pickupWithRightHand
                    );

                    if (currentWeaponName == weaponName)
                    {
                        // Refill ammo for the same weapon
                        weaponManager.RefillAmmo(pickupWithRightHand);
                    }
                    else
                    {
                        // Pick up new weapon
                        weaponManager.PickupWeapon(weaponPrefab, pickupWithRightHand);
                    }

                    Destroy(gameObject);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopAllCoroutines();
        }
    }

    private IEnumerator WaitForKeyPress(WeaponManager weaponManager)
    {
        bool keyPressed = false;
        bool pickupWithRightHand = false;

        while (!keyPressed)
        {
            if (Input.GetKeyDown(KeyCode.E)) // E for right hand
            {
                pickupWithRightHand = true;
                keyPressed = true;
            }
            else if (Input.GetKeyDown(KeyCode.Q)) // Q for left hand
            {
                pickupWithRightHand = false;
                keyPressed = true;
            }

            yield return null;
        }

        string currentWeaponName = weaponManager.GetCurrentWeaponName(pickupWithRightHand);

        if (currentWeaponName == weaponName)
        {
            // Refill ammo for the same weapon
            weaponManager.RefillAmmo(pickupWithRightHand);
        }
        else
        {
            // Pick up new weapon
            weaponManager.PickupWeapon(weaponPrefab, pickupWithRightHand);
        }

        Destroy(gameObject);
    }
}
