using NUnit.Framework;
using PurrNet;
using PurrNet.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawningGunsState : StateNode<List<PlayerHealth>>
{
    private List<PlayerID> _players = new();


    public override void Enter(List<PlayerHealth> data, bool asServer)
    {
        base.Enter(data, asServer);

        if (!asServer)
            return;
        if (data.Count <= 0)
            return;


        StartCoroutine(GetGuns(data));
        machine.Next(data);
    }
    private IEnumerator GetGuns(List<PlayerHealth> data)
    {
        if (InstanceHandler.TryGetInstance(out Canvas canvas))
        {
            foreach (var player in data)
            {
                var weaponManager = player.GetComponent<WeaponManager>();
                if (!weaponManager) continue;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                var slotMachine = canvas.slotMachine.GetComponent<SlotMachine>();
                slotMachine.GetComponent<CanvasGroup>().alpha = 1f;
                canvas.slotMachine.SetActive(true);

                yield return slotMachine.Spin();
                Debug.Log("<color=green>✅ Spin completado correctamente</color>");
                yield return new WaitForSeconds(1f);
                slotMachine.GetComponent<CanvasGroup>().alpha = 0f;

                if (slotMachine.finalWeapon.weaponType == WeaponScripteableType.Primary)
                    weaponManager.NewWeapon(slotMachine.finalWeapon.gunPrefab.gameObject, true, false, false);

                else if (slotMachine.finalWeapon.weaponType == WeaponScripteableType.Secondary)
                    weaponManager.NewWeapon(slotMachine.finalWeapon.gunPrefab, false, false, false);

                else if (slotMachine.finalWeapon.weaponType == WeaponScripteableType.Utility)
                    weaponManager.NewWeapon(slotMachine.finalWeapon.gunPrefab, false, true, false);


                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                weaponManager.SwitchWeapon(0);
                canvas.slotMachine.SetActive(false);
                player.GetComponent<Player>().canMove = true;
            }
        }
        
    }
}

