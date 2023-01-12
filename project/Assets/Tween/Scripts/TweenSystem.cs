using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class TweenSystem : MonoBehaviour
{
    public Transform target;
    public Vector3 endPosition;
    public Quaternion endRotation;
    public float duration = 1f;
    private float currentTime = 0f;
    private Vector3 startPosition;
    private Quaternion startRotation;
    NativeArray<Vector3> startPositions;
    NativeArray<Vector3> endPositions;
    NativeArray<Quaternion> startRotations ;
    NativeArray<Quaternion> endRotations ;
    NativeArray<float> currentTimes ;
    private struct TweenJob : IJobParallelFor
    {
        public float deltaTime;
        public float duration;
        public NativeArray<Vector3> startPositions;
        public NativeArray<Vector3> endPositions;
        public NativeArray<Quaternion> startRotations;
        public NativeArray<Quaternion> endRotations;
        public NativeArray<float> currentTimes;

        public void Execute(int index)
        {
            currentTimes[index] += deltaTime;
            float t = currentTimes[index] / duration;
            t = math.clamp(t, 0f, 1f);
            startPositions[index] = Vector3.Lerp(startPositions[index], endPositions[index], t);
            startRotations[index] = Quaternion.Lerp(startRotations[index], endRotations[index], t);
        }
    }

    private JobHandle tweenJobHandle;

    private void Start()
    {
        startPosition = target.position;
        startRotation = target.rotation;
    }

    private void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= duration)
        {
            currentTime = duration;
            tweenJobHandle.Complete();
        }
        else
        {
             startPositions = new NativeArray<Vector3>(1, Allocator.TempJob);
            endPositions = new NativeArray<Vector3>(1, Allocator.TempJob);
            startRotations = new NativeArray<Quaternion>(1, Allocator.TempJob);
             endRotations = new NativeArray<Quaternion>(1, Allocator.TempJob);
            currentTimes = new NativeArray<float>(1, Allocator.TempJob);

            startPositions[0] = startPosition;
            endPositions[0] = endPosition;
            startRotations[0] = startRotation;
            endRotations[0] = endRotation;
            currentTimes[0] = currentTime;

            TweenJob tweenJob = new TweenJob()
            {
                deltaTime = Time.deltaTime,
                duration = duration,
                startPositions = startPositions,
                endPositions = endPositions,
                startRotations = startRotations,
                endRotations = endRotations,
                currentTimes = currentTimes
            };
             tweenJobHandle = tweenJob.Schedule(1,100);

        }
    }

    private void LateUpdate()
    {
        if (currentTime >= duration)
        {
            tweenJobHandle.Complete();
            target.position = endPosition;
            target.rotation = endRotation;
            startPositions.Dispose();
            endPositions.Dispose();
            startRotations.Dispose();
            endRotations.Dispose();
            currentTimes.Dispose();
            this.enabled = false;
        }
    }
}