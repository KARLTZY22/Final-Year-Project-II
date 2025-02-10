using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeaponManagerScene2 : MonoBehaviour
{
    [System.Serializable]
    public class BossWeapon
    {
        [Header("Basic Settings")]
        public GameObject weaponObject;
        public string weaponName;
        public Transform muzzlePoint;

        [Header("Weapon Stats")]
        public float damage = 20f;
        public float fireRate = 0.5f;
        public float range = 100f;

        [Header("Ammo Settings")]
        public int maxAmmo = 30;
        public float reloadTime = 2f;
        [HideInInspector]
        public int currentAmmo;

        [Header("Effects")]
        public GameObject hitEffectPrefab;
        public ParticleSystem muzzleFlash;
        [HideInInspector]
        public WeaponRecoil weaponRecoil;
    }

    [Header("Weapons")]
    public BossWeapon[] bossWeapons;
    public LayerMask hitLayers;

    [Header("UI Elements")]
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI weaponStatusText;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip switchSound;
    public AudioClip emptySound;
    [Range(0f, 1f)]
    public float audioVolume = 0.7f;

    [Header("Controls")]
    public KeyCode switchWeaponKey = KeyCode.Alpha1;
    public KeyCode reloadKey = KeyCode.R;
    public KeyCode aimKey = KeyCode.Mouse1;

    private AudioSource audioSource;
    private Camera mainCamera;
    private float nextFireTime;
    private bool isReloading;
    private bool isAiming;
    private int currentWeaponIndex = 0;

    private void Start()
    {
        InitializeComponents();
        InitializeWeapons();
    }

    private void InitializeComponents()
    {
        mainCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void InitializeWeapons()
    {
        foreach (var weapon in bossWeapons)
        {
            if (weapon.weaponObject != null)
            {
                weapon.currentAmmo = weapon.maxAmmo;
                weapon.weaponRecoil = weapon.weaponObject.GetComponent<WeaponRecoil>();
                weapon.weaponObject.SetActive(false);
            }
        }

        // Activate first weapon
        if (bossWeapons.Length > 0)
        {
            bossWeapons[0].weaponObject.SetActive(true);
            UpdateUI();
        }
    }

    private void Update()
    {
        if (!isReloading)
        {
            HandleShooting();
            HandleWeaponSwitch();
            HandleReload();
            HandleAiming();
        }
    }

    private void HandleShooting()
    {
        if (bossWeapons.Length == 0 || currentWeaponIndex >= bossWeapons.Length) return;

        BossWeapon currentWeapon = bossWeapons[currentWeaponIndex];

        if (Input.GetKey(KeyCode.Mouse0) && Time.time >= nextFireTime)
        {
            if (currentWeapon.currentAmmo <= 0)
            {
                PlaySound(emptySound);
                UpdateUI();
                return;
            }

            Shoot(currentWeapon);
        }
    }

    private void Shoot(BossWeapon weapon)
    {
        nextFireTime = Time.time + weapon.fireRate;
        weapon.currentAmmo--;

        // Play sound
        PlaySound(shootSound);

        // Muzzle flash
        if (weapon.muzzleFlash != null)
        {
            weapon.muzzleFlash.Play();
        }

        // Apply recoil
        if (weapon.weaponRecoil != null)
        {
            if (isAiming)
                weapon.weaponRecoil.AddRecoilAiming();
            else
                weapon.weaponRecoil.AddRecoil();
        }

        // Raycast for hit
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, weapon.range, hitLayers))
        {
            // Spawn hit effect
            if (weapon.hitEffectPrefab != null)
            {
                Instantiate(weapon.hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            }

            // Handle hit logic
            if (hit.collider.TryGetComponent(out BossController boss))
            {
                boss.TakeDamage(weapon.damage);
            }
            else if (hit.collider.TryGetComponent(out DestructibleMachine machine))
            {
                machine.TakeDamage(weapon.damage);
            }
        }

        UpdateUI();
    }

    private void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(switchWeaponKey))
        {
            SwitchWeapon();
        }
    }

    private void SwitchWeapon()
    {
        if (bossWeapons.Length <= 1) return;

        // Disable current weapon
        bossWeapons[currentWeaponIndex].weaponObject.SetActive(false);

        // Switch to next weapon
        currentWeaponIndex = (currentWeaponIndex + 1) % bossWeapons.Length;
        bossWeapons[currentWeaponIndex].weaponObject.SetActive(true);

        // Play switch sound
        PlaySound(switchSound);

        UpdateUI();
    }

    private void HandleReload()
    {
        if (Input.GetKeyDown(reloadKey) && !isReloading)
        {
            StartReload();
        }
    }

    private void StartReload()
    {
        BossWeapon currentWeapon = bossWeapons[currentWeaponIndex];
        if (currentWeapon.currentAmmo >= currentWeapon.maxAmmo) return;

        isReloading = true;
        PlaySound(reloadSound);

        Invoke(nameof(FinishReload), currentWeapon.reloadTime);
    }

    private void FinishReload()
    {
        bossWeapons[currentWeaponIndex].currentAmmo = bossWeapons[currentWeaponIndex].maxAmmo;
        isReloading = false;
        UpdateUI();
    }

    private void HandleAiming()
    {
        isAiming = Input.GetKey(aimKey);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, audioVolume);
        }
    }

    private void UpdateUI()
    {
        BossWeapon currentWeapon = bossWeapons[currentWeaponIndex];

        if (weaponNameText != null)
            weaponNameText.text = currentWeapon.weaponName;

        if (ammoText != null)
            ammoText.text = $"Ammo: {currentWeapon.currentAmmo}/{currentWeapon.maxAmmo}";

        if (weaponStatusText != null)
            weaponStatusText.text = isReloading ? "Reloading..." : "";
    }
}