using UnityEngine;

public class PlayerLightSensor : LightSensorBase, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Player;

    public void Initialize()
    {
        InitializeSensor();
    }

    private void Update()
    {
        TickSensor(transform.position);
    }
}
