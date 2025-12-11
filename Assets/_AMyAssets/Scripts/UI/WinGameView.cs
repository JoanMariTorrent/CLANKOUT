using UnityEngine;
using UnityEngine.SceneManagement;

public class WinGameView : View
{
    public override void OnShow(){ Cursor.lockState = CursorLockMode.None; Cursor.visible = true;}

    public override void OnHide(){ Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;}

    public void Exit()
    {
        SceneManager.LoadScene(0);
    }
}
