using UnityEngine;


[System.Serializable]
public class WeaponData
{
    public bool isUnlocked;
    public string weaponName;
    public int currentAmmo;
    public int maxAmmo;

    // Additional properties for weapon state
    public float damage;
    public float range;
    public float fireRate;
}