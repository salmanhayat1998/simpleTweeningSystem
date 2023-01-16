using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;
using static UnityEngine.Networking.UnityWebRequest;
using System.Runtime.CompilerServices;
using UnityEngine.Jobs;

public class TweenSystem : MonoBehaviour
{
    public bool useJobs;
    public TweenObject[] targets;
    NativeArray<float> resultT;
    NativeArray<float> resultElap;
    NativeArray<bool> resultBool;
    NativeArray<float> durations;
    NativeArray<float> positionSpeed;
    NativeArray<float> rotationSpeed;
    NativeArray<float> timeDeltas;
    NativeArray<float> timeDeltas2;
    NativeArray<bool> isloop;
    NativeArray<LoopType> loopTypeResult;


    NativeArray<float3> myPositions;
    NativeArray<float3> myStartPositions;
    NativeArray<float3> myendPositions;
    NativeArray<float3> myRotations;
    NativeArray<float3> myStartRotations;
    NativeArray<float3> myendRotation;
    TransformAccessArray m_Array;
    private void Awake()
    {
        targets = FindObjectsOfType<TweenObject>();
    }
    JobHandle j0,j1, j2,jfinal;
    eTime job00;
    firstJob job01;
    TweenJob tweenJob;

    private void Start()
    {
        if (useJobs)
        {


            resultT = new NativeArray<float>(targets.Length, Allocator.Persistent);
            resultElap = new NativeArray<float>(targets.Length, Allocator.Persistent);
            resultBool = new NativeArray<bool>(targets.Length, Allocator.Persistent);
            durations = new NativeArray<float>(targets.Length, Allocator.Persistent);
            positionSpeed = new NativeArray<float>(targets.Length, Allocator.Persistent);
            rotationSpeed = new NativeArray<float>(targets.Length, Allocator.Persistent);
            timeDeltas = new NativeArray<float>(targets.Length, Allocator.Persistent);
            timeDeltas2 = new NativeArray<float>(targets.Length, Allocator.Persistent);
            isloop = new NativeArray<bool>(targets.Length, Allocator.Persistent);
            loopTypeResult = new NativeArray<LoopType>(targets.Length, Allocator.Persistent);


            myPositions = new NativeArray<float3>(targets.Length, Allocator.Persistent);
            myStartPositions = new NativeArray<float3>(targets.Length, Allocator.Persistent);
            myendPositions = new NativeArray<float3>(targets.Length, Allocator.Persistent);
            myRotations = new NativeArray<float3>(targets.Length, Allocator.Persistent);
            myStartRotations = new NativeArray<float3>(targets.Length, Allocator.Persistent);
            myendRotation = new NativeArray<float3>(targets.Length, Allocator.Persistent);
            m_Array =new TransformAccessArray(targets.Length);
            for (int i = 0; i < targets.Length; i++)
            {
                resultBool[i] = true;
                durations[i] = targets[i].duration;
                isloop[i] = targets[i].loop;
                // resultElap[i] = targets[i].elapsedTime + Time.deltaTime;
                resultElap[i] = targets[i].elapsedTime;
                loopTypeResult[i] = targets[i].loopType;
                timeDeltas[i] = 0;
                timeDeltas2[i] = 0;
                resultT[i] = 0;
                positionSpeed[i] = 0;
                rotationSpeed[i] = 0;
                myPositions[i] = targets[i].transform.position;
                myStartPositions[i] = targets[i].transform.position;
                myendPositions[i] = targets[i].transform.position + targets[i].endPosition;
                myRotations[i] = targets[i].transform.eulerAngles;
                myStartRotations[i] = targets[i].startRotation;
                myendRotation[i] = targets[i].endRotation;


            }
            job00 = new eTime()
            {
                timeDelta = Time.deltaTime,
                resultTimeElapse = resultElap
            };

            job01 = new firstJob()
            {
                duration = durations,
                loop = isloop,
                loopTypeArr = loopTypeResult,
                timeDelta = timeDeltas,
                resultT = resultT,
                resultElap = resultElap,
                resultBool = resultBool
            };
            tweenJob = new TweenJob()
            {
                position = myPositions,
                startposition = myStartPositions,
                endPosition = myendPositions,
                rotation = myRotations,
                moveSpeed = timeDeltas,
                //  moveSpeed=positionSpeed,
                startRotation = myStartRotations,
                //  rotationSpeed = rotationSpeed,
                rotationSpeed = timeDeltas2,
                endRotation = myendRotation,

            };   


         
        }
        else
        {
            for (int i = 0; i < targets.Length; i++)
            {

            targets[i].startPosition = targets[i].space == Space.World ? targets[i].transform.position : targets[i].transform.localPosition;
                targets[i].startRotation = targets[i].space == Space.World ? targets[i].transform.eulerAngles : targets[i].transform.localEulerAngles;
            }
        }
    }
    private void Update()
    {
        #region Native Arrays
        //NativeArray<float> resultT = new NativeArray<float>(targets.Length, Allocator.TempJob);
        //NativeArray<float> resultElap = new NativeArray<float>(targets.Length, Allocator.TempJob);
        //NativeArray<bool> resultBool = new NativeArray<bool>(targets.Length, Allocator.TempJob);
        //NativeArray<float> durations = new NativeArray<float>(targets.Length, Allocator.TempJob);
        //NativeArray<float> positionSpeed = new NativeArray<float>(targets.Length, Allocator.TempJob);
        //NativeArray<float> rotationSpeed = new NativeArray<float>(targets.Length, Allocator.TempJob);
        //NativeArray<float> timeDeltas = new NativeArray<float>(targets.Length, Allocator.TempJob);
        //NativeArray<bool> isloop = new NativeArray<bool>(targets.Length, Allocator.TempJob);
        //NativeArray<LoopType> loopTypeResult = new NativeArray<LoopType>(targets.Length, Allocator.TempJob);


        //NativeArray<float3> myPositions = new NativeArray<float3>(targets.Length, Allocator.TempJob);
        //NativeArray<float3> myStartPositions = new NativeArray<float3>(targets.Length, Allocator.TempJob);
        //NativeArray<float3> myendPositions = new NativeArray<float3>(targets.Length, Allocator.TempJob);
        //NativeArray<float3> myRotations = new NativeArray<float3>(targets.Length, Allocator.TempJob);
        //NativeArray<float3> myStartRotations = new NativeArray<float3>(targets.Length, Allocator.TempJob);
        //NativeArray<float3> myendRotation = new NativeArray<float3>(targets.Length, Allocator.TempJob);
        #endregion

        if (useJobs)
        {


            //j0 = job00.Schedule(targets.Length, 200);
            ////j0.Complete();
            //j1 = job01.Schedule(targets.Length, 200, j0);
            //j1.Complete();
            j0 = job00.Schedule(targets.Length, 100);
            j1 = job01.Schedule(targets.Length, 100,j0);
            j2 = tweenJob.Schedule(targets.Length, 100,j1);
           // jfinal = JobHandle.CombineDependencies(j0, j1, j2);
            //jfinal.Complete();
            //j2 = tweenJob.Schedule(targets.Length, 200, j1);
            //j2.Complete();
            if (j2.IsCompleted)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    targets[i].transform.position = myPositions[i];
                    targets[i].transform.eulerAngles = myRotations[i];

                    //targets[i].loopType = loopTypeResult[i];
                    // targets[i].elapsedTime = resultElap[i];
                    // timeDeltas[i] = resultT[i];



                }

            }
            #region Disposing Arrays

            //isloop.Dispose();
            //durations.Dispose();
            //positionSpeed.Dispose();
            //rotationSpeed.Dispose();
            //timeDeltas.Dispose();
            //resultT.Dispose();
            //resultElap.Dispose();
            //resultBool.Dispose();
            //loopTypeResult.Dispose();
            //myPositions.Dispose();
            //myStartPositions.Dispose();
            //myendPositions.Dispose();
            //myRotations.Dispose();
            //myStartRotations.Dispose();
            //myendRotation.Dispose();
            #endregion
        }
        
        else
        {

            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].elapsedTime += Time.deltaTime;
                float t = targets[i].elapsedTime / targets[i].duration;
                if (targets[i].loopType == LoopType.PingPong)
                {
                    t = Mathf.PingPong(t, 1f);
                }
                else
                {
                    t = t % 1f;
                }

                //positionValue = positionCurve.Evaluate(t);
                //rotationValue = rotationCurve.Evaluate(t);  
                //targets[i].positionValue = t;
                //targets[i].rotationValue = t;

                //if (targets[i].loopType == LoopType.Incremental)
                //{
                //    t = (targets[i].elapsedTime % targets[i].duration) / targets[i].duration;
                //}

                //if (targets[i].loopType == LoopType.Incremental)
                //{
                //    targets[i].startPosition = targets[i].space == Space.World ? transform.position : transform.localPosition;
                //    targets[i].endPosition = targets[i].startPosition + targets[i].endPosition;
                //    targets[i].startRotation = targets[i].space == Space.World ? transform.eulerAngles : transform.localEulerAngles;
                //    targets[i].endRotation = targets[i].startRotation + targets[i].endRotation;
                //}

                if (targets[i].space == Space.World)
                {
                    targets[i].transform.position = Vector3.Lerp(targets[i].startPosition, targets[i].endPosition, t);
                    targets[i].transform.eulerAngles = Vector3.Lerp(targets[i].startRotation, targets[i].endRotation, t);
                }
                //else
                //{
                //    transform.localPosition = Vector3.Lerp(targets[i].startPosition, targets[i].endPosition, t);
                //    transform.localEulerAngles = Vector3.Lerp(targets[i].startRotation, targets[i].endRotation, t);
                //}

                if (targets[i].elapsedTime >= targets[i].duration)
                {
                    if (targets[i].loop)
                    {
                        switch (targets[i].loopType)
                        {
                            case LoopType.Restart:
                                targets[i].elapsedTime = 0f;
                                break;
                            case LoopType.Incremental:
                                targets[i].elapsedTime = targets[i].elapsedTime % targets[i].duration;
                                break;
                        }
                    }
                    else
                    {
                        targets[i].isPlaying = false;
                    }
                }
            }
        }
    }
    private void LateUpdate()
    {
        //j0.Complete();
        //j1.Complete();
        j2.Complete();
    }

    private void OnDestroy()
    {

        isloop.Dispose();
        durations.Dispose();
        positionSpeed.Dispose();
        rotationSpeed.Dispose();
        timeDeltas.Dispose();
        timeDeltas2.Dispose();
        resultT.Dispose();
        resultElap.Dispose();
        resultBool.Dispose();
        loopTypeResult.Dispose();
        myPositions.Dispose();
        myStartPositions.Dispose();
        myendPositions.Dispose();
        myRotations.Dispose();
        myStartRotations.Dispose();
        myendRotation.Dispose();
        m_Array.Dispose();
    }
}
[BurstCompile]
public struct eTime : IJobParallelFor
{
    public NativeArray<float> resultTimeElapse;
    public  float timeDelta;
    public void Execute(int index)
    {
        resultTimeElapse[index] += timeDelta; 
    }
}

[BurstCompile]
public struct firstJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<float> duration;
    public NativeArray<float> timeDelta;

    [ReadOnly]
    public NativeArray<bool> loop;

    // [ReadOnly]
    public NativeArray<LoopType> loopTypeArr;
    //  [WriteOnly]
    public NativeArray<float> resultT;

    public NativeArray<float> resultElap;
    [ReadOnly]
    public NativeArray<bool> resultBool;
    public void Execute(int index)
    {

        timeDelta[index] = resultElap[index] / duration[index];
        if (loopTypeArr[index] == LoopType.PingPong)
        {
            timeDelta[index] = Mathf.PingPong(timeDelta[index], 1f);
        }
        else if (loopTypeArr[index] == LoopType.Incremental)
        {
            timeDelta[index] = (resultElap[index] % duration[index]) / duration[index];
        }
        else
        {
            timeDelta[index] = timeDelta[index] % 1f;
        }

        if (resultElap[index] >= duration[index])
        {
            if (loop[index])
            {
                switch (loopTypeArr[index])
                {
                    case LoopType.Restart:
                        resultElap[index] = 0f;
                        break;
                    case LoopType.Incremental:
                        resultElap[index] = resultElap[index] % duration[index];
                        break;
                }
            }
            else
            {
                resultBool[index] = false;
            }
        }
        resultT[index] = timeDelta[index];
    }
}

[BurstCompile]
public struct TweenJob : IJobParallelFor
{

    [WriteOnly]
    public NativeArray<float3> position;
    [ReadOnly]
    public NativeArray<float3> startposition;
    [ReadOnly]
    public NativeArray<float3> endPosition;
    [WriteOnly]
    public NativeArray<float3> rotation;
    [ReadOnly]
    public NativeArray<float3> startRotation;
    [ReadOnly]
    public NativeArray<float3> endRotation;
    [ReadOnly] public NativeArray<float> moveSpeed;
    [ReadOnly] public NativeArray<float> rotationSpeed;
    public void Execute(int index)
    {
        position[index] = Vector3.Lerp(startposition[index], endPosition[index], moveSpeed[index]);
        rotation[index] = Vector3.Lerp(startRotation[index], endRotation[index], rotationSpeed[index]);
    }
}
