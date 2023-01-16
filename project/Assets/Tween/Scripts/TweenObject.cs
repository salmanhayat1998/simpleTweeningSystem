using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenObject:MonoBehaviour
{

    public bool autoPlay;
    public float duration = 1f;
    public float playDelay;
    public Vector3 endPosition;
    public Vector3 endRotation;
    public bool loop;
    public LoopType loopType;
    public Space space;
    [HideInInspector] public Vector3 startPosition;
    [HideInInspector] public Vector3 startRotation;
      public float elapsedTime;
    [HideInInspector] public bool isPlaying;

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
