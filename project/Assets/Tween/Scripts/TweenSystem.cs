using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;

public class TweenSystem : MonoBehaviour
{
    public TweenObject[] targets;

    private void Awake()
    {
        targets = FindObjectsOfType<TweenObject>();
    }

    private void Update()
    {
        #region Native Arrays
        NativeArray<float> resultT = new NativeArray<float>(targets.Length, Allocator.TempJob);
        NativeArray<float> resultElap = new NativeArray<float>(targets.Length, Allocator.TempJob);
        NativeArray<bool> resultBool = new NativeArray<bool>(targets.Length, Allocator.TempJob);
        NativeArray<float> durations = new NativeArray<float>(targets.Length, Allocator.TempJob);
        NativeArray<float> positionSpeed = new NativeArray<float>(targets.Length, Allocator.TempJob);
        NativeArray<float> rotationSpeed = new NativeArray<float>(targets.Length, Allocator.TempJob);
        NativeArray<float> timeDeltas = new NativeArray<float>(targets.Length, Allocator.TempJob);
        NativeArray<bool> isloop = new NativeArray<bool>(targets.Length, Allocator.TempJob);
        NativeArray<LoopType> loopTypeResult = new NativeArray<LoopType>(targets.Length, Allocator.TempJob);


        NativeArray<float3> myPositions = new NativeArray<float3>(targets.Length, Allocator.TempJob);
        NativeArray<float3> myStartPositions = new NativeArray<float3>(targets.Length, Allocator.TempJob);
        NativeArray<float3> myendPositions = new NativeArray<float3>(targets.Length, Allocator.TempJob);
        NativeArray<float3> myRotations = new NativeArray<float3>(targets.Length, Allocator.TempJob);
        NativeArray<float3> myStartRotations = new NativeArray<float3>(targets.Length, Allocator.TempJob);
        NativeArray<float3> myendRotation = new NativeArray<float3>(targets.Length, Allocator.TempJob);
        #endregion

        ///
        // Populate arrays
        ///
        #region Populating Arrays
        for (int k = 0; k < resultBool.Length; k++)
        {
            resultBool[k] = true;
        }     
        for (int k = 0; k < durations.Length; k++)
        {
            durations[k] = targets[k].duration;
        }    
        for (int k = 0; k < isloop.Length; k++)
        {
            isloop[k] = targets[k].loop;
        }
        for (int k = 0; k < resultElap.Length; k++)
        {
            resultElap[k] = targets[k].elapsedTime + Time.deltaTime;
        }
        for (int k = 0; k < loopTypeResult.Length; k++)
        {
            loopTypeResult[k] = targets[k].loopType;
        }
        for (int k = 0; k < timeDeltas.Length; k++)
        {
            timeDeltas[k] = 0;
        }     
        for (int k = 0; k < resultT.Length; k++)
        {
            resultT[k] = 0;
        } 
        for (int k = 0; k < positionSpeed.Length; k++)
        {
             positionSpeed[k] = 0;
           // positionSpeed[k] = timeDeltas[k];
        }    
        for (int k = 0; k < rotationSpeed.Length; k++)
        {
            rotationSpeed[k] = 0;
        }

       
        ////////// Second JOB Arrays ////////////////

   
        for (int i = 0; i < myPositions.Length; i++)
        {
            myPositions[i] = targets[i].transform.position;
        } 
        for (int i = 0; i < myStartPositions.Length; i++)
        {
            myStartPositions[i] = targets[i].startPosition;
        }

        for (int i = 0; i < myendPositions.Length; i++)
        {
            myendPositions[i] = targets[i].endPosition;
        }
        for (int i = 0; i < myRotations.Length; i++)
        {
            myRotations[i] = targets[i].transform.eulerAngles;
        }  
        for (int i = 0; i < myStartRotations.Length; i++)
        {
            myStartRotations[i] = targets[i].startRotation;
        }
        for (int i = 0; i < myendRotation.Length; i++)
        {
            myendRotation[i] = targets[i].endRotation;
        }

        #endregion
        firstJob job1 = new firstJob()
        {
            duration = durations,
            loop = isloop,
            loopTypeArr = loopTypeResult,
            timeDelta = timeDeltas,
            resultT = resultT,
            resultElap = resultElap,
            resultBool = resultBool
        };

        JobHandle job0 = job1.Schedule(targets.Length, 1);

        job0.Complete();

  

        TweenJob tweenJob = new TweenJob()
        {
            position = myPositions,
            startposition= myStartPositions,
            endPosition = myendPositions,
            rotation = myRotations,
            moveSpeed= timeDeltas,
          //  moveSpeed=positionSpeed,
            startRotation=myStartRotations,
            rotationSpeed=rotationSpeed,
            endRotation = myendRotation,
            
        };
        //for (int i = 0; i < targets.Length; i++)
        //{
        //    positionSpeed[i] = targets[i].positionCurve.Evaluate(timeDeltas[i]);
        //    rotationSpeed[i] = targets[i].rotationCurve.Evaluate(timeDeltas[i]);
        //}

        JobHandle tweenJobHandle = tweenJob.Schedule(targets.Length, 1);
        tweenJobHandle.Complete();



        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].transform.position = myPositions[i];
            targets[i].transform.eulerAngles = myRotations[i];

            targets[i].loopType = loopTypeResult[i];
            targets[i].elapsedTime = resultElap[i];
            timeDeltas[i] = resultT[i];


         
        }


        #region Disposing Arrays

        isloop.Dispose();
        durations.Dispose();
        positionSpeed.Dispose();
        rotationSpeed.Dispose();
        timeDeltas.Dispose();
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
        #endregion
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
    [ReadOnly]public NativeArray<float> moveSpeed;
    [ReadOnly]public NativeArray<float> rotationSpeed;
    public void Execute(int index)
    {
        position[index] = Vector3.Lerp(startposition[index], endPosition[index], moveSpeed[index]);
        rotation[index] = Vector3.Lerp(startRotation[index], endRotation[index], rotationSpeed[index]);
    }
}
