namespace AdvancedTimer.Core;

public interface IStateStore
{
    AppState Load();
    void Save(AppState state);
}
