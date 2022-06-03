using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Knpl.Dots
{
    public class DotsDriver : MonoBehaviour
    {
        // Start is called before the first frame update
        public void Execute00()
        {
            Debug.Log($"[{Time.frameCount}] Job Start");
            var result = new NativeArray<float>(1, Allocator.TempJob);

            var jobData = new MyJob();
            jobData.a = 10;
            jobData.b = 20;
            jobData.result = result;

            var handle = jobData.Schedule();
            Debug.Log($"[{Time.frameCount}] Job Schedule");
            handle.Complete();
            Debug.Log($"[{Time.frameCount}] Job Complete");

            var aPlusB = result[0];
            Debug.Log($"[{Time.frameCount}] {aPlusB}");

            result.Dispose();
            Debug.Log($"[{Time.frameCount}] Job End");
        }

        public void Execute01()
        {
            Debug.Log($"[{Time.frameCount}] Job Start");
            var result = new NativeArray<float>(1, Allocator.TempJob);

            var jobData = new MyJob
            {
                a = 10,
                b = 20,
                result = result
            };

            var handle = jobData.Schedule();
            var incJobData = new AddOneJob
            {
                result = result,
            };
            var secondHandle = incJobData.Schedule(handle);

            secondHandle.Complete();
            var aPlusB = result[0];
            Debug.Log($"[{Time.frameCount}] {aPlusB}");

            result.Dispose();
            Debug.Log($"[{Time.frameCount}] Job End");
        }

        public void Execute02()
        {
            Debug.Log($"[{Time.frameCount}] Job Start");
            var a = new NativeArray<float>(2, Allocator.TempJob);
            var b = new NativeArray<float>(2, Allocator.TempJob);
            var result = new NativeArray<float>(2, Allocator.TempJob);

            a[0] = 1.1f;
            b[0] = 2.2f;
            a[1] = 3.3f;
            b[1] = 4.4f;

            var jobData = new MyParallelJob
            {
                a = a,
                b = b,
                result = result
            };

            var handle = jobData.Schedule(result.Length, 1);
            handle.Complete();
            Debug.Log($"[{Time.frameCount}] {result[0]}, {result[1]}");

            a.Dispose();
            b.Dispose();
            result.Dispose();
            Debug.Log($"[{Time.frameCount}] Job End");
        }
    }

    [BurstCompile]
    public struct MyJob : IJob
    {
        public float a;
        public float b;
        public NativeArray<float> result;

        public void Execute()
        {
            result[0] = a + b;
        }
    }

    [BurstCompile]
    public struct AddOneJob : IJob
    {
        public NativeArray<float> result;

        public void Execute()
        {
            result[0] = result[0] + 1;
        }
    }

    [BurstCompile]
    public struct IncrementByDeltaTimeJob : IJobParallelFor
    {
        public NativeArray<float> values;
        public float deltaTime;

        public void Execute(int index)
        {
            var tmp = values[index];
            tmp += deltaTime;
            values[index] = tmp;
        }
    }

    [BurstCompile]
    public struct MyParallelJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> a;
        [ReadOnly] public NativeArray<float> b;
        public NativeArray<float> result;

        public void Execute(int i) => result[i] = a[i] + b[i];
    }
}