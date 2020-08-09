using System.Collections;
using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [SerializeField] private Transform target; // object to orbit
    public float rotSpeed = 1.5f;
    private float _rotY;
    private Vector3 _offset;
    void Start()
    {
        _rotY = transform.eulerAngles.y; // get auler angle
        // get positional offset between camera and target to maintain ditance
        _offset = target.position - transform.position; 
    }

    void LateUpdate()
    {
        // rotate camera slowly with arrows
        float horInput = Input.GetAxis("Horizontal");
        if (horInput != 0)
        {
            _rotY += horInput * rotSpeed;
            // or control reversed:
            //_rotY -= horInput * rotSpeed;
        }
        // or quickly with mouse
        else
        {
            _rotY += Input.GetAxis("Mouse X") * rotSpeed * 3;
        }
        // maintain starting offset shifted according to camera rotation
        // conveted to a quaternion
        Quaternion rotation = Quaternion.Euler(0, _rotY, 0);
        transform.position = target.position - (rotation * _offset);
        // turn camera towards target
        transform.LookAt(target);
    }
}
