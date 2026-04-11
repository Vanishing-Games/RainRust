using Cysharp.Threading.Tasks;

namespace Core
{
    public interface IGameFlowCommand
    {
        string CommandName { get; }
        UniTask Execute();
    }
}
