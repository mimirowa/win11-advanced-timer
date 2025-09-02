using System.Threading.Tasks;

namespace AdvancedTimer.Core;

public interface IStateStore
{
    Task<AppState> LoadAsync();
    Task SaveAsync(AppState state);
}
