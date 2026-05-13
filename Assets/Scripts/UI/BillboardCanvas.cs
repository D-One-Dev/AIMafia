using UnityEngine;

public class BillboardCanvas : MonoBehaviour
{
    [SerializeField] private Transform targetCamera;

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        Vector3 direction = targetCamera.position - transform.position;
        direction.y = 0f;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(-direction);
        }
    }
}