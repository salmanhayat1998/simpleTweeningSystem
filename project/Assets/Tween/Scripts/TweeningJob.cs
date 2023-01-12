using System.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

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

    MovementJob MovementJob;
    JobHandle jobHandle2;


    private void Awake()
    {
        myTweenObj = GetComponent<TweenObject>();
    }
    private void Start()
    {
         m_AccessArray = new TransformAccessArray(m_Transforms);
        //for (int i = 0; i < m_Transforms.Length; i++)
        //{
        //    tweenObjects[i].startPosition = tweenObjects[i].space == Space.World ? m_Transforms[i].position : m_Transforms[i].localPosition;
        //    tweenObjects[i].startRotation = tweenObjects[i].space == Space.World ? m_Transforms[i].rotation : m_Transforms[i].localRotation;


        //    if (tweenObjects[i].autoPlay)
        //    {
        //        tweenObjects[i].Play();
        //        Invoke("Play", playDelay);
        //    }
        //}

        myTweenObj.startPosition = myTweenObj.space == Space.World ? transform.position :transform.localPosition;
        myTweenObj.startRotation = myTweenObj.space == Space.World ? transform.rotation : transform.localRotation;


        if (myTweenObj.autoPlay)
        {
            myTweenObj.Play();
            //Invoke("Play", playDelay);
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

            //////// Animation Curve Evaluation ///////////

            resultT = new NativeArray<float>(1, Allocator.TempJob);
            resultElap = new NativeArray<float>(1, Allocator.TempJob);
            resultBool = new NativeArray<bool>(1, Allocator.TempJob);
            loopTypeResult = new NativeArray<LoopType>(1, Allocator.TempJob);

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


            JobHandle handle = animationCurveT.Schedule();
            positionValue = myTweenObj.positionCurve.Evaluate(t);
            rotationValue = myTweenObj.rotationCurve.Evaluate(t);



            ///////////////////// /////////////////
            ///
            /////// Actual job for Moving Transform ////////////

            startPos = new NativeArray<Vector3>(m_AccessArray.length, Allocator.TempJob);
            endPos = new NativeArray<Vector3>(m_AccessArray.length, Allocator.TempJob);
            startRot = new NativeArray<Quaternion>(m_AccessArray.length, Allocator.TempJob);
            endRot = new NativeArray<Quaternion>(m_AccessArray.length, Allocator.TempJob);

            // populate arrays //

            for (var k = 0; k < startPos.Length; ++k)
            {
                startPos[k] = myTweenObj.startPosition;
            }
            for (var k = 0; k < endPos.Length; ++k)
            {
                endPos[k] = myTweenObj.endPosition;
            }
            for (var k = 0; k < startRot.Length; ++k)
            {
                startRot[k] = myTweenObj.startRotation;
            }
            for (var k = 0; k < endRot.Length; ++k)
            {
                endRot[k] = myTweenObj.endRotation;
            }

            MovementJob = new MovementJob()
            {
                startPosition = startPos,
                endPosition = endPos,
                startRotation = startRot,
                endRotation = endRot,
                space = myTweenObj.space,
                _startPosition = myTweenObj.startPosition,
                _endPosition = myTweenObj.endPosition,
                _startRotation = myTweenObj.startRotation,
                _endRotation = myTweenObj.endRotation,
                moveSpeed = positionValue,
                rotationSpeed = rotationValue,
                loopTypeArr = loopTypeResult,
                deltaTime = Time.deltaTime,
            };

            jobHandle2 = MovementJob.Schedule(m_AccessArray, handle);
            jobHandle2.Complete();
            myTweenObj.loopType = loopTypeResult[0];
            myTweenObj.elapsedTime = resultElap[0];
            t = resultT[0];
            myTweenObj.isPlaying = resultBool[0];

            if (myTweenObj.loopType == LoopType.Incremental)
            {
                myTweenObj.endPosition = endPos[0];
                myTweenObj.endRotation = endRot[0];
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

        //   playAnimation(myTweenObj);

    }


    void playAnimation(TweenObject tweenObject)
    {

        if (tweenObject.isPlaying)
        {

            //////// Animation Curve Evaluation ///////////

            resultT = new NativeArray<float>(1, Allocator.TempJob);
            resultElap = new NativeArray<float>(1, Allocator.TempJob);
            resultBool = new NativeArray<bool>(1, Allocator.TempJob);
            loopTypeResult = new NativeArray<LoopType>(1, Allocator.TempJob);

            // populate arrays //

            for (int k = 0; k < resultBool.Length; k++)
            {
                resultBool[k] = true;
            }
            for (int k = 0; k < resultElap.Length; k++)
            {
                resultElap[k] = tweenObject.elapsedTime + Time.deltaTime;
            }
            for (int k = 0; k < loopTypeResult.Length; k++)
            {
                loopTypeResult[k] = tweenObject.loopType;
            }
            AnimationCurveTCalculation animationCurveT = new AnimationCurveTCalculation();
            animationCurveT.duration = tweenObject.duration;
            animationCurveT.loop = tweenObject.loop;
            animationCurveT.loopTypeArr = loopTypeResult;
            animationCurveT.t = t;
            animationCurveT.resultT = resultT;
            animationCurveT.resultElap = resultElap;
            animationCurveT.resultBool = resultBool;


            JobHandle handle = animationCurveT.Schedule();
            positionValue = tweenObject.positionCurve.Evaluate(t);
            rotationValue = tweenObject.rotationCurve.Evaluate(t);



            ///////////////////// /////////////////
            ///
            /////// Actual job for Moving Transform ////////////
            
            startPos = new NativeArray<Vector3>(m_AccessArray.length, Allocator.TempJob);
            endPos = new NativeArray<Vector3>(m_AccessArray.length, Allocator.TempJob);
            startRot = new NativeArray<Quaternion>(m_AccessArray.length, Allocator.TempJob);
            endRot = new NativeArray<Quaternion>(m_AccessArray.length, Allocator.TempJob);

            // populate arrays //

            for (var k = 0; k < startPos.Length; ++k)
            {
                startPos[k] = tweenObject.startPosition;
            }
            for (var k = 0; k < endPos.Length; ++k)
            {
                endPos[k] = tweenObject.endPosition;
            }
            for (var k = 0; k < startRot.Length; ++k)
            {
                startRot[k] = tweenObject.startRotation;
            }
            for (var k = 0; k < endRot.Length; ++k)
            {
                endRot[k] = tweenObject.endRotation;
            }

            MovementJob = new MovementJob()
            {
                startPosition = startPos,
                endPosition = endPos,
                startRotation = startRot,
                endRotation = endRot,
                space = tweenObject.space,
                _startPosition = tweenObject.startPosition,
                _endPosition = tweenObject.endPosition,
                _startRotation = tweenObject.startRotation,
                _endRotation = tweenObject.endRotation,
                moveSpeed = positionValue,
                rotationSpeed = rotationValue,
                loopTypeArr = loopTypeResult,
                deltaTime = Time.deltaTime,
            };

            jobHandle2 = MovementJob.Schedule(m_AccessArray, handle);
            jobHandle2.Complete();
            tweenObject.loopType = loopTypeResult[0];
            tweenObject.elapsedTime = resultElap[0];
            t = resultT[0];
            tweenObject.isPlaying = resultBool[0];

            if (tweenObject.loopType == LoopType.Incremental)
            {
                tweenObject.endPosition = endPos[0];
                tweenObject.endRotation = endRot[0];
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

        if (loopTypeArr[0] == LoopType.Incremental)
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
public struct AnimationCurveTCalculation : IJob
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
    public void Execute()
    {

        t = resultElap[0] / duration;
        if (loopTypeArr[0] == LoopType.PingPong)
        {
            t = Mathf.PingPong(t, 1f);
        }
        else if (loopTypeArr[0] == LoopType.Incremental)
        {
            t = (resultElap[0] % duration) / duration;
        }
        else
        {
            t = t % 1f;
        }

        if (resultElap[0] >= duration)
        {
            if (loop)
            {
                switch (loopTypeArr[0])
                {
                    case LoopType.Restart:
                        resultElap[0] = 0f;
                        break;
                    case LoopType.Incremental:
                        resultElap[0] = resultElap[0] % duration;
                        break;
                }
            }
            else
            {
                resultBool[0] = false;
            }
        }
        resultT[0] = t;
    }
}

