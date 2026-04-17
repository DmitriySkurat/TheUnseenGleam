using System.Threading.Tasks;

public class GameServiceRegistry : ServiceRegistry
{
    public override async Task InitializeAsync()
    {
        Services.Register(new PlayerPersistentState());
        await base.InitializeAsync();
    }
}
