using UnityEngine;

public class CoreWeapon : MonoBehaviour
{
    public bool rightHand = true;
    protected string fireKey = "Fire1";

    void Awake()
    {
        if (rightHand == false)
        {
            fireKey = "Fire2";
        }
    }
}
