using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    public const float Reach = 10;
    
    public GameObject marker;
    public GameObject marker2;
    public World world;
    public Camera cam;
    public float lookSpeed = 3;

    private Transform camTransform;
    
    private Vector2 rotation = Vector2.zero;
    private Rigidbody rb;
    private float speed = 5f;
    private Transform pTransform;
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
        pTransform = transform;
        camTransform = cam.transform;
    }

    private void Update()
    {
        Look();
        Move();

        bool leftClick = Input.GetButtonDown("Fire1"); // Break block
        bool rightClick = Input.GetButtonDown("Fire2"); // Place block

        if (leftClick || rightClick)
        {
            (Block.Id blockId, Vector3Int blockPos, Vector3Int hitNormal) = 
                VoxelRay.Cast(world, camTransform.position, camTransform.forward, 10);

            if (leftClick && blockId != Block.Id.Air)
            {
                world.SetBlock(Block.Id.Air, blockPos.x, blockPos.y, blockPos.z);
            }
            else if (rightClick)
            {
                Vector3Int placePos = blockPos + hitNormal;
                Block.Id placePosBlockId = world.GetBlock(placePos.x, placePos.y, placePos.z);

                if (placePosBlockId == Block.Id.Air)
                {
                    world.SetBlock(Block.Id.Grass, placePos.x, placePos.y, placePos.z);
                }
            }
        }
    }

    private void Move()
    {
        Vector3 movement = Vector3.zero;
        Vector2 input = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        input.Normalize();
        movement += speed * input.x * pTransform.right;
        movement += speed * input.y * pTransform.forward;
        movement.y = rb.velocity.y;

        rb.velocity = movement;
    }

    private void Look()
    {
        rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
        rotation.x -= Input.GetAxis("Mouse Y") * lookSpeed;
        rotation.x = Mathf.Clamp(rotation.x, -89f, 89f);
        transform.eulerAngles = new Vector2(0, rotation.y);
        cam.transform.localRotation = Quaternion.Euler(rotation.x, 0, 0);
    }
}
