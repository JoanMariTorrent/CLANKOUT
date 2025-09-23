using PurrNet;
using UnityEngine;
using System.Collections.Generic;


public class Platform : NetworkBehaviour
{



    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        var _player = other.GetComponent<PlayerHealth>();
        if (_player == null) return;

        Debug.Log("Has entrado en colision con una plataforma!");
        if (_player.isOwner)
            RequestPowerMessageServerRPC(_player.PlayerID);
    }





    [ServerRpc]
    private void RequestPowerMessageServerRPC(PlayerID playerID)
    {
        Debug.Log($"{playerID}, ¡Has recibido un poder!");
    }








}
