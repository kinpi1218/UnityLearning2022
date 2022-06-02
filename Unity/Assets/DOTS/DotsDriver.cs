using System.Collections;
using System.Collections.Generic;
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
    }

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

    public struct AddOneJob : IJob
    {
        public NativeArray<float> result;

        public void Execute()
        {
            result[0] = result[0] + 1;
        }
    }
}