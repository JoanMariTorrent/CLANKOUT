using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Game/WeaponDatabase")]
public class WeaponDatabase : ScriptableObject
{
    public List<WeaponScripteableObject> weapons;

    public int GetIdOfWeapon(WeaponScripteableObject weapon)
    {
        return weapons.IndexOf(weapon);
    }

    public WeaponScripteableObject GetWeaponByID(int id)
    {
        return weapons[id];
    }
}
