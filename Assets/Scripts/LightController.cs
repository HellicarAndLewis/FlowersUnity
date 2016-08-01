using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightPreset
{
	public Color colour;
	public Vector3 eulerAngle;
}

/// <summary>
/// Simple controller for lerping between light presets
/// </summary>
public class LightController : AnimatedController
{
	// --------------------------------------------------------------------------------------------------------
	//
	public Light sunLight;
	public LightPreset[] presets = new LightPreset[4];
	public float transitionTime = 8.0f;

	public float progress = 1;
	public float transitionElapsed = 0;
	private LightPreset current = new LightPreset();
	private LightPreset previous;
	private LightPreset target;

    public onsetDetector onset;

	// --------------------------------------------------------------------------------------------------------
	//
	override protected void Start()
	{
        base.Start();

		if(!sunLight)
			sunLight = FindObjectOfType<Light>();

        if (!onset)
            onset = FindObjectOfType<onsetDetector>();

		// Dawn
		if(presets[0] == null)
			presets[0] = new LightPreset(){colour= Color.cyan, eulerAngle = new Vector3(20, 80, 0)};
        // Daylight, FFCE43FF
        if (presets[1] == null)
			presets[1] = new LightPreset(){colour= new Color(1, 0.808f, 0.263f), eulerAngle= new Vector3(56, 26, 0)};
		// Dusk
		if(presets[2] == null)
			presets[2] = new LightPreset(){colour= Color.red, eulerAngle= new Vector3(170, 80, 0)};
        // Night
        if (presets[3] == null)
            presets[3] = new LightPreset() { colour = Color.black, eulerAngle = new Vector3(0, 80, 0) };

        previous = presets[0];
		target = presets[0];
		Preset(TerrainMode.Daytime);
	}

    // --------------------------------------------------------------------------------------------------------
    //
    override protected void Update()
	{
        base.Update();

        /*
        if (sunLight) {
			if(transitionElapsed < transitionTime) {
				transitionElapsed += Time.deltaTime;
				progress = transitionElapsed / transitionTime;
			} else {
				progress = 1;
			}

            if (onset) sunLight.intensity = onset.onsetTotal * 8.0f;
			
			current.colour = Color.Lerp(previous.colour, target.colour, progress);
			current.eulerAngle = Vector3.Lerp(previous.eulerAngle, target.eulerAngle, progress);

			sunLight.color = current.colour;
			var rotation = sunLight.transform.localRotation;
			rotation.eulerAngles = current.eulerAngle;
			sunLight.transform.localRotation = rotation;
		}
        */
	}

	// --------------------------------------------------------------------------------------------------------
	//
	public void Preset(TerrainMode mode, float duration = -1)
	{
		var index = (int)mode;
		previous = new LightPreset(){colour = current.colour, eulerAngle = current.eulerAngle };
		target = presets[index];
		transitionElapsed = 0;
		if(duration > -1)
			transitionTime = duration;
	}
}
