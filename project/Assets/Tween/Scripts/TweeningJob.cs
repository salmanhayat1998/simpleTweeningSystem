using System.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

public struct tweenStruct
{
    public AnimationCurve positionCurve;
    public AnimationCurve rotationCurve;
    public bool autoPlay;
    public float duration;
    public float playDelay;
    public Vector3 endPosition;
    public Quaternion endRotation;
    public bool loop;
    public LoopType loopType;
    public Space space;
    public Vector3 startPosition;
    public Quaternion startRotation;
    public float elapsedTime;
    public bool isPlaying;
}

public class TweeningJob : MonoBehaviour
{
    public TweenObject myTweenObj;

    public TweenObject[] tweenObjects;
    public Transform[] m_Transforms;



    float positionValue;
    float rotationValue;
    TransformAccessArray m_AccessArray;
    float t;


    /// Native Arrays /// 

    NativeArray<float> resultT;
    NativeArray<float> resultElap;
    NativeArray<bool> resultBool;
    NativeArray<LoopType> loopTypeResult;

    /// 

    NativeArray<Vector3> startPos;
    NativeArray<Vector3> endPos;
    NativeArray<Quaternion> startRot;
    NativeArray<Quaternion> endRot;
    NativeArray<tweenStruct> tweenStructs;

    MovementJob MovementJob;
    JobHandle jobHandle2;


    private void Awake()
    {
        myTweenObj = GetComponent<TweenObject>();
    }
    private void Start()
    {
        m_AccessArray = new TransformAccessArray(m_Transforms);
     
        for (int i = 0; i < tweenObjects.Length; i++)
        {
            var obj = tweenObjects[i];
            tweenStruct tweenStruct = new tweenStruct();
          //  tweenStruct.positionCurve = obj.positionCurve;
         //   tweenStruct.positionCurve = obj.positionCurve;
            tweenStruct.startPosition = obj.startPosition;
           // tweenStruct.startRotation = obj.startRotation;
            tweenStruct.autoPlay = obj.autoPlay;
            tweenStruct.playDelay = obj.playDelay;
            tweenStruct.duration = obj.duration;
            tweenStruct.endPosition = obj.endPosition;
         //   tweenStruct.endRotation = obj.endRotation;
            tweenStruct.loop = obj.loop;
            tweenStruct.loopType = obj.loopType;
            tweenStruct.space = obj.space;
            tweenStruct.elapsedTime = obj.elapsedTime;
            tweenStruct.isPlaying = obj.isPlaying;


            tweenStructs[i] = tweenStruct;
        }

        //for (int i = 0; i < m_Transforms.Length; i++)
        //{
        //    tweenObjects[i].startPosition = tweenObjects[i].space == Space.World ? tweenObjects[i].transform.position : tweenObjects[i].transform.localPosition;
        //    tweenObjects[i].startRotation = tweenObjects[i].space == Space.World ? tweenObjects[i].transform.rotation : tweenObjects[i].transform.localRotation;


        //    if (tweenObjects[i].autoPlay)
        //    {
        //        tweenObjects[i].Play();
        //        // Invoke("Play", playDelay);
        //    }
        //}

        myTweenObj.startPosition = myTweenObj.space == Space.World ? transform.position : transform.localPosition;
     //   myTweenObj.startRotation = myTweenObj.space == Space.World ? transform.rotation : transform.localRotation;


        if (myTweenObj.autoPlay)
        {
            myTweenObj.Play();
            //   Invoke("Play", playDelay);
        }

    }

    private void Update()
    {
        //for (int i = 0; i < tweenObjects.Length; i++)
        //{
        //playAnimation(tweenObjects[i]);

        //}

        if (myTweenObj.isPlaying)
        {
            resultT = new NativeArray<float>(m_Transforms.Length, Allocator.TempJob);
            resultElap = new NativeArray<float>(m_Transforms.Length, Allocator.TempJob);
            resultBool = new NativeArray<bool>(m_Transforms.Length, Allocator.TempJob);
            loopTypeResult = new NativeArray<LoopType>(m_Transforms.Length, Allocator.TempJob);

            // populate arrays //

            for (int k = 0; k < resultBool.Length; k++)
            {
                resultBool[k] = true;
            }
            for (int k = 0; k < resultElap.Length; k++)
            {
                resultElap[k] = myTweenObj.elapsedTime + Time.deltaTime;
            }
            for (int k = 0; k < loopTypeResult.Length; k++)
            {
                loopTypeResult[k] = myTweenObj.loopType;
            }
            AnimationCurveTCalculation animationCurveT = new AnimationCurveTCalculation();
            animationCurveT.duration = myTweenObj.duration;
            animationCurveT.loop = myTweenObj.loop;
            animationCurveT.loopTypeArr = loopTypeResult;
            animationCurveT.t = t;
            animationCurveT.resultT = resultT;
            animationCurveT.resultElap = resultElap;
            animationCurveT.resultBool = resultBool;



          //  positionValue = myTweenObj.positionCurve.Evaluate(t);
            //rotationValue = myTweenObj.rotationCurve.Evaluate(t);



            ///////////////////// /////////////////
            ///
            /////// Actual job for Moving Transform ////////////

            startPos = new NativeArray<Vector3>(m_Transforms.Length, Allocator.TempJob);
            endPos = new NativeArray<Vector3>(m_Transforms.Length, Allocator.TempJob);
            startRot = new NativeArray<Quaternion>(m_Transforms.Length, Allocator.TempJob);
            endRot = new NativeArray<Quaternion>(m_Transforms.Length, Allocator.TempJob);
            tweenStructs = new NativeArray<tweenStruct>(m_Transforms.Length, Allocator.Persistent);
            // populate arrays //

            for (int i = 0; i < tweenStructs.Length; i++)
            {
                for (var k = 0; k < startPos.Length; ++k)
                {
                    startPos[k] = tweenStructs[i].startPosition;
                }
                for (var k = 0; k < endPos.Length; ++k)
                {
                    endPos[k] = tweenStructs[i].endPosition;
                }
                for (var k = 0; k < startRot.Length; ++k)
                {
                    startRot[k] = tweenStructs[i].startRotation;
                }
                for (var k = 0; k < endRot.Length; ++k)
                {
                    endRot[k] = tweenStructs[i].endRotation;
                }



                MovementJob = new MovementJob()
                {
                    startPosition = startPos,
                    endPosition = endPos,
                    startRotation = startRot,
                    endRotation = endRot,
                    space = tweenStructs[i].space,
                    _startPosition = tweenStructs[i].startPosition,
                    _endPosition = tweenStructs[i].endPosition,
                    _startRotation = tweenStructs[i].startRotation,
                    _endRotation = tweenStructs[i].endRotation,
                    moveSpeed = positionValue,
                    rotationSpeed = rotationValue,
                    loopTypeArr = loopTypeResult,
                    deltaTime = Time.deltaTime,
                };

                JobHandle handle = animationCurveT.Schedule(m_Transforms.Length, 10);
                jobHandle2 = MovementJob.Schedule(m_AccessArray, handle);



                jobHandle2.Complete();
                if (jobHandle2.IsCompleted)
                {
                    var tp = tweenStructs[i];
                    tp.loopType = loopTypeResult[0];
                    tp.elapsedTime = resultElap[0];
                    t = resultT[0];
                    tp.isPlaying = resultBool[0];

                    if (tweenStructs[i].loopType == LoopType.Incremental)
                    {
                        tp.endPosition = endPos[0];
                        tp.endRotation = endRot[0];
                    }

                    resultT.Dispose();
                    resultElap.Dispose();
                    resultBool.Dispose();

                    startPos.Dispose();
                    endPos.Dispose();
                    startRot.Dispose();
                    endRot.Dispose();
                    loopTypeResult.Dispose();
                }
            }
        }
    }
    //if (myTweenObj.isPlaying)
    //{

    //    //////// Animation Curve Evaluation ///////////

    //    resultT = new NativeArray<float>(m_Transforms.Length, Allocator.TempJob);
    //    resultElap = new NativeArray<float>(m_Transforms.Length, Allocator.TempJob);
    //    resultBool = new NativeArray<bool>(m_Transforms.Length, Allocator.TempJob);
    //    loopTypeResult = new NativeArray<LoopType>(m_Transforms.Length, Allocator.TempJob);

    //    // populate arrays //

    //    for (int k = 0; k < resultBool.Length; k++)
    //    {
    //        resultBool[k] = true;
    //    }
    //    for (int k = 0; k < resultElap.Length; k++)
    //    {
    //        resultElap[k] = myTweenObj.elapsedTime + Time.deltaTime;
    //    }
    //    for (int k = 0; k < loopTypeResult.Length; k++)
    //    {
    //        loopTypeResult[k] = myTweenObj.loopType;
    //    }
    //    AnimationCurveTCalculation animationCurveT = new AnimationCurveTCalculation();
    //    animationCurveT.duration = myTweenObj.duration;
    //    animationCurveT.loop = myTweenObj.loop;
    //    animationCurveT.loopTypeArr = loopTypeResult;
    //    animationCurveT.t = t;
    //    animationCurveT.resultT = resultT;
    //    animationCurveT.resultElap = resultElap;
    //    animationCurveT.resultBool = resultBool;



    //    positionValue = myTweenObj.positionCurve.Evaluate(t);
    //    rotationValue = myTweenObj.rotationCurve.Evaluate(t);



    //    ///////////////////// /////////////////
    //    ///
    //    /////// Actual job for Moving Transform ////////////

    //    startPos = new NativeArray<Vector3>(m_Transforms.Length, Allocator.TempJob);
    //    endPos = new NativeArray<Vector3>(m_Transforms.Length, Allocator.TempJob);
    //    startRot = new NativeArray<Quaternion>(m_Transforms.Length, Allocator.TempJob);
    //    endRot = new NativeArray<Quaternion>(m_Transforms.Length, Allocator.TempJob);

    //    // populate arrays //

    //    for (var k = 0; k < startPos.Length; ++k)
    //    {
    //        startPos[k] = myTweenObj.startPosition;
    //    }
    //    for (var k = 0; k < endPos.Length; ++k)
    //    {
    //        endPos[k] = myTweenObj.endPosition;
    //    }
    //    for (var k = 0; k < startRot.Length; ++k)
    //    {
    //        startRot[k] = myTweenObj.startRotation;
    //    }
    //    for (var k = 0; k < endRot.Length; ++k)
    //    {
    //        endRot[k] = myTweenObj.endRotation;
    //    }

    //    MovementJob = new MovementJob()
    //    {
    //        startPosition = startPos,
    //        endPosition = endPos,
    //        startRotation = startRot,
    //        endRotation = endRot,
    //        space = myTweenObj.space,
    //        _startPosition = myTweenObj.startPosition,
    //        _endPosition = myTweenObj.endPosition,
    //        _startRotation = myTweenObj.startRotation,
    //        _endRotation = myTweenObj.endRotation,
    //        moveSpeed = positionValue,
    //        rotationSpeed = rotationValue,
    //        loopTypeArr = loopTypeResult,
    //        deltaTime = Time.deltaTime,
    //    };


    //    JobHandle handle = animationCurveT.Schedule(m_Transforms.Length, 10);
    //    jobHandle2 = MovementJob.Schedule(m_AccessArray, handle);
    //    jobHandle2.Complete();
    //    myTweenObj.loopType = loopTypeResult[0];
    //    myTweenObj.elapsedTime = resultElap[0];
    //    t = resultT[0];
    //    myTweenObj.isPlaying = resultBool[0];

    //    if (myTweenObj.loopType == LoopType.Incremental)
    //    {
    //        myTweenObj.endPosition = endPos[0];
    //        myTweenObj.endRotation = endRot[0];
    //    }

    //    resultT.Dispose();
    //    resultElap.Dispose();
    //    resultBool.Dispose();

    //    startPos.Dispose();
    //    endPos.Dispose();
    //    startRot.Dispose();
    //    endRot.Dispose();
    //    loopTypeResult.Dispose();
    //}

    //   playAnimation(myTweenObj);



    //#region tes 


    //void playAnimation(TweenObject tweenObject)
    //{

    //    if (tweenObject.isPlaying)
    //    {

    //        //////// Animation Curve Evaluation ///////////

    //        resultT = new NativeArray<float>(1, Allocator.TempJob);
    //        resultElap = new NativeArray<float>(1, Allocator.TempJob);
    //        resultBool = new NativeArray<bool>(1, Allocator.TempJob);
    //        loopTypeResult = new NativeArray<LoopType>(1, Allocator.TempJob);

    //        // populate arrays //

    //        for (int k = 0; k < resultBool.Length; k++)
    //        {
    //            resultBool[k] = true;
    //        }


    //        for (int k = 0; k < resultElap.Length; k++)
    //        {
    //            resultElap[k] = tweenObject.elapsedTime + Time.deltaTime;
    //        }
    //        for (int k = 0; k < loopTypeResult.Length; k++)
    //        {
    //            loopTypeResult[k] = tweenObject.loopType;
    //        }
    //        AnimationCurveTCalculation animationCurveT = new AnimationCurveTCalculation();
    //        animationCurveT.duration = tweenObject.duration;
    //        animationCurveT.loop = tweenObject.loop;
    //        animationCurveT.loopTypeArr = loopTypeResult;
    //        animationCurveT.t = t;
    //        animationCurveT.resultT = resultT;
    //        animationCurveT.resultElap = resultElap;
    //        animationCurveT.resultBool = resultBool;


    //        JobHandle handle = animationCurveT.Schedule(2,10);
    //        positionValue = tweenObject.positionCurve.Evaluate(t);
    //        rotationValue = tweenObject.rotationCurve.Evaluate(t);



    //        ///////////////////// /////////////////
    //        ///
    //        /////// Actual job for Moving Transform ////////////

    //        startPos = new NativeArray<Vector3>(m_AccessArray.length, Allocator.TempJob);
    //        endPos = new NativeArray<Vector3>(m_AccessArray.length, Allocator.TempJob);
    //        startRot = new NativeArray<Quaternion>(m_AccessArray.length, Allocator.TempJob);
    //        endRot = new NativeArray<Quaternion>(m_AccessArray.length, Allocator.TempJob);

    //        // populate arrays //

    //        for (var k = 0; k < startPos.Length; ++k)
    //        {
    //            startPos[k] = tweenObject.startPosition;
    //        }
    //        for (var k = 0; k < endPos.Length; ++k)
    //        {
    //            endPos[k] = tweenObject.endPosition;
    //        }
    //        for (var k = 0; k < startRot.Length; ++k)
    //        {
    //            startRot[k] = tweenObject.startRotation;
    //        }
    //        for (var k = 0; k < endRot.Length; ++k)
    //        {
    //            endRot[k] = tweenObject.endRotation;
    //        }



    //        MovementJob = new MovementJob()
    //        {
    //            startPosition = startPos,
    //            endPosition = endPos,
    //            startRotation = startRot,
    //            endRotation = endRot,
    //            space = tweenObject.space,
    //            _startPosition = tweenObject.startPosition,
    //            _endPosition = tweenObject.endPosition,
    //            _startRotation = tweenObject.startRotation,
    //            _endRotation = tweenObject.endRotation,
    //            moveSpeed = positionValue,
    //            rotationSpeed = rotationValue,
    //            loopTypeArr = loopTypeResult,
    //            deltaTime = Time.deltaTime,
    //        };

    //        jobHandle2 = MovementJob.Schedule(m_AccessArray, handle);
    //        jobHandle2.Complete();
    //        tweenObject.loopType = loopTypeResult[0];
    //        tweenObject.elapsedTime = resultElap[0];
    //        t = resultT[0];
    //        tweenObject.isPlaying = resultBool[0];

    //        if (tweenObject.loopType == LoopType.Incremental)
    //        {
    //            tweenObject.endPosition = endPos[0];
    //            tweenObject.endRotation = endRot[0];
    //        }

    //        resultT.Dispose();
    //        resultElap.Dispose();
    //        resultBool.Dispose();

    //        startPos.Dispose();
    //        endPos.Dispose();
    //        startRot.Dispose();
    //        endRot.Dispose();
    //        loopTypeResult.Dispose();
    //    }
    //}

    //#endregion



    void OnDestroy()
    {
        // TransformAccessArrays must be disposed manually. 
        m_AccessArray.Dispose();
    }

}

[BurstCompile]
public struct MovementJob : IJobParallelForTransform
{
    [ReadOnly]
    public NativeArray<Vector3> startPosition;
    [ReadOnly]
    public NativeArray<Vector3> endPosition;
    [ReadOnly]
    public NativeArray<Quaternion> startRotation;
    [ReadOnly]
    public NativeArray<Quaternion> endRotation;
    [ReadOnly]
    public NativeArray<LoopType> loopTypeArr;

    public NativeArray<tweenStruct> tweenStructs;
    //   public LoopType loopType;
    public float moveSpeed;
    public float rotationSpeed;
    public float deltaTime;
    public Space space;
    public Vector3 _startPosition; // pas inspector value
    public Vector3 _endPosition; // pas inspector value
    public Quaternion _startRotation; // pas inspector value
    public Quaternion _endRotation; // pas inspector value
    public void Execute(int index, TransformAccess transform)
    {
        //if (loopTypeArr[index] == LoopType.Incremental)
        //{

        //    startPosition[index] = space == Space.World ? transform.position : transform.localPosition;
        //    endPosition[index] = startPosition[index] + endPosition[index];
        //    startRotation[index] = space == Space.World ? transform.rotation : transform.localRotation;
        //    endRotation[index] = startRotation[index] * endRotation[index];
        //}

        //if (space == Space.World)
        //{
        //    transform.position = Vector3.Lerp(startPosition[index], endPosition[index], moveSpeed);
        //    transform.rotation = Quaternion.Lerp(startRotation[index], endRotation[index], rotationSpeed);
        //}
        //else
        //{
        //    transform.localPosition = Vector3.Lerp(startPosition[index], endPosition[index], moveSpeed);
        //    transform.localRotation = Quaternion.Lerp(startRotation[index], endRotation[index], rotationSpeed);
        //}
        
        
        if (loopTypeArr[index] == LoopType.Incremental)
        {

            startPosition[index] = space == Space.World ? transform.position : transform.localPosition;
            endPosition[index] = startPosition[index] + endPosition[index];
            startRotation[index] = space == Space.World ? transform.rotation : transform.localRotation;
            endRotation[index] = startRotation[index] * endRotation[index];
        }

        if (space == Space.World)
        {
            transform.position = Vector3.Lerp(startPosition[index], endPosition[index], moveSpeed);
            transform.rotation = Quaternion.Lerp(startRotation[index], endRotation[index], rotationSpeed);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(startPosition[index], endPosition[index], moveSpeed);
            transform.localRotation = Quaternion.Lerp(startRotation[index], endRotation[index], rotationSpeed);
        }
    }
}

[BurstCompile]
public struct AnimationCurveTCalculation : IJobParallelFor
{
    public float duration;

    public float t;
    public bool loop;

    [ReadOnly]
    public NativeArray<LoopType> loopTypeArr;
    [WriteOnly]
    public NativeArray<float> resultT;
    [ReadOnly]
    public NativeArray<float> resultElap;
    [ReadOnly]
    public NativeArray<bool> resultBool;
    public void Execute(int index)
    {

        t = resultElap[index] / duration;
        if (loopTypeArr[index] == LoopType.PingPong)
        {
            t = Mathf.PingPong(t, 1f);
        }
        else if (loopTypeArr[index] == LoopType.Incremental)
        {
            t = (resultElap[index] % duration) / duration;
        }
        else
        {
            t = t % 1f;
        }

        if (resultElap[index] >= duration)
        {
            if (loop)
            {
                switch (loopTypeArr[index])
                {
                    case LoopType.Restart:
                        resultElap[index] = 0f;
                        break;
                    case LoopType.Incremental:
                        resultElap[index] = resultElap[index] % duration;
                        break;
                }
            }
            else
            {
                resultBool[index] = false;
            }
        }
        resultT[index] = t;
    }
}

