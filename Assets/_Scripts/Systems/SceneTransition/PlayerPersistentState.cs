/// <summary>
/// Хранит состояние игрока (здоровье, выносливость) между переходами между сценами.
/// Регистрируется как глобальный сервис в GameServiceRegistry.
/// </summary>
public class PlayerPersistentState
{
    public float Health { get; private set; }
    public float Stamina { get; private set; }
    public bool HasData { get; private set; }

    public void Save(float health, float stamina)
    {
        Health = health;
        Stamina = stamina;
        HasData = true;
    }

    public void Clear()
    {
        HasData = false;
    }
}
