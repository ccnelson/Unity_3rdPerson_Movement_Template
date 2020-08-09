using System.Collections;
using UnityEngine;

public class RelativeMovement : MonoBehaviour
{
    [SerializeField] private Transform target; // the object to move relative to
    public float rotSpeed = 15.0f; // speed for lerping
    public float moveSpeed = 6.0f; // locomotion speed
    public float jumpSpeed = 15.0f;
    public float gravity = -9.8f;
    public float terminalVelocity = -10.0f;
    public float minFall = -1.5f;
    private float _vertSpeed;
    private CharacterController _charController; // attached component
    private ControllerColliderHit _contact; // collision data for slipping off slopes and ledges
    private Animator _animator;
    

    void Start()
    {
        _vertSpeed = minFall; // initialised as minimum
        _charController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        Vector3 movement = Vector3.zero; // new vector

        float horInput = Input.GetAxis("Horizontal");
        float vertInput = Input.GetAxis("Vertical");
        if (horInput != 0 || vertInput != 0)
        {
            movement.x = horInput * moveSpeed;
            movement.z = vertInput * moveSpeed;
            movement = Vector3.ClampMagnitude(movement, moveSpeed); // limit diagonal

            Quaternion tmp = target.rotation; // initial rotation to restore
            // get only rotation around y axis
            target.eulerAngles = new Vector3(0, target.eulerAngles.y, 0);
            // transform movement from local to global corrdinates
            movement = target.TransformDirection(movement);
            // restore targets rotation
            target.rotation = tmp;
            // calculate quaternion facing that direction and apply
            //transform.rotation = Quaternion.LookRotation(movement);
            // or better yet - use linear interpolation
            Quaternion direction = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Lerp(transform.rotation,
            direction, rotSpeed * Time.deltaTime);
        }

        _animator.SetFloat("Speed", movement.sqrMagnitude);

        //// jumping and gravity
        //if (_charController.isGrounded) // controller is touching ground
        //{
        //    if (Input.GetButtonDown("Jump"))
        //    {
        //        _vertSpeed = jumpSpeed;
        //    }
        //    else
        //    {
        //        _vertSpeed = minFall;
        //    }
        //}
        //else // controller isnt touching ground
        //{
        //    _vertSpeed += gravity * 5 * Time.deltaTime; // apply gravity
        //    if (_vertSpeed < terminalVelocity) // ensure doesnt exceed terminal velocity
        //    {
        //        // (less-than because value is negative)
        //        _vertSpeed = terminalVelocity;
        //    }
        //}

        // jumping  and gravity with slopes and ledges
        bool hitGround = false;
        RaycastHit hit;
        // check if player is falling
        if (_vertSpeed < 0 && Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            // controller height + rounded ends / 2 (as ray cast from middle of controller)
            // check slightly beyond bottom of collider so 1.9 instead of 2 (for small inacuracies)
            float check = (_charController.height + _charController.radius) / 1.9f;
            hitGround = hit.distance <= check;
        }

        if (hitGround) // instead of isGrounded check raycast result
        {
            if (Input.GetButtonDown("Jump"))
            {
                _vertSpeed = jumpSpeed;
            }
            else
            {
                _vertSpeed = minFall;
                _animator.SetBool("Jumping", false);
            }
        }
        else
        {
            _vertSpeed += gravity * 5 * Time.deltaTime;
            if (_vertSpeed < terminalVelocity)
            {
                _vertSpeed = terminalVelocity;
            }

            if (_contact != null)
            {
                // prevents animation playing when game initially starts
                _animator.SetBool("Jumping", true);
            }

            if (_charController.isGrounded) // raycast didnt find ground, but collider did
            {
                // response depends on how the character is facing the contact point
                // (mvmnt vector facing relative to collision point determined using dot product)
                if (Vector3.Dot(movement, _contact.normal) < 0)
                {
                    movement = _contact.normal * moveSpeed;
                }
                else
                {
                    movement += _contact.normal * moveSpeed;
                }
            }
        }

        movement.y = _vertSpeed;

        movement *= Time.deltaTime;
        _charController.Move(movement);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // collision data stored in callback
        _contact = hit;
    }
}
