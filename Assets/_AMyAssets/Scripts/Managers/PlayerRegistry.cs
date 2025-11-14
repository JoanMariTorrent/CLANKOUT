using System.Collections.Generic;
using System.Linq;
using PurrNet;

public class PlayerRegistry 
{
    // Lista de todos los jugadores
    public static List<Player> AllPlayers = new();

    // Devuelve el Player local correspondiente al PlayerID
    public static Player GetLocalPlayer(PlayerID id)
    {
        return AllPlayers.FirstOrDefault(p => p.owner.Value == id);
    }
}
