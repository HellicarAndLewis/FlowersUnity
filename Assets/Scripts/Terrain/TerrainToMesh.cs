using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Text;

enum SaveFormat { Triangles, Quads }
enum SaveResolution { Full=0, Half, Quarter, Eighth, Sixteenth }

/// <summary>
/// Converts a Unity terrain to a mesh with vertices, UVs and normals
/// </summary>
public class TerrainToMesh : MonoBehaviour
{
	// --------------------------------------------------------------------------------------------------------
	/// <summary>
	/// Generate a mesh from the specified terrain and position.
	/// </summary>
	/// <param name="terrain">Terrain.</param>
	/// <param name="terrainPos">Terrain position.</param>
	public static Mesh Generate(TerrainData terrain, Vector3 terrainPos)
	{
		SaveFormat saveFormat = SaveFormat.Triangles;
		SaveResolution saveResolution = SaveResolution.Quarter;

		int w = terrain.heightmapWidth;
		int h = terrain.heightmapHeight;
		Vector3 meshScale = terrain.size;
		int tRes = (int)Mathf.Pow(2, (int)saveResolution);
		meshScale = new Vector3(meshScale.x / (w - 1) * tRes, meshScale.y, meshScale.z / (h - 1) * tRes);
		Vector2 uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));
		float[,] tData = terrain.GetHeights(0, 0, w, h);

		w = (w - 1) / tRes + 1;
		h = (h - 1) / tRes + 1;
		Vector3[] tVertices = new Vector3[w * h];
		Vector2[] tUV = new Vector2[w * h];

		int[] tPolys;

		if(saveFormat == SaveFormat.Triangles) {
			tPolys = new int[(w - 1) * (h - 1) * 6];
		} else {
			tPolys = new int[(w - 1) * (h - 1) * 4];
		}

		// Build vertices and UVs
		for(int y = 0; y < h; y++) {
			for(int x = 0; x < w; x++) {
				tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(-y, tData[x * tRes, y * tRes], x)) + terrainPos;
				tUV[y * w + x] = Vector2.Scale(new Vector2(x * tRes, y * tRes), uvScale);
			}
		}

		int index = 0;
		if(saveFormat == SaveFormat.Triangles) {
			// Build triangle indices: 3 indices into vertex array for each triangle
			for(int y = 0; y < h - 1; y++) {
				for(int x = 0; x < w - 1; x++) {
					// For each grid cell output two triangles
					tPolys[index++] = (y * w) + x;
					tPolys[index++] = ((y + 1) * w) + x;
					tPolys[index++] = (y * w) + x + 1;

					tPolys[index++] = ((y + 1) * w) + x;
					tPolys[index++] = ((y + 1) * w) + x + 1;
					tPolys[index++] = (y * w) + x + 1;
				}
			}
		} else {
			// Build quad indices: 4 indices into vertex array for each quad
			for(int y = 0; y < h - 1; y++) {
				for(int x = 0; x < w - 1; x++) {
					// For each grid cell output one quad
					tPolys[index++] = (y * w) + x;
					tPolys[index++] = ((y + 1) * w) + x;
					tPolys[index++] = ((y + 1) * w) + x + 1;
					tPolys[index++] = (y * w) + x + 1;
				}
			}
		}

		// Generate a new mesh and set the vertices, UVs and triangles
		Mesh mesh = new Mesh();
		mesh.vertices = tVertices;
		mesh.uv = tUV;
		mesh.triangles = tPolys;
		mesh.Optimize();
		mesh.RecalculateNormals();
		return mesh;
	}

}