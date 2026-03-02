using UnityEngine;

[DisallowMultipleComponent]
public class WeaponPickup : MonoBehaviour, IInteractable
{
    public WeaponData weaponData;

    private Vector3 basePosition;

    void OnEnable()
    {
        if (weaponData == null) return;

        basePosition = transform.position;
    }

    void Update()
    {
        if (weaponData == null) return;

        float yOffset =
            Mathf.Sin(Time.time * weaponData.floatSpeed) *
            weaponData.floatHeight;

        transform.position = new Vector3(
            basePosition.x,
            basePosition.y + yOffset,
            basePosition.z
        );

        transform.Rotate(
            Vector3.up,
            weaponData.rotationSpeed * Time.deltaTime
        );
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (weaponData == null) return;

        PlayerWeaponManager manager =
            interactor.GetWeaponManager();

        if (manager == null) return;

        manager.PickupWeapon(weaponData, gameObject);
    }
}