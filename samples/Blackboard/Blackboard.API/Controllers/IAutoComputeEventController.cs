namespace Democrite.Sample.Blackboard.Memory.Controllers
{
    using Democrite.Framework.Node.Blackboard.Abstractions.VGrains.Controllers;
    using Democrite.Sample.Blackboard.Memory.Models;

    public interface IAutoComputeEventController : IBlackboardBaseControllerGrain<AutoComputeBlackboardOptions>, IBlackboardEventControllerGrain
    {
    }
}
