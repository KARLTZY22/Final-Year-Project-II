using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

public class WeaponManager : MonoBehaviour
{
    [System.Serializable]
    public class WeaponUnlockInfo
    {
        [Header("Basic Settings")]
        public GameObject weaponObject;
        public int trashRequiredToUnlock;
        public string weaponName;
        public int weaponIndex;
        [HideInInspector]
        public bool isUnlocked = false;

        [Header("Weapon Stats")]
        public float fireRate = 0.5f;
        public float damage = 10f;
        public float range = 100f;
        public Transform muzzlePoint;
        public GameObject hitEffectPrefab;

        [Header("Ammo Settings")]
        public int maxAmmo = 30;
        public float reloadTime = 2f;
        [HideInInspector]
        public int currentAmmo;
        [HideInInspector]
        public WeaponRecoil weaponRecoil;
    }

    [Header("Weapons Configuration")]
    public WeaponUnlockInfo[] weapons;
    public LayerMask hitLayers;

    [Header("UI References")]
    public TextMeshProUGUI weaponStatusText;
    public TextMeshProUGUI unlockProgressText;
    public TextMeshProUGUI notificationText;
    public TextMeshProUGUI ammoText;
    public float notificationDuration = 3f;

    [Header("Control Keys")]
    public KeyCode switchWeaponKey = KeyCode.Alpha1;
    public KeyCode fireKey = KeyCode.Mouse0;
    public KeyCode reloadKey = KeyCode.R;
    public KeyCode aimKey = KeyCode.Mouse2;

    [Header("Audio")]
    public AudioClip unlockSound;
    public AudioClip switchSound;
    public AudioClip reloadSound;
    public AudioClip shootSound;
    public AudioClip emptyClickSound;
    [Range(0f, 1f)]
    public float audioVolume = 0.7f;

    private AudioSource audioSource;
    private Camera mainCamera;
    private float nextFireTime = 0f;
    private bool isReloading = false;
    private bool isAiming = false;
    private int currentWeaponIndex = -1;
    private InventorySystem inventorySystem;
    private bool isInitialized = false;
    private bool isScene2;

    private void Start()
    {
        SetupComponents();
        InitializeWeapons();
        isInitialized = true;
    }

    private void SetupComponents()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        mainCamera = Camera.main;
        if (mainCamera == null)
            Debug.LogError("WeaponManager: Main camera not found!");

        inventorySystem = FindFirstObjectByType<InventorySystem>();
        if (inventorySystem == null)
            Debug.LogError("WeaponManager: InventorySystem not found!");
    }

    private void InitializeWeapons()
    {
        foreach (var weapon in weapons)
        {
            if (weapon.weaponObject != null)
            {
                weapon.isUnlocked = false;
                weapon.weaponObject.SetActive(false);
                weapon.currentAmmo = weapon.maxAmmo;
                weapon.weaponRecoil = weapon.weaponObject.GetComponent<WeaponRecoil>();
            }
        }

        currentWeaponIndex = -1;
        UpdateUI();
    }

    private void Update()
    {
        if (!isInitialized) return;

        if (!isReloading)
        {
            HandleAiming();
            HandleWeaponSwitch();
            HandleShooting();
            HandleReload();
        }

        if (!isScene2)
        {
            CheckWeaponUnlocks();


        }
    }

    private void HandleAiming()
    {
        isAiming = Input.GetKey(aimKey);
        UpdateCurrentWeaponAim();
    }

    private void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(switchWeaponKey))
        {
            SwitchToNextWeapon();
        }
    }

    private void HandleShooting()
    {
        if (!CanShoot()) return;

        WeaponUnlockInfo currentWeapon = weapons[currentWeaponIndex];

        if (Input.GetKey(fireKey) && Time.time >= nextFireTime)
        {
            if (currentWeapon.currentAmmo <= 0)
            {
                HandleEmptyWeapon();
                return;
            }

            Shoot(currentWeapon);
        }
    }

    private void Shoot(WeaponUnlockInfo weapon)
    {
        nextFireTime = Time.time + weapon.fireRate;
        weapon.currentAmmo--;

        PlaySound(shootSound);

        if (weapon.weaponRecoil != null)
            weapon.weaponRecoil.AddRecoil();

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, weapon.range, hitLayers))
        {
            HandleHit(hit, weapon);
        }

        UpdateUI();
    }

    private void HandleHit(RaycastHit hit, WeaponUnlockInfo weapon)
    {
        if (hit.collider.TryGetComponent(out EnemyController enemy))
        {
            enemy.TakeHit(weapon.damage, hit.point);
        }

        if (weapon.hitEffectPrefab != null)
        {
            GameObject hitEffect = Instantiate(weapon.hitEffectPrefab, hit.point,
                Quaternion.LookRotation(hit.normal));
            Destroy(hitEffect, 2f);
        }
    }

    private void HandleEmptyWeapon()
    {
        PlaySound(emptyClickSound);
        ShowNotification("Out of ammo! Press R to reload");
        nextFireTime = Time.time + weapons[currentWeaponIndex].fireRate;
    }

    private void HandleReload()
    {
        if (Input.GetKeyDown(reloadKey))
        {
            StartReload();
        }
    }

    private void StartReload()
    {
        if (isReloading || currentWeaponIndex < 0) return;

        WeaponUnlockInfo currentWeapon = weapons[currentWeaponIndex];
        if (currentWeapon.currentAmmo >= currentWeapon.maxAmmo)
        {
            ShowNotification("Ammo already full!");
            return;
        }

        isReloading = true;
        PlaySound(reloadSound);
        ShowNotification($"Reloading {currentWeapon.weaponName}...");
        Invoke(nameof(FinishReload), currentWeapon.reloadTime);
    }

    private void FinishReload()
    {
        isReloading = false;
        if (currentWeaponIndex >= 0)
        {
            weapons[currentWeaponIndex].currentAmmo = weapons[currentWeaponIndex].maxAmmo;
            UpdateUI();
            ShowNotification("Reload complete!");
        }
    }

    public void AddTrashCollected(int amount)
    {
        // If you have a trash counter, update it (optional)
        CheckWeaponUnlocks(); // Re-check unlock conditions
    }

    private void CheckWeaponUnlocks()
    {
        if (inventorySystem == null) return;

        bool newUnlock = false;
        int currentTrash = GetTotalTrashCount();

        foreach (var weapon in weapons)
        {
            if (!weapon.isUnlocked && currentTrash >= weapon.trashRequiredToUnlock)
            {
                UnlockWeapon(weapon);
                newUnlock = true;
            }
        }

        if (newUnlock && currentWeaponIndex == -1)
        {
            SwitchToFirstUnlockedWeapon();
        }

        UpdateUI();
    }

    private int GetTotalTrashCount()
    {
        int total = 0;
        foreach (TrashType type in System.Enum.GetValues(typeof(TrashType)))
        {
            total += inventorySystem.GetItemCount(type);
        }
        return total;
    }

    private void UnlockWeapon(WeaponUnlockInfo weapon)
    {
        if (!weapon.isUnlocked)
        {
            weapon.isUnlocked = true;
            PlaySound(unlockSound);
            ShowNotification($"Unlocked {weapon.weaponName}!");

            if (currentWeaponIndex == -1)
            {
                SwitchToWeapon(weapon.weaponIndex);
            }
        }
    }

    private void SwitchToNextWeapon()
    {
        if (currentWeaponIndex >= 0)
        {
            weapons[currentWeaponIndex].weaponObject.SetActive(false);
        }

        int startIndex = currentWeaponIndex;
        do
        {
            currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;
            if (weapons[currentWeaponIndex].isUnlocked)
            {
                SwitchToWeapon(currentWeaponIndex);
                return;
            }
        } while (currentWeaponIndex != startIndex);
    }

    private void SwitchToFirstUnlockedWeapon()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].isUnlocked)
            {
                SwitchToWeapon(i);
                break;
            }
        }
    }

    private void SwitchToWeapon(int index)
    {
        if (index >= 0 && index < weapons.Length && weapons[index].isUnlocked)
        {
            currentWeaponIndex = index;
            weapons[index].weaponObject.SetActive(true);
            UpdateCurrentWeaponAim();
            PlaySound(switchSound);
            ShowNotification($"Switched to {weapons[index].weaponName}");
            UpdateUI();
        }
    }

    private void UpdateCurrentWeaponAim()
    {
        if (currentWeaponIndex >= 0 && weapons[currentWeaponIndex].weaponRecoil != null)
        {
            
        }
    }

    private bool CanShoot()
    {
        return currentWeaponIndex >= 0 &&
               weapons[currentWeaponIndex].isUnlocked &&
               !isReloading;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, audioVolume);
        }
    }

    private void ShowNotification(string message)
    {
        if (notificationText != null)
        {
            notificationText.text = message;
            CancelInvoke(nameof(ClearNotification));
            Invoke(nameof(ClearNotification), notificationDuration);
        }
    }

    private void ClearNotification()
    {
        if (notificationText != null)
        {
            notificationText.text = "";
        }
    }

    private void UpdateUI()
    {
        UpdateWeaponStatus();
        UpdateAmmoDisplay();
        UpdateUnlockProgress();
    }

    private void UpdateWeaponStatus()
    {
        if (weaponStatusText != null)
        {
            weaponStatusText.text = currentWeaponIndex >= 0 && weapons[currentWeaponIndex].isUnlocked
                ? $"Current: {weapons[currentWeaponIndex].weaponName}"
                : "No Weapon";
        }
    }

    private void UpdateAmmoDisplay()
    {
        if (ammoText != null && currentWeaponIndex >= 0)
        {
            WeaponUnlockInfo currentWeapon = weapons[currentWeaponIndex];
            ammoText.text = $"Ammo: {currentWeapon.currentAmmo}/{currentWeapon.maxAmmo}";
        }
    }

    private void UpdateUnlockProgress()
    {
        if (unlockProgressText != null)
        {
            int currentTrash = GetTotalTrashCount();

            WeaponUnlockInfo nextWeapon = weapons.FirstOrDefault(w => !w.isUnlocked);

            if (nextWeapon != null)
            {
                int remaining = nextWeapon.trashRequiredToUnlock - currentTrash;
                unlockProgressText.text = $"Collect {remaining} more trash to unlock {nextWeapon.weaponName}";
            }
            else
            {
                unlockProgressText.text = "All weapons unlocked!";
            }
        }
    }
}