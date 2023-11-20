using UnityEngine;

public abstract class PlayerInput : MonoBehaviour 
{
    public abstract bool GetButtonPressed();
    public abstract float GetDirX();
    public abstract bool GetJump();
    
}