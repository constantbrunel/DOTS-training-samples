using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;

public class LogicalPositionUpdateSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        JobHandle jh = Entities.ForEach((ref LogicalPosition logicalPosition, in Translation translation) =>
        {
            logicalPosition.PositionX = (int)translation.Value.x;
            logicalPosition.PositionY = (int)translation.Value.z;
        }).Schedule(inputDeps);

        return jh;
    }
}
