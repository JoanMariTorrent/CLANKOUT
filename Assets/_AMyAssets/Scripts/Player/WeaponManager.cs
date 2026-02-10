using PurrNet;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Rendering;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField] private Transform _handTransform;
    [SerializeField] private Transform _handTransformWithoutAnim;
    [SerializeField] private CinemachineCamera _playerCamera;
    [SerializeField] private LayerMask _hitLayer;
    [SerializeField] private RecoilCamera recoil;
    [SerializeField] private PlayerAnimationHandler animHandler;

    // --- CAMBIO PRINCIPAL: Usamos la clase padre ---
    // public Gun _currentGun; // (Viejo)
    public EquippableItem _currentItem;
    public Gun _currentGun 
{
    get 
    {
        return _currentItem as Gun;
    }
}

    [SerializeField] private LastGunEquiped lastGun;
    
    public SyncList <GameObject> _ownedWeapons = new(ownerAuth: false); 
    [SerializeField] private GameObject weaponInstance = null;
    [SerializeField] private PlayerCharacter playerChar;
    [SerializeField] private Player player;
    [Space][Header("Audios")]
    [SerializeField] private AudioClip[] takeGunSound;

    protected override void OnSpawned()
    {
        GetPlayerScript();
        if(isServer) EnsureWeaponSlots();
    }

    void Update()
    {
        lastGun = playerChar._lastGunEquiped;

        if (isOwner && _currentItem != null)
        {
            bool down = Input.GetButtonDown("Fire1");
            bool held = Input.GetButton("Fire1");
            bool up = Input.GetButtonUp("Fire1");

            _currentItem.UseItem(down, held, up);
        }
    }

    // --- RPCs y LOGICA SERVER ---

    [ServerRpc(requireOwnership: true)] 
    public void RequestPickupGunServerRpc(GameObject gunObject, bool isPrimary, bool isUtility)
    {
        NewWeapon(gunObject, isPrimary, isUtility, true);
    }

    public void NewWeapon(GameObject weaponPrefab, bool primary, bool utility, bool groundGun)
    {
        if (!isServer) return;

        EnsureWeaponSlots();
        
        Gun gunScript = weaponPrefab.GetComponent<Gun>();
        
        if(gunScript == null) return;

        WeaponID newWeaponID = gunScript.weaponID;
        bool havePrimary = _ownedWeapons[0] || _ownedWeapons[1];
        bool haveSecondary = _ownedWeapons[2] || _ownedWeapons[3];
        bool shouldDelete = false;

        if (primary && !utility)
        {
            if (havePrimary)
            {
                if ((_ownedWeapons[0] != null && _ownedWeapons[1] != null) || HasWeaponOfType(newWeaponID))
                    shouldDelete = true;
            }
        }
        else if (!primary && !utility)
        {
            if (haveSecondary)
            {
                if ((_ownedWeapons[2] != null && _ownedWeapons[3] != null) || HasWeaponOfType(newWeaponID))
                    shouldDelete = true;
            }
        }
        
        EquipWeapon(weaponPrefab, shouldDelete, primary, groundGun);
        PlayEquipSoundObserversRpc();
    }

    private void EquipWeapon(GameObject weaponPrefab, bool deleteWeapon, bool primaryWeapon, bool groundGun)
    {
        int targetIndex = -1;
        GameObject finalWeaponObject = null;

        if (deleteWeapon)
        {
            Gun gunScript = weaponPrefab.GetComponent<Gun>();
            targetIndex = IndexHasWeaponOfType(gunScript.weaponID);

            if (targetIndex == -1)
            {
                Gun currentGunScript = _currentItem as Gun;

                if (primaryWeapon)
                {
                    targetIndex = (currentGunScript != null && currentGunScript.weaponType == WeaponType.Primary) 
                                     ? IndexHasWeaponOfType(currentGunScript.weaponID) 
                                     : 0;
                    if (targetIndex == -1) targetIndex = 0;
                }
                else
                {
                    targetIndex = (currentGunScript != null && currentGunScript.weaponType == WeaponType.Secundary) 
                                     ? IndexHasWeaponOfType(currentGunScript.weaponID) 
                                     : 2;
                    if (targetIndex == -1) targetIndex = 2;
                }
            }

            if (targetIndex >= 0 && _ownedWeapons[targetIndex] != null)
            {
                DropWeaponAtIndex(targetIndex);
            }
        }
        else
        {
            targetIndex = GetWeaponIndex(primaryWeapon);
        }

        if (groundGun)
        {
            AddGunFromGround(weaponPrefab); 
            finalWeaponObject = weaponPrefab;
        }
        else
        {
            InstantiateGun(weaponPrefab);
            finalWeaponObject = weaponInstance;
        }

        if (targetIndex >= 0)
        {
            while (_ownedWeapons.Count <= targetIndex) _ownedWeapons.Add(null);
            _ownedWeapons[targetIndex] = finalWeaponObject;
            SwitchWeapon(targetIndex, finalWeaponObject);
        }
    }

    // --- SWITCH WEAPON (POLIMÓRFICO) ---

    [ObserversRpc(requireServer: false)]
    public void SwitchWeapon(int index, GameObject forcedWeapon = null) 
    {
        GameObject weaponToSwitch = forcedWeapon;

        if (weaponToSwitch == null)
        {
            if (index >= 0 && index < _ownedWeapons.Count)
                weaponToSwitch = _ownedWeapons[index];
        }

        if (weaponToSwitch == null) return;
        if (_currentItem != null && weaponToSwitch == _currentItem.gameObject) return;

        for (int i = 0; i < _ownedWeapons.Count; i++)
        {
            if (_ownedWeapons[i] != null) _ownedWeapons[i].SetActive(false);
        }

        weaponToSwitch.SetActive(true);
        
        _currentItem = weaponToSwitch.GetComponent<EquippableItem>();
        
        Rigidbody rb = weaponToSwitch.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        Collider col = weaponToSwitch.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (_currentItem != null) _currentItem.GiveOwnership(owner.Value);
        
        if (_currentItem is Gun gunScript)
        {
            if(gunScript.gunAnimHandler != null) weaponToSwitch.transform.SetParent(_handTransform);
            else weaponToSwitch.transform.SetParent(_handTransformWithoutAnim);
            
            // Setup de Arma
            if(player == null) GetPlayerScript();
            gunScript.Setup(_playerCamera.transform, _hitLayer, recoil, playerChar, player, this, animHandler);
        }
        else
        {
            weaponToSwitch.transform.SetParent(_handTransformWithoutAnim);
        }

        weaponToSwitch.transform.localPosition = Vector3.zero;
        weaponToSwitch.transform.localRotation = Quaternion.identity;

        // Llamamos al OnEquip del padre
        if (_currentItem != null) _currentItem.OnEquip();

        Debug.Log($"Cambio de item a {(_currentItem != null ? _currentItem.itemName : "Desconocido")} en el slot {index}");
    }

    public void SwitchWeapon(int index) { SwitchWeapon(index, null); }

    // --- RESTO DE FUNCIONES ---

    private void InstantiateGun(GameObject weaponPrefab)
    {
        if (_currentItem != null) _currentItem.gameObject.SetActive(false);

        // Instanciamos
        Gun gunCheck = weaponPrefab.GetComponent<Gun>(); 
        if(gunCheck != null && gunCheck.gunAnimHandler != null) weaponInstance = Instantiate(weaponPrefab, _handTransform);
        else weaponInstance = Instantiate(weaponPrefab, _handTransformWithoutAnim);
        
        weaponInstance.SetActive(true);
        weaponInstance.transform.localPosition = Vector3.zero;
        weaponInstance.transform.localRotation = Quaternion.identity;

        // Obtenemos el item
        EquippableItem newItem = weaponInstance.GetComponent<EquippableItem>();
        if (newItem == null) return;
        
        newItem.GiveOwnership(owner.Value);
        
        // Si es arma, setup
        if (newItem is Gun newGun)
        {
             if (player == null) GetPlayerScript();
             newGun.Setup(_playerCamera.transform, _hitLayer, recoil, playerChar, player, this, animHandler);
        }
    }

    public void AddGunFromGround(GameObject weaponObject)
    {
        EquippableItem itemScript = weaponObject.GetComponent<EquippableItem>();
        if (itemScript == null) return;
        
        // Parent logic
        if (itemScript is Gun g && g.gunAnimHandler != null) itemScript.transform.SetParent(_handTransform);
        else itemScript.transform.SetParent(_handTransformWithoutAnim);
        
        itemScript.transform.localPosition = Vector3.zero;
        itemScript.transform.localRotation = Quaternion.identity;
        
        Rigidbody rb = itemScript.GetComponent<Rigidbody>();
        if (rb) 
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        
        Collider col = itemScript.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        itemScript.GiveOwnership(owner.Value);
        
        if (itemScript is Gun gunScript)
        {
            if(player == null) GetPlayerScript();
            gunScript.Setup(_playerCamera.transform, _hitLayer, recoil, playerChar, player, this, animHandler);
        }
        
        itemScript.gameObject.SetActive(true);
    }
 
    [ObserversRpc(runLocally: true)]
    private void DropWeaponAtIndex(int index)
    {
        if (index < 0 || index >= _ownedWeapons.Count) return;
        GameObject weaponObj = _ownedWeapons[index];
        if (weaponObj == null) return;

        EquippableItem itemScript = weaponObj.GetComponent<EquippableItem>();
        if (itemScript == null) return;

        // Desequipar genérico
        itemScript.OnUnequip();
        
        // Desequipar específico de arma
        if (itemScript is Gun g) g.SetDown();

        weaponObj.transform.SetParent(null);
        weaponObj.SetActive(true);
        
        Rigidbody rb = weaponObj.GetComponent<Rigidbody>();
        if(rb)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce((transform.forward + Vector3.up).normalized * 3f, ForceMode.Impulse);
        }

        itemScript.GiveOwnership(null);

        if(isServer) _ownedWeapons[index] = null;
        if (_currentItem == itemScript) _currentItem = null;
    }

    private void EquipUtility(GameObject utilityPrefab) 
    { 
        InstantiateGun(utilityPrefab); 
        if(isServer) _ownedWeapons[4] = weaponInstance; 
        SwitchWeapon(4, weaponInstance); 
    }

    private bool HasWeaponOfType(WeaponID id) 
    { 
        foreach (var w in _ownedWeapons) 
        { 
            if (w == null) continue;
            // Solo chequeamos Guns
            Gun g = w.GetComponent<Gun>();
            if (g && g.weaponID == id) return true; 
        }
        return false; 
    }

    private int IndexHasWeaponOfType(WeaponID id) 
    { 
        for (int i = 0; i < _ownedWeapons.Count; i++) 
        { 
             if (_ownedWeapons[i] == null) continue;
             Gun g = _ownedWeapons[i].GetComponent<Gun>();
             if (g && g.weaponID == id) return i; 
        } 
        return -1; 
    }

    private int GetWeaponIndex(bool primaryWeapon) 
    { 
        int start = primaryWeapon ? 0 : 2; 
        int end = primaryWeapon ? 2 : 4; 
        for (int i = start; i < end; i++) 
        { 
            while (_ownedWeapons.Count <= i) 
                _ownedWeapons.Add(null); 
            if (_ownedWeapons[i] == null) return i; 
        } 
        return -1; 
    }

    private void EnsureWeaponSlots() 
    { 
        while (_ownedWeapons.Count < 5) 
            _ownedWeapons.Add(null); 
    }

    public void UtilityThrowed() 
    { 
        if(isServer) 
        { 
            _ownedWeapons.RemoveAt(4); 
            _ownedWeapons.Insert(4, null); 
        } 
        SwitchWeapon(0); 
    }

    public void DropGun() 
    { 
        if(isServer) 
            DoDropGunLogic(); 
        else 
            RequestDropGunServerRpc(); 
    }

    [ServerRpc(requireOwnership: true)] private void RequestDropGunServerRpc() { DoDropGunLogic(); }

    [ObserversRpc(runLocally: true)] private void DoDropGunLogic() 
    { 
        if (_currentItem == null) return; // Antes _currentGun

        _currentItem.isEquipped = false; 
        GameObject dropped = _currentItem.gameObject; 

        if (_currentItem is Gun g) g.SetDown(); // Específico

        dropped.transform.SetParent(null); 
        
        Rigidbody rb = dropped.GetComponent<Rigidbody>();
        if(rb)
        {
            rb.isKinematic = false; 
            rb.useGravity = true; 
            rb.AddForce(_handTransform.transform.forward * 5f, ForceMode.Impulse); 
        }

        _currentItem.GiveOwnership(null); 
        
        if (isServer) 
        { 
            int idx = _ownedWeapons.IndexOf(dropped); 
            if (idx >= 0) _ownedWeapons[idx] = null; 
        } 
        
        _currentItem = null; 
        
        int next = -1; 
        if (_ownedWeapons[0]) next = 0; 
        else if (_ownedWeapons[1]) next = 1; 
        else if (_ownedWeapons[2]) next = 2; 
        else if (_ownedWeapons[3]) next = 3; 
        if (next != -1) SwitchWeapon(next); 
    }

    [ObserversRpc(runLocally: true)] private void PlayEquipSoundObserversRpc() 
    { 
        if (takeGunSound.Length <= 0) return; 
        
        if(isOwner)
            AudioManager.Instance.PlaySound2D(takeGunSound[Random.Range(0, takeGunSound.Length)], type: AudioType.SFX, volume: 0.3f ,pitch: Random.Range(0.98f, 1.02f));
        else
            AudioManager.Instance.PlaySound(takeGunSound[Random.Range(0, takeGunSound.Length)], transform.position , type: AudioType.SFX, volume: 0.3f, pitch: Random.Range(0.98f, 1.02f));
    }

    [ObserversRpc(runLocally: true)] public void DropAllWeaponsOnDeath() 
    { 
        foreach (var w in _ownedWeapons) 
        {  
            if (!w) continue; 
            
            // Usamos item genérico
            EquippableItem item = w.GetComponent<EquippableItem>();
            if (!item) continue; 
            
            if (item is Gun g) g.SetDown();
            else item.OnUnequip();

            w.transform.SetParent(null); 
            w.SetActive(true); 
            
            Rigidbody rb = w.GetComponent<Rigidbody>();
            if(rb)
            {
                rb.isKinematic = false; 
                rb.useGravity = true; 
                rb.AddForce((Random.insideUnitSphere + Vector3.up).normalized * 4f, ForceMode.Impulse); 
            }
            item.GiveOwnership(null); 
        } 
        _currentItem = null; 
        if (isServer) 
        for (int i = 0; i < _ownedWeapons.Count; i++) 
            _ownedWeapons[i] = null; 
    }

    private void GetPlayerScript() { player = GetComponent<Player>(); }
}