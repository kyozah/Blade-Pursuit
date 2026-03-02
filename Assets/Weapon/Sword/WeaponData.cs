using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName = "Sword";

    [Header("Prefab")]
    public GameObject weaponPrefab;

    [Header("Hold Settings")]
    public Vector3 holdPositionOffset;
    public Vector3 holdRotationOffset;

    [Header("Ground Settings")]
    public float groundHeight = 0.4f;
    public float floatHeight = 0.2f;
    public float floatSpeed = 2f;
    public float rotationSpeed = 60f;

    [Header("Combat Stats")]
    [Tooltip("Sát thương cơ bản của vũ khí")]
    public float baseDamage = 20f;

    [Tooltip("Lực đẩy ngang")]
    public float knockbackForce = 10f;

    [Tooltip("Lực hất lên")]
    public float knockbackUpwardForce = 2f;

    [Header("Hitbox Settings")]
    [Tooltip("Kích thước hitbox (Box Collider)")]
    public Vector3 hitboxSize = new Vector3(0.2f, 1.5f, 0.2f);

    [Tooltip("Vị trí offset của hitbox so với weapon")]
    public Vector3 hitboxOffset = new Vector3(0, 0.75f, 0);
}