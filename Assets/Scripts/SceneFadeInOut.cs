using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFadeInOut : MonoBehaviour
{
    public float fadeSpeed = 1.5f;
    public Image image;

    public delegate void FadeOutComplete();
    public event FadeOutComplete OnFadeOut;
    public delegate void FadeInComplete();
    public event FadeInComplete OnFadeIn;

    private bool isFadingIn = true;
    private bool isFadingOut = false;



    // --------------------------------------------------------------------------------------------------------
    //
    void Awake()
    {
        FadeIn();
    }

    // --------------------------------------------------------------------------------------------------------
    //
    void Update()
    {
        if (isFadingIn)
            UpdateFadeIn();
        else if (isFadingOut)
            UpdateFadeOut();
    }

    // --------------------------------------------------------------------------------------------------------
    //
    void FadeToClear()
    {
        image.color = Color.Lerp(image.color, Color.clear, fadeSpeed * Time.deltaTime);
    }

    // --------------------------------------------------------------------------------------------------------
    //
    void FadeToBlack()
    {
        image.color = Color.Lerp(image.color, Color.black, fadeSpeed * Time.deltaTime);
    }

    // --------------------------------------------------------------------------------------------------------
    //
    void UpdateFadeIn()
    {
        FadeToClear();
        if (image.color.a <= 0.05f)
        {
            image.color = Color.clear;
            image.enabled = false;
            isFadingIn = false;
            if (OnFadeIn != null)
                OnFadeIn();
        }
    }

    public void UpdateFadeOut()
    {
        FadeToBlack();
        if (image.color.a >= 0.95f)
        {
            if (OnFadeOut != null)
                OnFadeOut();
        }
    }

    // --------------------------------------------------------------------------------------------------------
    //
    public void FadeIn()
    {
        image.enabled = true;
        isFadingIn = true;
        isFadingOut = false;
    }

    // --------------------------------------------------------------------------------------------------------
    //
    public void FadeOut()
    {
        image.enabled = true;
        isFadingIn = false;
        isFadingOut = true;
    }
}