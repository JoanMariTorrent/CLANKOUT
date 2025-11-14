using System.Collections.Generic;
using System.Linq;
using PurrNet;

public static class PlayerRegistry
{
    public static List<Player> AllPlayers = new();

    public static Player GetLocalPlayer(PlayerID id)
    {
        return AllPlayers.FirstOrDefault(p => p.owner.Value == id);
    }
}
