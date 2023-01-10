using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
public class testing : MonoBehaviour
{
    [SerializeField]
    float r,elapseTime;
    void Update()
    {
        NativeArray<float> result1 = new NativeArray<float>(1, Allocator.TempJob);
        NativeArray<float> result2 = new NativeArray<float>(1, Allocator.TempJob);
        testJob test = new testJob()
        {
            a=r,
            etime=elapseTime,
            deltaTime = Time.deltaTime,
            result1 = result1,
            result2 = result2
        };
        JobHandle handle = test.Schedule();
        handle.Complete();     
        JobHandle handle2 = test.Schedule();
        handle2.Complete();
         elapseTime = result1[0];
         r = result2[0];
        result1.Dispose();
        result2.Dispose();
    }
}
[BurstCompile]
public struct testJob : IJob
{
    public float a;
    public float etime;
    public float deltaTime;
    public NativeArray<float> result1;
    public NativeArray<float> result2;
    public void Execute()
    {
        etime+= deltaTime;
        a = etime / 2;
        result1[0] = etime;
        result2[0] = a;

    }
}