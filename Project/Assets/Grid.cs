//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18213
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using UnityEngine;

public class Grid : MonoBehaviour
{
	public static int Resolution = 4;
	GameObject player;
	void Start ()
	{
		player = GameObject.Find("player");
		MeshFilter mf = gameObject.AddComponent<MeshFilter>();
		gameObject.AddComponent<MeshRenderer>();
		
		gameObject.renderer.material.color = new Color(0f, 0f, 0f, 0);
		
		float width = 0.05f;
		float length = 100f;


		int linesThroughOrigin = 2;
		int linesPerDirection = (int) (length / Resolution);
		// 4 directions, -X, X, Z, -Z
		int linesTotal = linesThroughOrigin + 2 * linesPerDirection;

		Vector3[] verts = new Vector3[4 * linesTotal];
		Vector2[] uv = new Vector2[4 * linesTotal];
		Vector3[] normals = new Vector3[4 * linesTotal];
		int[] tri = new int[6 * linesTotal];
		
		for (int lineNr = -linesPerDirection; lineNr < linesPerDirection; lineNr++) {
			int v_i = lineNr * 4 + 4 * linesPerDirection;

			float distance = Resolution * lineNr;
			verts[v_i + 0] = new Vector3(-width + distance, 0f, -length);
			verts[v_i + 1] = new Vector3(width + distance, 0f, -length);
			verts[v_i + 2] = new Vector3(width + distance, 0f, length);
			verts[v_i + 3] = new Vector3(-width + distance, 0f, length);
			
			normals[v_i + 0] = new Vector3(0f, 1f, 0f);
			normals[v_i + 1] = new Vector3(0f, 1f, 0f);
			normals[v_i + 2] = new Vector3(0f, 1f, 0f);
			normals[v_i + 3] = new Vector3(0f, 1f, 0f);
			
			uv[v_i + 0] = new Vector2(0f, 0f);
			uv[v_i + 1] = new Vector2(0f, 1f);
			uv[v_i + 2] = new Vector2(1f, 1f);
			uv[v_i + 3] = new Vector2(1f, 0f);
			
			int f_i = lineNr * 6 + 6 * linesPerDirection;
			tri[f_i + 0] = v_i + 2;
			tri[f_i + 1] = v_i + 1;
			tri[f_i + 2] = v_i + 0;

			tri[f_i + 3] = v_i + 0;
			tri[f_i + 4] = v_i + 3;
			tri[f_i + 5] = v_i + 2;
		}

		Mesh mesh = new Mesh();
		mesh.name = "Plane";
		mesh.vertices = verts;
		mesh.triangles = tri;
		mesh.uv = uv;
		mesh.normals = normals;
		mf.mesh = mesh;
	}

	void Update () {
		int playerX = (int) player.transform.position.x;
		int playerZ = (int) player.transform.position.z;

		// Round down to Resolution
		int newGridX = (int) (playerX / Resolution) * Resolution;
		int newGridZ = (int) (playerZ / Resolution) * Resolution;
		transform.position = new Vector3 (newGridX, 0, newGridZ);
	}
}
