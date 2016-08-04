using UnityEngine;
using System.Collections;

public class AnimatedController : MonoBehaviour
{
    public bool isPlaying = false;
    [Range(0, 1)]
    public float normalisedTime = 0;
    public float duration = 30;

    private bool loop = false;
    protected Animator animator;

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
            Debug.LogWarning("AnimatedController needs to have an Animator compontent");
        }
    }
    
    // --------------------------------------------------------------------------------------------------------
    //
    virtual protected void Update()
    {
        if (isPlaying)
        {
            animator.speed = 1;
            UpdateAnimation(CaptureTime.Delta);
        }
        else
        {
            PlayNormalised(normalisedTime);
            animator.speed = 0;
        }
    }

    // --------------------------------------------------------------------------------------------------------
    //
    public void UpdateAnimation(float deltaTime)
    {
        if (animator)
        {
            var currentState = animator.GetCurrentAnimatorStateInfo(0);
            var length = duration;
            normalisedTime = currentState.normalizedTime + (deltaTime / length);

            if (normalisedTime < 0.99f)
            {
                PlayNormalised(normalisedTime);
            }
            else
            {
                Debug.Log("AnimatedController.Complete");
                Pause();
            }
        }
    }

    public void Play()
    {
        isPlaying = true;
        animator.speed = 1;
    }

    public void Pause()
    {
        isPlaying = false;
        animator.speed = 0;
    }

    // --------------------------------------------------------------------------------------------------------
    //
    public void PlayNormalised(float normalisedTime)
    {
        if (animator)
        {
            this.normalisedTime = normalisedTime;
            var currentState = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play(currentState.fullPathHash, 0, normalisedTime);
        }
    }

    // --------------------------------------------------------------------------------------------------------
    //
    public void StartAnimation(bool reset = false)
    {
        //Debug.Log("AnimatedPath.StartAnimation");
        if (animator)
        {
            isPlaying = true;
            reset = false;
            animator.speed = 1;
            if (reset)
            {
                var animationHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
                animator.Play(animationHash, 0, 0);
            }
            
        }
    }
}
