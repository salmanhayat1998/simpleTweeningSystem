using UnityEngine;

public enum LoopType
{
    Restart,
    PingPong,
    Incremental
}

public class TweeningExample : MonoBehaviour
{
    public AnimationCurve positionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public AnimationCurve rotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public bool autoPlay;
    public float duration = 1f;
    public float playDelay;
    public Vector3 endPosition;
    public Vector3 endRotation;
    public bool loop;
    public LoopType loopType;
    public Space space;
    private Vector3 startPosition;
    private Vector3 startRotation;
    private float elapsedTime;
    private bool isPlaying;

    private void Start()
    {
        startPosition = space == Space.World ? transform.position : transform.localPosition;
        startRotation = space == Space.World ? transform.eulerAngles : transform.localEulerAngles;

        if (autoPlay)
        {
            Invoke("Play", playDelay);
        }
    }

    float positionValue;
    float rotationValue;
    private void Update()
    {
        if (isPlaying)
        {
            UpdateElapsedTime();
            EvaluateCurves();
            ApplyTransformations();
            CheckLoopCompletion();
        }
    }

    private void UpdateElapsedTime()
    {
        elapsedTime += Time.deltaTime;
    }

    private void EvaluateCurves()
    {
        float t = elapsedTime / duration;
        if (loopType == LoopType.PingPong)
        {
            t = Mathf.PingPong(t, 1f);
        }
        else
        {
            t = t % 1f;
        }

        positionValue = positionCurve.Evaluate(t);
        rotationValue = rotationCurve.Evaluate(t);

        if (loopType == LoopType.Incremental)
        {
            t = (elapsedTime % duration) / duration;
        }
    }

    private void ApplyTransformations()
    {
        if (loopType == LoopType.Incremental)
        {
            startPosition = space == Space.World ? transform.position : transform.localPosition;
            endPosition = startPosition + endPosition;
            startRotation = space == Space.World ? transform.eulerAngles : transform.localEulerAngles;
            endRotation = startRotation + endRotation;
        }

        if (space == Space.World)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, positionValue);
            transform.eulerAngles = Vector3.Lerp(startRotation, endRotation, rotationValue);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(startPosition, endPosition, positionValue);
            transform.localEulerAngles = Vector3.Lerp(startRotation, endRotation, rotationValue);
        }
    }

    private void CheckLoopCompletion()
    {
        if (elapsedTime >= duration)
        {
            if (loop)
            {
                switch (loopType)
                {
                    case LoopType.Restart:
                        elapsedTime = 0f;
                        break;
                    case LoopType.Incremental:
                        elapsedTime = elapsedTime % duration;
                        break;
                }
            }
            else
            {
                isPlaying = false;
            }
        }
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
