using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
public class TweeningJob : MonoBehaviour
{
    public AnimationCurve positionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public AnimationCurve rotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public bool autoPlay;
    public float duration = 1f;
    public float playDelay;
    public Vector3 endPosition;
    public Quaternion endRotation;
    public bool loop;
    public LoopType loopType;
    public Space space;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float elapsedTime;
    private bool isPlaying;

    private void Start()
    {
        startPosition = space == Space.World ? transform.position : transform.localPosition;
        startRotation = space == Space.World ? transform.rotation : transform.localRotation;
        m_Transforms[0] = transform;
        m_AccessArray = new TransformAccessArray(m_Transforms);

        if (autoPlay)
        {
            Invoke("Play", playDelay);
        }
    }

    float positionValue;
    float rotationValue;

    private JobHandle MovementJobHandle;
    Transform[] m_Transforms = new Transform[1];
    TransformAccessArray m_AccessArray;
    float t;

    private void Update()
    {

        if (isPlaying)
        {

            //////// Animation Curve Evaluation ///////////

            NativeArray<float> resultT = new NativeArray<float>(1, Allocator.TempJob);
            NativeArray<float> resultElap = new NativeArray<float>(1, Allocator.TempJob);
            NativeArray<bool> resultBool = new NativeArray<bool>(1, Allocator.TempJob);
            NativeArray<LoopType> loopTypeResult = new NativeArray<LoopType>(m_Transforms.Length, Allocator.Persistent);

            AnimationCurveTCalculation animationCurveT = new AnimationCurveTCalculation();
            animationCurveT.elapsedTime = elapsedTime;
            animationCurveT.duration = duration;
            animationCurveT.deltaTime = Time.deltaTime;
            animationCurveT.loop = loop;
            animationCurveT.loopType = loopType;

            animationCurveT.loopTypeArr = loopTypeResult;
            animationCurveT.t = t;
            animationCurveT.resultT = resultT;
            animationCurveT.resultElap = resultElap;
            animationCurveT.resultBool = resultBool;


            JobHandle handle = animationCurveT.Schedule();
            handle.Complete();
            loopType = loopTypeResult[0];
            elapsedTime = resultElap[0];
            t = resultT[0];
            isPlaying = resultBool[0];
            positionValue = positionCurve.Evaluate(t);
            rotationValue = rotationCurve.Evaluate(t);
            resultT.Dispose();
            resultElap.Dispose();
            resultBool.Dispose();


            ///////////////////// /////////////////
            ///
            /////// Actual job for Moving Transform ////////////

            NativeArray<Vector3> startPos = new NativeArray<Vector3>(m_Transforms.Length, Allocator.Persistent);
            NativeArray<Vector3> endPos = new NativeArray<Vector3>(m_Transforms.Length, Allocator.Persistent);
            NativeArray<Quaternion> startRot = new NativeArray<Quaternion>(m_Transforms.Length, Allocator.Persistent);
            NativeArray<Quaternion> endRot = new NativeArray<Quaternion>(m_Transforms.Length, Allocator.Persistent);
         


            MovementJobHandle = new MovementJob()
            {
                startPosition = startPos,
                endPosition = endPos,
                startRotation = startRot,
                endRotation = endRot,
                space = space,
                _startPosition = startPosition,
                _endPosition = endPosition,
                _startRotation = startRotation,
                _endRotation = endRotation,
                moveSpeed = positionValue,
                rotationSpeed = rotationValue,
                loopType=loopType,
                loopTypeArr= loopTypeResult,
                deltaTime = Time.deltaTime,
            }.Schedule(m_AccessArray);

            MovementJobHandle.Complete();

            if (loopType == LoopType.Incremental)
            {
                endPosition = endPos[0];
                endRotation = endRot[0];
            }
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
    public void Play()
    {
        isPlaying = true;
        elapsedTime = 0f;
    }

    public void Stop()
    {
        isPlaying = false;
    }
}
[BurstCompile]
public struct MovementJob : IJobParallelForTransform
{
    public NativeArray<Vector3> startPosition;
    public NativeArray<Vector3> endPosition;
    public NativeArray<Quaternion> startRotation;
    public NativeArray<Quaternion> endRotation;
    public NativeArray<LoopType> loopTypeArr;
    public LoopType loopType;
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
        for (var i = 0; i < startPosition.Length; ++i)
        {
            startPosition[i] = _startPosition;
        }
        for (var i = 0; i < endPosition.Length; ++i)
        {
            endPosition[i] = _endPosition;
        }
        for (var i = 0; i < startRotation.Length; ++i)
        {
            startRotation[i] = _startRotation;
        }
        for (var i = 0; i < endRotation.Length; ++i)
        {
            endRotation[i] = _endRotation;
        }
        loopTypeArr[0] = loopType;
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
        //  transform.position = Vector3.Lerp(startPosition[index], endPosition[index], speed);
    }
}

[BurstCompile]
public struct AnimationCurveTCalculation : IJob
{
    public float duration;
    public float deltaTime;
    public float elapsedTime;
    public float t;
    public bool loop;
    public NativeArray<LoopType> loopTypeArr;
    public LoopType loopType;
    public NativeArray<float> resultT;
    public NativeArray<float> resultElap;
    public NativeArray<bool> resultBool;
    public void Execute()
    {
        resultBool[0] = true;
        loopTypeArr[0] = loopType;
        elapsedTime += deltaTime;
        resultElap[0] = elapsedTime;
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