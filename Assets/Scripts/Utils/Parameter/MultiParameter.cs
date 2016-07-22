using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Takes a normalised input and maps it to multiple outputs based on the specified curve/easing equations
 * Allows for different relationships between outputs e.g. inversely proportional
 * Allows for different curves/line types e.g. exponential, sin, linear
 * Uses easing equations in Easing.cs
 */
public class MultiParameter
{
	public float input;
	public List<EasedParameter> parameters = new List<EasedParameter>();



	/// <summary>
	/// Adds an output paramter with a specified range and a curve/easing type as a static function reference from the options in Easing.cs
	/// e.g. param.AddParameter(0, 10, Easing.ExpoEaseIn);
	/// e.g. param.AddParameter(4, 36, Easing.Linear);
	/// </summary>
	// --------------------------------------------------------------------------------------------------------
	//
	public void AddParameter(string name, float min, float max, System.Func<float, float, float, float, float> easing)
	{
		var parameter = new EasedParameter
		{
			name = name,
			min = min,
			max = max,
			easing = easing
		};
		parameter.debugDrawColour = Color.blue;
		if (parameters.Count > 0 ) parameter.debugDrawColour = Color.red;
		if (parameters.Count > 1 ) parameter.debugDrawColour = Color.green;
		parameters.Add(parameter);
	}

	// --------------------------------------------------------------------------------------------------------
	//
	public void AddParameter(float min, float max, System.Func<float, float, float, float, float> easing)
	{
		AddParameter("", min, max, easing);
	}


	// --------------------------------------------------------------------------------------------------------
	//
	public List<float> GetOutputs(float input)
	{
		this.input = input;
		List<float> outputs = new List<float>();
		foreach (EasedParameter parameter in parameters)
		{
			outputs.Add( parameter.GetOutput(input) );
		}
		return outputs;
	}


	// --------------------------------------------------------------------------------------------------------
	//
	public override string ToString()
	{
		string s = string.Format("MultiParameter with {0} outputs: ", parameters.Count);
		foreach (EasedParameter parameter in parameters)
		{
			s += string.Format("[ {0} ], ", parameter.ToString());
		}
		return s;
	}


	// --------------------------------------------------------------------------------------------------------
	//
	public void Draw ()
	{
		foreach (EasedParameter parameter in parameters)
		{
			List<Vector3> points = new List<Vector3>();
			int size = 100;
			for (int x = 0; x < size; x++)
			{
				var y = parameter.GetOutput( x / (float)size );
				points.Add( new Vector3(x, y, 0) );
			}
			debugDraw(points, parameter.debugDrawColour );
		}
	}

	// --------------------------------------------------------------------------------------------------------
	//
	void debugDraw(List<Vector3> _points, Color _col )
	{
		debugDraw(_points, _col, Vector3.zero );
	}

	// --------------------------------------------------------------------------------------------------------
	//
	void debugDraw(List<Vector3> _points, Color _col, Vector3 _offset )
	{
		if (_points.Count > 1)
		{
			for (int i = 0; i < _points.Count - 1; i++)
			{
				Debug.DrawLine(_points[i] + _offset, _points[i + 1] + _offset, _col);
			}
		}
	}
}
