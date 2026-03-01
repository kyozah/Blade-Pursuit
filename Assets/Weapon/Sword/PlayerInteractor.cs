using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public float interactRange = 2f;
    public LayerMask interactLayer;
    public KeyCode interactKey = KeyCode.E;

    private IInteractable currentTarget;

    void Update()
    {
        DetectInteractable();

        if (currentTarget != null && Input.GetKeyDown(interactKey))
        {
            currentTarget.Interact(this);
        }
    }

    void DetectInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            interactRange,
            interactLayer
        );

        // ✅ DEBUG: in ra số lượng collider detect được và tên của chúng
        if (hits.Length > 0)
        {
            foreach (Collider col in hits)
            {
                Debug.Log($"[Interactor] Detected: {col.gameObject.name} | Layer: {LayerMask.LayerToName(col.gameObject.layer)} | HasIInteractable: {col.GetComponent<IInteractable>() != null}");
            }
        }
        else
        {
            // Không detect được gì — kiểm tra xem weapon có gần không
            Debug.Log($"[Interactor] No hits | Position: {transform.position} | Range: {interactRange} | LayerMask value: {interactLayer.value}");
        }

        float closest = Mathf.Infinity;
        IInteractable nearest = null;

        foreach (Collider col in hits)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();

            if (interactable == null) continue;

            float distance = Vector3.Distance(transform.position, col.transform.position);

            if (distance < closest)
            {
                closest = distance;
                nearest = interactable;
            }
        }

        currentTarget = nearest;
        Debug.Log($"[Interactor] Current target: {(currentTarget != null ? currentTarget.ToString() : "NULL")}");
    }

    public PlayerWeaponManager GetWeaponManager()
    {
        return GetComponent<PlayerWeaponManager>();
    }

    // Visualize detect range trong Scene view
    void OnDrawGizmos()
    {
        Gizmos.color = currentTarget != null ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}