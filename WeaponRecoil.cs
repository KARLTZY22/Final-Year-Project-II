using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    [Header("Recoil Settings")]
    public float recoilX = -2f;
    public float recoilY = 2f;
    public float recoilZ = 0.35f;
    public float aimRecoilMultiplier = 0.5f;
    public float snappiness = 6f;
    public float returnSpeed = 2f;

    [Header("References")]
    private Transform recoilTransform;
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    private void Start()
    {
        recoilTransform = transform;
    }

    private void Update()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.deltaTime);
        recoilTransform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void AddRecoil()
    {
        targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
    }

    public void AddRecoilAiming()
    {
        targetRotation += new Vector3(
            recoilX * aimRecoilMultiplier,
            Random.Range(-recoilY, recoilY) * aimRecoilMultiplier,
            Random.Range(-recoilZ, recoilZ) * aimRecoilMultiplier
        );
    }

    public void ResetRecoil()
    {
        targetRotation = Vector3.zero;
        currentRotation = Vector3.zero;
    }
}
