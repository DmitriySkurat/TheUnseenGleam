using UnityEngine;

public class AgentLightSensor : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Enemy;
    
    public void Initialize()
    {
        
    }
}
