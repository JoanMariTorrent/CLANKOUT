using NUnit.Framework;
using PurrNet;
using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField] private Transform _handTransform;
    [SerializeField] private CinemachineCamera _playerCamera;
    [SerializeField] private LayerMask _hitLayer;

    private Gun _currentGun;
    [SerializeField] private List<GameObject> _ownedWeapons = new();

    [ServerRpc]
    public void EquipWeapon(GameObject weaponPrefab)
    {
        if (_currentGun != null)
        {
            Destroy(_currentGun.gameObject);
        }

        var weaponInstance = Instantiate(weaponPrefab, _handTransform);
        weaponInstance.transform.localPosition = Vector3.zero;
        weaponInstance.transform.localRotation = Quaternion.identity;

        _currentGun = weaponInstance.GetComponent<Gun>();
        if (_currentGun != null)
        {
            _currentGun.Setup(_playerCamera.transform, _hitLayer);
        }

        if (!_ownedWeapons.Contains(weaponPrefab))
            _ownedWeapons.Add(weaponPrefab);
    }



    [ServerRpc]
    public void SwitchWeapon(int index)
    {
        if (index >= 0 && index < _ownedWeapons.Count)
        {
            EquipWeapon(_ownedWeapons[index]);
        }
    }
}
