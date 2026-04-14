using System.Runtime.CompilerServices;
using UnityEngine;

public class InputSystem : MonoBehaviour
{
	public static float HorizontalRaw() => Input.GetAxisRaw("Horizontal");
	
	public static bool Jump() => Input.GetKeyDown(KeyCode.Space);

	public static bool Dash() => Input.GetKeyDown(KeyCode.X);
    
	public static bool Crouch() => Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);
  
	public static bool Run() => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

	public static bool Escape() => Input.GetKeyDown(KeyCode.Escape);

	public static bool PickUp() => Input.GetKeyDown(KeyCode.E);
    public static bool Drop() => Input.GetKeyDown(KeyCode.Q);

    public static bool SelectSlot1() => Input.GetKeyDown(KeyCode.Alpha1);
	public static bool SelectSlot2() => Input.GetKeyDown(KeyCode.Alpha2);
	public static bool SelectSlot3() => Input.GetKeyDown(KeyCode.Alpha3);

	public static float ScrollWheel() => Input.GetAxis("Mouse ScrollWheel");

    public static bool LMB_Down() => Input.GetMouseButtonDown(0);


    /// <summary>
    /// Возвращает -1 если колесо вверх, +1 если вниз, 0 если не прокручено.
    /// </summary>
    public static int ScrollDirection()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) return -1;
        if (scroll < 0f) return 1;
        return 0;
    }
}

