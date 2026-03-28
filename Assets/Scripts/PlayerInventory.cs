using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public GameObject currentWeapon;

    public void Equip(GameObject weapon)
    {
        currentWeapon = weapon;
    }

    public void SwapWeapon(GameObject newWeapon)
    {
        // Drop the old weapon by leaving it in the scene
        // The old weapon remains as is
        currentWeapon = newWeapon;
    }
}