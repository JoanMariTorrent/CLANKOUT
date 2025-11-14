using PurrNet;
using UnityEngine;

public class PruebasRPC : NetworkBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
            ChangeColor(Color.red);
        if(Input.GetKeyDown(KeyCode.Alpha2))
            ChangeColor(Color.green);
        if(Input.GetKeyDown(KeyCode.Alpha3))
            ChangeColor(Color.blue);
        if(Input.GetKeyDown(KeyCode.Alpha4))
            ChangeColor(Color.black);
        if(Input.GetKeyDown(KeyCode.Alpha5))
            ChangeColor(Color.grey);
    }


    private void ChangeColor(Color color)
    {
        GetComponent<Renderer>().sharedMaterial.color = color;
    }
}
