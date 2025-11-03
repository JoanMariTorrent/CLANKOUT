using PurrNet;
using UnityEngine;
using Steamworks;

public class GodMode : NetworkBehaviour
{

    private const ulong DEV_STEAM_ID = 76561198355953706;

    [SerializeField] private bool isGodMode = false;

    public void Update()
    {
        if (!isOwner) return;
        ulong mySteamID = SteamUser.GetSteamID().m_SteamID;
        if (mySteamID != DEV_STEAM_ID) return;

        if (Input.GetKeyDown(KeyCode.F10))
        {
            Debug.Log("asdasdasd");
        }
    }
}
