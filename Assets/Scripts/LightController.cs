using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightPreset
{
	public Color colour;
	public Vector3 eulerAngle;
	//void LightPreset(Color colour, Vector3 eulerAngle)
}

public class LightController : MonoBehaviour
{
	// --------------------------------------------------------------------------------------------------------
	//
	public Light sunLight;
	public LightPreset[] presets = new LightPreset[4];

	private LightPreset current;
	private LightPreset previous;
	private LightPreset target;

	// --------------------------------------------------------------------------------------------------------
	//
	void Start()
	{
		if(!sunLight)
			sunLight = FindObjectOfType<Light>();

		// Dawn
		if(presets[0] == null)
			presets[0] = new LightPreset(){colour= Color.cyan, eulerAngle = new Vector3(27, 80, 0)};
		// Daylight
		if(presets[1] == null)
			presets[1] = new LightPreset(){colour= Color.yellow, eulerAngle= new Vector3(27, 80, 0)};
		// Dusk
		if(presets[2] == null)
			presets[2] = new LightPreset(){colour= Color.red, eulerAngle= new Vector3(27, 80, 0)};

		current = presets[0];
		Preset(ShowMode.Daytime);
	}

	// --------------------------------------------------------------------------------------------------------
	//
	void Update()
	{
		if(Input.GetKeyDown("1")) Preset(ShowMode.Dawn);
		if(Input.GetKeyDown("2")) Preset(ShowMode.Daytime);
		if(Input.GetKeyDown("3")) Preset(ShowMode.Dusk);
			
		if(sunLight) {
			sunLight.color = current.colour;
			var rotation = sunLight.transform.localRotation;
			rotation.eulerAngles = current.eulerAngle;
			sunLight.transform.localRotation = rotation;
		}
	}

	// --------------------------------------------------------------------------------------------------------
	//
	public void Preset(ShowMode mode, float duration = 0)
	{
		var index = (int)mode;
		previous = current;
		target = presets[index];
		current = presets[index];
	}
}
