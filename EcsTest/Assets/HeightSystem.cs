using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial class NoiseHeightSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var time = Convert.ToSingle(Time.ElapsedTime);
        Entities.ForEach((ref Translation translation) =>
        {
            var pos = translation.Value;
            translation.Value.y = 5 * math.sin(math.PI * (time + (pos.x + pos.z) * 0.025f));
        }).ScheduleParallel();
    }
}