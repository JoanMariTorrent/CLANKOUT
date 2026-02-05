using TMPro;
using UnityEngine;

public class EnemyDown : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI player1Name;
    [SerializeField] private TextMeshProUGUI player2Name;

    public void Intiialize(string pl1, string pl2)
    {
        player1Name.text = pl1;
        if(!string.IsNullOrWhiteSpace(pl2) && pl2.Length > 0) player2Name.text = pl2;
        else player2Name.text = "void";
    }
}
