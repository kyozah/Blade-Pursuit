using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("References")]
    public Transform weaponHolder;

    [Header("Current Weapon")]
    private GameObject currentWeapon;
    private WeaponData currentWeaponData;
    private WeaponHitbox currentWeaponHitbox;

    [Header("Debug")]
    public bool showDebugInfo = true;

    void Start()
    {
        if (showDebugInfo)
            Debug.Log("✅ PlayerWeaponManager initialized");
    }

    public void PickupWeapon(WeaponData data, GameObject weaponObject)
    {
        if (data == null || weaponObject == null)
        {
            Debug.LogError("❌ Invalid weapon data or object!");
            return;
        }

        DropCurrentWeapon();

        currentWeapon = weaponObject;
        currentWeaponData = data;

        currentWeapon.transform.SetParent(weaponHolder);
        currentWeapon.transform.localPosition = data.holdPositionOffset;
        currentWeapon.transform.localRotation = Quaternion.Euler(data.holdRotationOffset);

        SetupEquippedState(currentWeapon);
        SetupWeaponHitbox();

        if (showDebugInfo)
        {
            Debug.Log($"🗡️ Equipped {data.weaponName}");
            Debug.Log($"  Damage: {data.baseDamage}");
            Debug.Log($"  Knockback: {data.knockbackForce}");
        }
    }

    void SetupEquippedState(GameObject weapon)
    {
        WeaponPickup pickup = weapon.GetComponent<WeaponPickup>();
        if (pickup != null)
            pickup.enabled = false;

        Collider col = weapon.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        WeaponHitbox hitbox = weapon.GetComponentInChildren<WeaponHitbox>();
        if (hitbox != null)
        {
            Collider hitboxCol = hitbox.GetComponent<Collider>();
            if (hitboxCol != null)
                hitboxCol.enabled = true;

            hitbox.gameObject.layer = LayerMask.NameToLayer("Weapon");
        }
    }

    void SetupWeaponHitbox()
    {
        if (currentWeapon == null || currentWeaponData == null) return;

        currentWeaponHitbox = currentWeapon.GetComponentInChildren<WeaponHitbox>();

        if (currentWeaponHitbox == null)
        {
            Debug.LogWarning($"⚠ No WeaponHitbox found on prefab '{currentWeaponData.weaponName}'. Creating fallback.");
            currentWeaponHitbox = CreateFallbackHitbox();
        }
        else
        {
            if (showDebugInfo)
                Debug.Log($"✅ Found WeaponHitbox on prefab: {currentWeaponHitbox.gameObject.name}");
        }

        currentWeaponHitbox.damage = currentWeaponData.baseDamage;
        currentWeaponHitbox.knockbackForce = currentWeaponData.knockbackForce;
        currentWeaponHitbox.knockbackUpwardForce = currentWeaponData.knockbackUpwardForce;

        currentWeaponHitbox.DisableDamage();

        AttackComboController attackController = GetComponent<AttackComboController>();
        if (attackController != null)
        {
            attackController.weaponHitbox = currentWeaponHitbox;
            if (showDebugInfo)
                Debug.Log("✅ WeaponHitbox linked to AttackComboController");
        }
    }

    WeaponHitbox CreateFallbackHitbox()
    {
        GameObject hitboxObj = new GameObject("WeaponHitbox_Auto");
        hitboxObj.transform.SetParent(currentWeapon.transform);
        hitboxObj.transform.localPosition = currentWeaponData.hitboxOffset;
        hitboxObj.transform.localRotation = Quaternion.identity;

        BoxCollider col = hitboxObj.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = currentWeaponData.hitboxSize;

        hitboxObj.layer = LayerMask.NameToLayer("Weapon");

        return hitboxObj.AddComponent<WeaponHitbox>();
    }

    public void DropCurrentWeapon()
    {
        if (currentWeapon == null) return;

        Transform weapon = currentWeapon.transform;
        weapon.SetParent(null);

        // ✅ Dùng Y hiện tại của player + offset 0.4
        Vector3 dropPos = transform.position + transform.forward * 1.5f;
        dropPos.y = transform.position.y + 0.4f;

        weapon.position = dropPos;
        weapon.rotation = Quaternion.identity;

        SetupGroundState(currentWeapon);

        AttackComboController attackController = GetComponent<AttackComboController>();
        if (attackController != null)
            attackController.weaponHitbox = null;

        currentWeapon = null;
        currentWeaponData = null;
        currentWeaponHitbox = null;
    }

    void SetupGroundState(GameObject weapon)
    {
        WeaponHitbox hitbox = weapon.GetComponentInChildren<WeaponHitbox>();
        if (hitbox != null)
        {
            hitbox.DisableDamage();

            Collider hitboxCol = hitbox.GetComponent<Collider>();
            if (hitboxCol != null)
                hitboxCol.enabled = false;

            hitbox.gameObject.layer = LayerMask.NameToLayer("Weapon");
        }

        WeaponPickup pickup = weapon.GetComponent<WeaponPickup>();
        if (pickup != null)
            pickup.enabled = true;

        Collider col = weapon.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = true;
        }

        weapon.layer = LayerMask.NameToLayer("Interactable");
    }

    public WeaponData GetCurrentWeaponData() => currentWeaponData;
    public WeaponHitbox GetCurrentWeaponHitbox() => currentWeaponHitbox;
    public float GetCurrentDamage() => currentWeaponData != null ? currentWeaponData.baseDamage : 0f;
}