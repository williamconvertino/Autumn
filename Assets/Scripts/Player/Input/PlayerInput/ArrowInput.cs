using UnityEngine;

public class ArrowInput : PlayerInput 
{
    public override bool GetButtonPressed()
    {
        return Input.GetKeyDown(KeyCode.RightShift);
    }

    public override float GetDirX()
    {
        float dirX = 0f;
        if (Input.GetKey(KeyCode.RightArrow)) dirX += 1.0f;
        if (Input.GetKey(KeyCode.LeftArrow)) dirX -= 1.0f;
        return dirX;
    }
    
    public override bool GetJump()
    {
        return Input.GetKeyDown(KeyCode.UpArrow);
    }

}