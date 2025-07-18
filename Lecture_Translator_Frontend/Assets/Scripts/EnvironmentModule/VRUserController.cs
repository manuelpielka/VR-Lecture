using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Class <c>VRUserController</c> handles VR user movement and interaction in VR environment.
/// </summary>
public class VRUserController : MonoBehaviour
{

    [Header("Movement Settings")]
    [SerializeField]
    private float MoveSpeed = 1.0f;

    [SerializeField]
    private float rotationSpeed = 60f;


    [Header("References")]

    [SerializeField] [Tooltip("Reference to the left hand controller.")]
    public ActionBasedController leftController;

    [SerializeField] [Tooltip("Reference to the right hand controller.")]
    public ActionBasedController rightController;

    [SerializeField] [Tooltip("The XR Origin GameObject (player root).")]
    public Transform xrOrigin;

    [SerializeField] [Tooltip("Reference to the user's head (Main Camera).")]
    public Transform headTransform;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Initializes the VR user at the start of the scene.
    void Start()
    {

    }

    // Update is called once per frame
    // Updates user movement and interaction every frame.
    void Update()
    {

    }

    /// <summary>
    /// Moves the VR user in the specified direction.
    /// </summary>
    /// <param name="direction">Direction vector input (x = left/right, z = forward/back)</param>
    public void Move(Vector3 direction)
    {
        // Get the forward direction from the head (camera)
        Vector3 forward = headTransform.forward;
        forward.y = 0;
        forward.Normalize();

        // Get the right direction from the head
        Vector3 right = headTransform.right;
        right.y = 0;
        right.Normalize();

        // Combine forward/backward and left/right movement based on input
        Vector3 move = forward * direction.z + right * direction.x;

        // Move the entire XR Origin (user)
        xrOrigin.position += move * MoveSpeed * Time.deltaTime;
    }

    /// <summary>
    /// Rotates the VR user(XR Origin) to the specified orientation.
    /// </summary>
    /// <param name="rotation">The target rotation used to determine how much to rotate.</param>
    public void Rotate(Quaternion rotation)
    {
        xrOrigin.Rotate(Vector3.up, rotation.eulerAngles.y * rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Instantly teleports the VR user to the specified target position.
    /// </summary>
    /// <param name="target">The destination position to teleport the player to.</param>
    public void Teleport(Vector3 target)
    {
        xrOrigin.position = target;
    }
}
