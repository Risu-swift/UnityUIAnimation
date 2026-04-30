using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class FrameEvent
{
    public enum TriggerMode { Frame, Time }

    [Tooltip("Whether the trigger fires at a specific frame index or after a specific time within the target sequence.")]
    public TriggerMode mode = TriggerMode.Frame;

    [Tooltip("Index of the sequence (in animationSequence) this event belongs to.")]
    public int sequenceIndex = 0;

    [Tooltip("Frame index within the target sequence at which to fire (used when mode = Frame).")]
    public int frame = 0;

    [Tooltip("Seconds elapsed since the target sequence started at which to fire (used when mode = Time).")]
    public float time = 0f;

    public UnityEvent onTrigger;

    [System.NonSerialized] public bool fired;
}

[RequireComponent(typeof(Image))]
public class ImageAnimation : MonoBehaviour
{
    public List<ImageAnimSequence> animationSequence;
    public bool isAutoPlay = false;
    public bool isLoop = false;

    public List<FrameEvent> frameEvents;

    private Image image;

    private float frameDuration;
    private int totalFrames;
    private bool isPlaying = false;
    private float playbackPercentage = 1f;
    private float elapsedTime;
    private float sequenceElapsedTime;
    private int currentFrameIndex;
    private int sequenceIndex = 0;
    private int targetFrameIndex;
    private bool hasStarted = false;

    public UnityEvent OnAnimationEnd;

    // Start is called before the first frame update
    void OnEnable()
    {
        image = gameObject.GetComponent<Image>();

        if (animationSequence != null && animationSequence.Count > 0)

        {

            if (isAutoPlay)
            {
                Play(1f);
            }
            else
            {
                image.sprite = animationSequence[0].AnimationSequence[0];
            }
        }

    }

    void OnDisable()
    {
        ResetAnimation();
    }

    public void Play() => Play(1f);

    public void PlayPercent(int percentage) => Play((float)percentage / 100f);

    // Update is called once per frame
    void Update()
    {
        if (isPlaying)
        {
            elapsedTime += Time.deltaTime;
            sequenceElapsedTime += Time.deltaTime;

            if (elapsedTime >= frameDuration)
            {
                elapsedTime = 0f;

                if (hasStarted)
                {
                    currentFrameIndex++;

                    if (currentFrameIndex > targetFrameIndex)
                    {
                        if (sequenceIndex < animationSequence.Count - 1)
                        {
                            sequenceIndex++;
                            StartNewSequence();
                        }

                        else
                        {
                            if (isLoop)
                            {
                                currentFrameIndex = 0;
                                elapsedTime = 0f;
                                sequenceElapsedTime = 0f;
                                ResetFrameEvents();
                            }
                            else
                            {
                                isPlaying = false;
                            }
                            OnAnimationEnd.Invoke();

                        }
                    }
                }
            }
            else
            {
                hasStarted = true;
            }

            if (currentFrameIndex >= 0 && currentFrameIndex < totalFrames)
            {
                image.sprite = animationSequence[sequenceIndex].AnimationSequence[currentFrameIndex];
            }

            CheckFrameEvents();
        }
    }

    private void CheckFrameEvents()
    {
        if (frameEvents == null) return;

        for (int i = 0; i < frameEvents.Count; i++)
        {
            var evt = frameEvents[i];
            if (evt == null || evt.fired || evt.sequenceIndex != sequenceIndex) continue;

            bool trigger = evt.mode == FrameEvent.TriggerMode.Frame
                ? currentFrameIndex >= evt.frame
                : sequenceElapsedTime >= evt.time;

            if (trigger)
            {
                evt.fired = true;
                evt.onTrigger?.Invoke();
            }
        }
    }

    private void ResetFrameEvents()
    {
        if (frameEvents == null) return;
        for (int i = 0; i < frameEvents.Count; i++)
        {
            if (frameEvents[i] != null) frameEvents[i].fired = false;
        }
    }

    public void Play(float percentage)
    {
        if (animationSequence == null || animationSequence.Count == 0 || percentage <= 0f) return;

        elapsedTime = 0f;
        sequenceElapsedTime = 0f;
        hasStarted = false;
        currentFrameIndex = 0;
        sequenceIndex = 0;
        isPlaying = true;
        ResetFrameEvents();

        playbackPercentage = Mathf.Clamp01(percentage);

        var currentSequence = animationSequence[sequenceIndex];
        frameDuration = 1f / currentSequence.TargetFPS;
        totalFrames = currentSequence.AnimationSequence.Count;
        targetFrameIndex = Mathf.FloorToInt(totalFrames * playbackPercentage) - 1;

        if (targetFrameIndex >= totalFrames) targetFrameIndex = totalFrames - 1;

        image.sprite = currentSequence.AnimationSequence[currentFrameIndex];
    }

    private void StartNewSequence()
    {
        var currentSequence = animationSequence[sequenceIndex];
        frameDuration = 1f / currentSequence.TargetFPS;
        totalFrames = currentSequence.AnimationSequence.Count;
        currentFrameIndex = 0;
        sequenceElapsedTime = 0f;
        image.sprite = currentSequence.AnimationSequence[currentFrameIndex];
    }

    private void ResetAnimation()
    {
        isPlaying = false;
        hasStarted = false;
        elapsedTime = 0f;
        sequenceElapsedTime = 0f;
        playbackPercentage = 0f;
        currentFrameIndex = 0;
        targetFrameIndex = 0;
        sequenceIndex = 0;
        ResetFrameEvents();

        if (animationSequence != null && animationSequence.Count > 0)
        {
            var initialSequence = animationSequence[0];
            image.sprite = initialSequence.AnimationSequence[0];
            frameDuration = 1f / initialSequence.TargetFPS;
            totalFrames = initialSequence.AnimationSequence.Count;
        }
    }
}
