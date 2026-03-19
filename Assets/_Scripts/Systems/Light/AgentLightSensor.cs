using UnityEngine;

public class AgentLightSensor : LightSensorBase, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Enemy;
    
    public void Initialize()
    {
        
    }
}
