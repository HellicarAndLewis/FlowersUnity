using UnityEngine;
using System.Collections;

public class AnimatedController : MonoBehaviour
{
    
    public bool autoPlay = true;
    public bool isPlaying = false;
    public bool reset = false;
    public bool loop = false;
    [Range(0, 1)]
    public float normalisedTime = 0;
    public float duration = 30;

    private Animator animator;

    // --------------------------------------------------------------------------------------------------------
    //
    protected virtual void Start()
    {
        if (GetComponent<Animator>())
        {
            animator = GetComponent<Animator>();
            animator.speed = 0;
        }
        else
        {
            Debug.LogError("AnimatedController needs to have an Animator compontent");
        }
    }
    
    // --------------------------------------------------------------------------------------------------------
    //
    virtual protected void Update()
    {
        if (autoPlay) UpdateAnimation(CaptureTime.Delta);
    }

    // --------------------------------------------------------------------------------------------------------
    //
    public void UpdateAnimation(float deltaTime)
    {

        if (!isPlaying || reset)
            StartAnimation();

        if (animator)
        {
            var currentState = animator.GetCurrentAnimatorStateInfo(0);
            var length = duration;
            normalisedTime = currentState.normalizedTime + (deltaTime / length);

            if (normalisedTime < 1.0f)
            {
                PlayNormalised(normalisedTime);
            }
            else
            {
                Debug.Log("AnimatedController.Complete");
                if (loop)
                    StartAnimation();
                else
                {
                    autoPlay = false;
                    isPlaying = false;
                    animator.speed = 0;
                }
            }
        }
    }

    // --------------------------------------------------------------------------------------------------------
    //
    public void PlayNormalised(float normalisedTime)
    {
        this.normalisedTime = normalisedTime;
        var currentState = animator.GetCurrentAnimatorStateInfo(0);
        animator.Play(currentState.fullPathHash, 0, normalisedTime);
    }

    // --------------------------------------------------------------------------------------------------------
    //
    public void StartAnimation()
    {
        //Debug.Log("AnimatedPath.StartAnimation");
        if (animator)
        {
            animator.speed = 1;
            var animationHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
            animator.Play(animationHash, 0, 0);
            isPlaying = true;
            reset = false;
        }
    }
}
