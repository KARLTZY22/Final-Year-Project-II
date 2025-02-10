using UnityEngine;

public class RecoilSystem : MonoBehaviour
{
    [Header("Recoil Settings")]
    [SerializeField] private float recoilX = -2f;         // How much the gun kicks back
    [SerializeField] private float recoilY = 2f;          // How much the gun kicks up
    [SerializeField] private float recoilZ = 0.35f;       // How much the gun kicks sideways
    [SerializeField] private float snappiness = 6f;       // How fast the gun moves to recoil position
    [SerializeField] private float returnSpeed = 2f;      // How fast the gun returns to original position

    [Header("Weapon Sway")]
    [SerializeField] private float swayAmount = 0.02f;    // Amount of weapon sway
    [SerializeField] private float maxSwayAmount = 0.06f; // Maximum sway limit
    [SerializeField] private float swaySmoothing = 4.0f;  // How smooth the sway movement is
    [SerializeField] private bool enableSway = true;      // Toggle weapon sway

    [Header("References")]
    [SerializeField] private Transform weaponTransform;   // The weapon model transform
    [SerializeField] private Transform cameraTransform;   // The camera transform for rotations

    // Recoil rotation
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    // Sway position
    private Vector3 initialPosition;
    private Vector3 targetPosition;

    private void Start()
    {
        // If no weapon transform assigned, use this object
        if (weaponTransform == null)
            weaponTransform = transform;

        // If no camera transform assigned, find the main camera
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        initialPosition = weaponTransform.localPosition;
    }

    private void Update()
    {
        if (enableSway)
            HandleWeaponSway();

        HandleRecoil();
    }

    public void AddRecoil()
    {
        // Add random recoil rotation
        targetRotation += new Vector3(
            recoilX,
            Random.Range(-recoilY, recoilY),
            Random.Range(-recoilZ, recoilZ)
        );
    }

    private void HandleRecoil()
    {
        // Smooth damp current rotation towards target rotation
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);

        // Apply rotation to weapon
        weaponTransform.localRotation = Quaternion.Euler(currentRotation);
    }

    private void HandleWeaponSway()
    {
        // Get mouse input for sway
        float mouseX = Input.GetAxisRaw("Mouse X") * swayAmount;
        float mouseY = Input.GetAxisRaw("Mouse Y") * swayAmount;

        // Calculate target position with sway
        Vector3 targetPos = new Vector3(
            Mathf.Clamp(mouseX, -maxSwayAmount, maxSwayAmount),
            Mathf.Clamp(mouseY, -maxSwayAmount, maxSwayAmount),
            0
        );

        // Smoothly move weapon towards target position
        weaponTransform.localPosition = Vector3.Lerp(
            weaponTransform.localPosition,
            initialPosition + targetPos,
            Time.deltaTime * swaySmoothing
        );
    }

    // Method to modify recoil settings at runtime
    public void ModifyRecoil(float xRecoil, float yRecoil, float zRecoil)
    {
        recoilX = xRecoil;
        recoilY = yRecoil;
        recoilZ = zRecoil;
    }

    // Reset recoil to original position
    public void ResetRecoil()
    {
        targetRotation = Vector3.zero;
        currentRotation = Vector3.zero;
        weaponTransform.localPosition = initialPosition;
    }
}
