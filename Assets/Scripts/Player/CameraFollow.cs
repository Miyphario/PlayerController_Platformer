using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _followObject;
    [SerializeField] private float _smoothSpeed = 5f;

    private void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, _followObject.position, _smoothSpeed * Time.fixedDeltaTime); 
    }
}
