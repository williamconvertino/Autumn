using UnityEngine;

public class WASDInput : PlayerInput 
{
    public override bool GetButtonPressed()
    {
        return Input.GetKeyDown(KeyCode.S);
    }

    public override float GetDirX()
    {
        float dirX = 0f;
        if (Input.GetKey(KeyCode.D)) dirX += 1.0f;
        if (Input.GetKey(KeyCode.A)) dirX -= 1.0f;
        return dirX;
    }

    public override bool GetJump()
    {
        return Input.GetKeyDown(KeyCode.W);
    }

}