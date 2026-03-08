using UnityEngine;

public class GravityHandler : MonoBehaviour
{
    [SerializeField] private float gravityMultiplier = 1f;

    public void SetGravityMult(float value)
    {
        gravityMultiplier = value;
    }
}
