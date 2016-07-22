using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugDrawUtils
{

	// --------------------------------------------------------------------------------------------------------
	//
	public static void drawAxisGizmo(float _size, Transform _transform)
	{
		drawAxisGizmo(_size, Matrix4x4.TRS(_transform.position, _transform.rotation, Vector3.one));
	}

	// --------------------------------------------------------------------------------------------------------
	//
	public static void drawAxisGizmo(float _size, Matrix4x4 _transform)
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(_transform.MultiplyPoint(Vector3.zero), _transform.MultiplyPoint(new Vector3(_size, 0f, 0f)));

		Gizmos.color = Color.green;
		Gizmos.DrawLine(_transform.MultiplyPoint(Vector3.zero), _transform.MultiplyPoint(new Vector3(0f, _size, 0f)));

		Gizmos.color = Color.blue;
		Gizmos.DrawLine(_transform.MultiplyPoint(Vector3.zero), _transform.MultiplyPoint(new Vector3(0f, 0f, _size)));
	}

	// --------------------------------------------------------------------------------------------------------
	//
	public static void debugDrawAxis(float _size, Transform _transform )
	{
		debugDrawAxis(_size, Matrix4x4.TRS(_transform.position, _transform.rotation, Vector3.one));
	}

	// --------------------------------------------------------------------------------------------------------
	//
	public static void debugDrawAxis(float _size, List<Transform> _transformList )
	{
		for( int i = 0; i < _transformList.Count; i++ )
		{
			debugDrawAxis( _size, _transformList[i] );
		}
	}

	// --------------------------------------------------------------------------------------------------------
	//
	public static void debugDrawAxis(float _size, Transform[] _transformList )
	{
		for( int i = 0; i < _transformList.Length; i++ )
		{
			debugDrawAxis( _size, _transformList[i] );
		}
	}

	// --------------------------------------------------------------------------------------------------------
	//
	public static void debugDrawAxis(float _size, Matrix4x4 _transform)
	{
		Debug.DrawLine(_transform.MultiplyPoint(Vector3.zero), _transform.MultiplyPoint(new Vector3(_size, 0f, 0f)), Color.red);
		Debug.DrawLine(_transform.MultiplyPoint(Vector3.zero), _transform.MultiplyPoint(new Vector3(0f, _size, 0f)), Color.green);
		Debug.DrawLine(_transform.MultiplyPoint(Vector3.zero), _transform.MultiplyPoint(new Vector3(0f, 0f, _size)), Color.blue);
	}

	// --------------------------------------------------------------------------------------------------------
	//
	public static void debugDrawAxis(float _size, List<Matrix4x4> _transformList )
	{
		
		for( int i = 0; i < _transformList.Count; i++ )
		{
			debugDrawAxis( _size, _transformList[i] );
		}
	}

	// --------------------------------------------------------------------------------------------------------
	//
	public static void debugDrawAxis(float _size, Matrix4x4[] _transformList )
	{
		for( int i = 0; i < _transformList.Length; i++ )
		{
			debugDrawAxis( _size, _transformList[i] );
		}
	}

	// --------------------------------------------------------------------------------------------------------
	//
	public static void debugDrawLines(List<Vector3> _points, Color _color )
	{
		for (int i = 0; i < _points.Count - 1; i++)
		{
			Debug.DrawLine( _points[i], _points[i+1], _color );
		}
	}

}
