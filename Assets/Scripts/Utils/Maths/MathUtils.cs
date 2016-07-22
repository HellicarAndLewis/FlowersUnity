using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MathUtils
{

	// ------------------------------------------------------------
	public static float DistanceToRay( Vector3 _point, Ray _ray  )  {
		Vector3 X2 = _ray.origin + _ray.direction;
		Vector3 X0X1 = (_point - _ray.origin);
		Vector3 X0X2 = (_point - X2);

		return ( Vector3.Cross(X0X1, X0X2).magnitude / (_ray.origin - X2).magnitude );
	}

	// ------------------------------------------------------------
	public static Vector2 ClosestPointOnSegment2D(Vector2 A,  Vector2 B, Vector2 P) {
		Vector2 D = B - A;
		float numer = Vector2.Dot(P - A, D);
		if (numer <= 0.0f)
			return A;
		float denom = Vector2.Dot(D, D);
		if (numer >= denom)
			return B;
		return A + (numer / denom) * D;
	}

	// ------------------------------------------------------------
	// Returns 0 if if point lies on the line, or negative or positive depending on which side
	public static float PointOnWhichSideOfLine( Vector2 _lineStart, Vector2 _lineEnd, Vector2 _point ) {

		Vector2 diff = _lineEnd - _lineStart;
		Vector2 perp =  new Vector2(-diff.y, diff.x);
		float d = Vector2.Dot(_point - _lineStart, perp);
		return d;
	}


	/// Todo: Remove
	public static float MapRange(float _val, float _inStart, float _inEnd, float _outStart, float _outEnd ) {
		return _outStart + (_val - _inStart) * (_outEnd - _outStart) / (_inEnd - _inStart);
	}

	//--------------------------------------------------
	public static float Map(float value, float inputMin, float inputMax, float outputMin, float outputMax, bool clamp = false )
	{
		if (Mathf.Abs(inputMin - inputMax) < Mathf.Epsilon)
		{
			return outputMin;
		}
		else
		{
			float outVal = ((value - inputMin) / (inputMax - inputMin) * (outputMax - outputMin) + outputMin);

			if (clamp)
			{
				if (outputMax < outputMin)
				{
					if (outVal < outputMax) outVal = outputMax;
					else if (outVal > outputMin) outVal = outputMin;
				}
				else
				{
					if (outVal > outputMax) outVal = outputMax;
					else if (outVal < outputMin) outVal = outputMin;
				}
			}
			return outVal;
		}
	}

	// ------------------------------------------------------------
	// Step functions
	// ------------------------------------------------------------

	// ------------------------------------------------------------
	static float step(float a, float x)
	{
		return (x >= a) ? 1.0f : 0.0f;
	}

	// ------------------------------------------------------------
	public static float LinearStep( float _edge0, float _edge1, float _t )
	{
		// Scale, and clamp x to 0..1 range
		return Mathf.Clamp( (_t - _edge0)/(_edge1 - _edge0), 0.0f, 1.0f);
	}

	// ------------------------------------------------------------
	public static float LinearStepOut( float _high1, float _low1, float _t ) 
	{ 
		return (1.0f - LinearStep( _high1, _low1, _t )); 
	}

	// ------------------------------------------------------------
	public static float LinearStepInOut( float _low0, float _high0, float _high1, float _low1, float _t )
	{
		return LinearStep( _low0, _high0, _t ) * (1.0f - LinearStep( _high1, _low1, _t ));
	}

	// ------------------------------------------------------------
	public static float CircularStep( float _edge0, float _edge1, float _t  )
	{
		// Scale, and clamp x to 0..1 range
		float t = Mathf.Clamp( (_t - _edge0)/(_edge1 - _edge0), 0.0f, 1.0f);
		t = t - 1f;
		return Mathf.Sqrt( 1 - t * t );
	}


	// ------------------------------------------------------------
	public static float CircularStepOut( float _edge0, float _edge1, float _t  )
	{
		// Scale, and clamp x to 0..1 range
		float t = Mathf.Clamp( (_t - _edge0)/(_edge1 - _edge0), 0.0f, 1.0f);
		return -1f * (Mathf.Sqrt( 1 - t * t ) - 1f);
	}

	// ------------------------------------------------------------
	public static float CircularStepInOut( float _low0, float _high0, float _high1, float _low1, float _t )
	{
		return CircularStep( _low0, _high0, _t ) * (1.0f - CircularStepOut( _high1, _low1, _t ));
	}

/*
		t = 0 (we’re just starting, so 0 seconds have passed)
		b = 50 (the beginning value of the property being tweened)
		c = 150 (the change in value – so the destination value of 200 minus the start value of 50 equals 150)
		d = 1 (total duration of 1 second) 

		Out:
 		return  c *   Mathf.Sqrt( 1 - ( t = t / d - 1 ) * t ) + b; 

		In:
		return -c * ( Mathf.Sqrt( 1 - ( t /= d )        * t ) - 1 ) + b;

 		return  1 *   Mathf.Sqrt( 1 - ( t = t / 1 - 1 ) * t );
		return -1 * ( Mathf.Sqrt( 1 - ( t * t ) - 1 );
 */

	// ------------------------------------------------------------
	public static float Smoothstep(float edge0, float edge1, float x) {
		// Scale, and clamp x to 0..1 range
		x = Mathf.Clamp( (x - edge0) / (edge1 - edge0), 0, 1);
		// Evaluate polynomial
		return x * x * x * (x * (x * 6 - 15) + 10);
	}

	/// <summary>
	/// Smoothstep in and out, for instance a bell curve would just be:
	/// (_t, 0.0, 0.5, 0.5, 1.0)
	/// To delay the start a little bit:
	/// (_t, 0.1, 0.5, 0.5, 1.0)
	/// To stay on full most of the time with a short in and out:
	/// (_t, 0.0, 0.05, 0.95, 1.0)
	/// Can also cross the middle point:
	/// (_t, 0.0, 0.05, 0.35, 1.0)
	/// </summary>
	public static float SmoothStepInOut( float _low0, float _high0, float _high1, float _low1, float _t ) {
		return Smoothstep( _low0, _high0, _t ) * (1.0f - Smoothstep( _high1, _low1, _t ));;
	}

	// ------------------------------------------------------------
	public static float SmoothStepInOut( StepFunctionParams _params, float _t ) {
		if( _params == null ) return 0f;
		return SmoothStepInOut( _params.low0, _params.high0, _params.high1, _params.low1, _t );
	}

	//-----------------------------------------
	// From http://www.flong.com/texts/code/shapers_exp/
	public static float exponentialEasing(float x, float a)
	{
		float epsilon = 0.00001f;
		float min_param_a = 0.0f + epsilon;
		float max_param_a = 1.0f - epsilon;
		a = Mathf.Max(min_param_a, Mathf.Min(max_param_a, a));

		if (a < 0.5)
		{
			// emphasis
			a = 2.0f * (a);
			float y = Mathf.Pow(x, a);
			return y;
		}
		else
		{
			// de-emphasis
			a = 2.0f * (a - 0.5f);
			float y = Mathf.Pow(x, 1f / (1f - a));
			return y;
		}
	}

	//--------------------------------------------------
	public static float AngleDifferenceDegrees(float currentAngle, float targetAngle) {
		return WrapDegrees(targetAngle - currentAngle);
	}

	//--------------------------------------------------
	public static float AngleDifferenceRadians(float currentAngle, float targetAngle) {
		return WrapRadians(targetAngle - currentAngle);
	}

	//--------------------------------------------------
	public static float WrapRadians(float _angle, float _from = -Mathf.PI, float _to  = Mathf.PI ) {
		while (_angle > _to )   _angle -= Mathf.PI * 2.0f;
		while (_angle < _from ) _angle += Mathf.PI * 2.0f;
		return _angle;
	}

	//--------------------------------------------------
	public static float WrapDegrees(float _angle, float _from = -180.0f, float _to = 180.0f ) {
		while (_angle > _to )   _angle -= 360;
		while (_angle < _from ) _angle += 360;
		return _angle;
	}

	// ------------------------------------------------------------
	// Shaping functions
	// ------------------------------------------------------------

	// ------------------------------------------------------------
	public static float pulseSquare( float _frequency, float _width, float _t )
	{
		return 1 - step( _width, ( _t % _frequency ) );
	}

	// ------------------------------------------------------------
	public static float pulseTriangle( float _frequency, float _width, float _t )
	{
		float triangleT = ( _t % _frequency ) / _width * 2.0f;
		return (1.0f - Mathf.Abs((triangleT % 2.0f) - 1.0f)) * pulseSquare( _frequency, _width, _t );
	}

	// ------------------------------------------------------------
	public static float pulseLineDownUp( float _frequency, float _width, float _t )
	{
		float tmpVal = ( _t % _frequency ) / _width;
		return tmpVal * (1 - step( 1.0f, tmpVal ));
	}

	// ------------------------------------------------------------
	public static float pulseLineUpDown( float _frequency, float _width, float _t )
	{
		float tmpVal = 1 - (( _t % _frequency ) / _width);
		return Mathf.Clamp( tmpVal * (1 - step( 1.0f, tmpVal )), 0, 1);
	}

	// ------------------------------------------------------------
	public static float pulseSawTooth( float _frequency, float _width, float _t )
	{
		float tmpVal = 1 - (( _t % _frequency ) / _width);
		return Mathf.Clamp( tmpVal * (1 - step( 1.0f, tmpVal )), 0, 1);
	}

	// ------------------------------------------------------------
	public static float pulseSine( float _frequency, float _width, float _t )
	{
		float tmpVal = Mathf.Clamp( ( ( _t % _frequency ) / _width), 0, 1);
		return Mathf.Sin(tmpVal * Mathf.PI);
	}

	// -----------------------------------------------------------
	public static float pulseSmoothStep( float _frequency, float _x0, float _x1, float _x2, float _x3, float _t )
	{
		float tmpT = ( _t % _frequency );
		return SmoothStepInOut( _x0, _x1, _x2, _x3, tmpT );
	}

}



// ------------------------------------------------------------------------------------------------------------------------------------------------
//
public class StepFunctionParams 
{
	// --------------------------------------------------------------------------------------------------------
	public StepFunctionParams()
	{
		low0  = 0f;
		high0 = 0f;
		high1 = 0f;
		low1  = 0f;
	}

	// --------------------------------------------------------------------------------------------------------
	public StepFunctionParams( float _low0, float _high0, float _high1, float _low1 )
	{
		set( _low0, _high0, _high1, _low1 );
	}

	// --------------------------------------------------------------------------------------------------------
	public void set( float _low0, float _high0, float _high1, float _low1 )
	{
		low0  = _low0;
		high0 = _high0;
		high1 = _high1;
		low1  = _low1;
	}

	// --------------------------------------------------------------------------------------------------------
	public bool isValid()
	{
		return (Mathf.Abs(low0) + Mathf.Abs(high0) + Mathf.Abs(high1) + Mathf.Abs(low1) ) > 0f;
	}

	public float low0;
	public float high0;
	public float high1;
	public float low1;
}
