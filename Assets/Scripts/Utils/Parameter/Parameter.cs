using UnityEngine;
using System.Collections;


public class Parameter
{
	public string name;
	public float min;
	public float max;
	public float value;
}

public class EasedParameter : Parameter
{
	public System.Func<float, float, float, float, float> easing;
	public Color debugDrawColour;

	
	/// <summary>
	/// Gets output in the min - max range for the specified input
	/// </summary>
	// --------------------------------------------------------------------------------------------------------
	//
	public float GetOutput(float input)
	{
		// map the normalised output to our min/max range
		var output = MathUtils.Map(GetNormalisedOutput(input), 0, 1, min, max);
		return output;
	}

	/// <summary>
	/// Gets normalised output for the specified input
	/// </summary>
	// --------------------------------------------------------------------------------------------------------
	//
	public float GetNormalisedOutput(float input)
	{
		// Working with easing euqations with normalised time/length as input
		// input     : normalised input
		// min/max   : normalised output range
		// input max : always 1 as we're working with a normalised input
		var output = easing(input, 0, 1, 1.0f);
		return output;
	}

	// --------------------------------------------------------------------------------------------------------
	//
	public override string ToString()
	{
		return string.Format("EasedParameter:{0} {1} - {2}", name, min, max);
	}

}