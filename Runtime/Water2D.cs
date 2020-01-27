/*
The MIT License (MIT)

Copyright (c) 2014 Cory R. Leach

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using UnityEngine;
using System.Collections;

namespace Gameframe.Water2D
{

	public class Water2D : MonoBehaviour
	{

		[Range(0f, 1f)] public float springConstant = 0.236f;
		[Range(0f, 1f)] public float damping = 0.878f;
		[Range(0f, 0.2f)] public float spread = 0.0173f;

		public GameObject splashFx;
		public Material material;

		public int nodeDensity = 1;

		public float zOffset = 0;

		public float width = 10;
		public float height = 10;

		public float nodeMass = 40;
		public float waterDrag = 2f;

		public bool useCollider3D = false;

		Water2DSurfaceNode[] nodes;
		Water2DMesh[] meshes;

		// Use this for initialization
		void Start()
		{
			Build();
		}

		void FixedUpdate()
		{
			PropagateWaves();
			UpdateNodes();
		}

		void Update()
		{
			UpdateMeshes();
		}

		float left = 0f;
		float top = 0f;

		float baseHight = 0;

		int edgeCount = 0;
		int nodeCount = 0;

		[ContextMenu("Build")]
		public void Build()
		{
			ClearChildren();
			SpawnWater();
		}

		public void ClearChildren()
		{
			int children = gameObject.transform.childCount;
			for (int i = children - 1; i >= 0; i--)
			{
				var child = gameObject.transform.GetChild(i);

				if (Application.isPlaying)
				{
					Destroy(child.gameObject);
				}
				else
				{
					DestroyImmediate(child.gameObject);
				}

			}
		}

		public void SpawnWater()
		{

			edgeCount = Mathf.RoundToInt(width) * nodeDensity;
			nodeCount = edgeCount + 1;

			float meshWidth = width / edgeCount;

			nodes = new Water2DSurfaceNode[nodeCount];
			meshes = new Water2DMesh[edgeCount];

			//Build Nodes
			for (int i = 0; i < nodes.Length; i++)
			{

				var node = new Water2DSurfaceNode();
				nodes[i] = node;

				node.yPos = top;
				node.xPos = left + (width * i) / edgeCount;
				node.acceleration = 0;
				node.velocity = 0;

			}

			//Build Meshes
			for (int i = 0; i < meshes.Length; i++)
			{

				var leftNode = nodes[i];
				var rightNode = nodes[i + 1];

				meshes[i] = new Water2DMesh();
				var edgeMesh = meshes[i];

				//Build Mesh
				edgeMesh.mesh = new Mesh();
				Vector3[] verts = new Vector3[4];
				verts[0] = new Vector3(0, leftNode.yPos, zOffset);
				verts[1] = new Vector3(meshWidth, rightNode.yPos, zOffset);
				verts[2] = new Vector3(meshWidth, -height, zOffset);
				verts[3] = new Vector3(0, -height, zOffset);

				// (0)      (1)  
				//  +--------+
				//  |        |
				//  +--------+
				// (3)      (2)

				Vector2[] uv = new Vector2[4];
				uv[0] = new Vector2(0, 1);
				uv[1] = new Vector2(1, 1);
				uv[2] = new Vector2(0, 0);
				uv[3] = new Vector3(1, 0);

				int[] triangles = new int[6] {0, 1, 3, 3, 1, 2};

				edgeMesh.mesh.vertices = verts;
				edgeMesh.mesh.uv = uv;
				edgeMesh.mesh.triangles = triangles;

				//Build Game Object
				edgeMesh.gameObject = new GameObject("Mesh " + i);
				edgeMesh.gameObject.transform.parent = transform;
				edgeMesh.gameObject.transform.localPosition = new Vector3(leftNode.xPos, 0, 0);

				edgeMesh.renderer = edgeMesh.gameObject.AddComponent<MeshRenderer>();
				edgeMesh.filter = edgeMesh.gameObject.AddComponent<MeshFilter>();

				edgeMesh.renderer.material = material;
				edgeMesh.filter.mesh = edgeMesh.mesh;

				//Build Collider
				if (useCollider3D)
				{
					edgeMesh.collider3D = edgeMesh.gameObject.AddComponent<BoxCollider>();
					edgeMesh.collider3D.size = new Vector3(meshWidth, height, 1);
					edgeMesh.collider3D.center = new Vector3(meshWidth * 0.5f, height * -0.5f, 0.5f);
					edgeMesh.collider3D.isTrigger = true; //*/

					var waterCollider = edgeMesh.collider3D.gameObject.AddComponent<Water3DCollider>();
					waterCollider.waterBody = this;
					waterCollider.leftNode = i;
					waterCollider.rightNode = i + 1;
				}
				else
				{
					edgeMesh.collider = edgeMesh.gameObject.AddComponent<BoxCollider2D>();
					edgeMesh.collider.size = new Vector2(meshWidth, height);
					edgeMesh.collider.offset = new Vector2(meshWidth * 0.5f, height * -0.5f);
					edgeMesh.collider.isTrigger = true;

					var waterCollider = edgeMesh.collider.gameObject.AddComponent<Water2DCollider>();
					waterCollider.waterBody = this;
					waterCollider.leftNode = i;
					waterCollider.rightNode = i + 1;
				}

			}


		}

		void UpdateMeshes()
		{
			float meshWidth = width / edgeCount;

			for (int i = 0; i < meshes.Length; i++)
			{

				var leftNode = nodes[i];
				var rightNode = nodes[i + 1];
				var edgeMesh = meshes[i];

				//Build Mesh
				Vector3[] verts = new Vector3[4];
				verts[0] = new Vector3(0, leftNode.yPos, zOffset);
				verts[1] = new Vector3(meshWidth, rightNode.yPos, zOffset);
				verts[2] = new Vector3(meshWidth, -height, zOffset);
				verts[3] = new Vector3(0, -height, zOffset);

				edgeMesh.mesh.vertices = verts;

			}

		}

		void UpdateNodes()
		{

			//Update node velocity and yPos
			for (int i = 0; i < nodes.Length; i++)
			{

				var node = nodes[i];
				float force = springConstant * (node.yPos - baseHight) + node.velocity * damping;
				node.acceleration = -force / nodeMass;
				node.velocity += node.acceleration;
				node.yPos += node.velocity; // * Time.deltaTime;

			}

		}

		void PropagateWaves()
		{

			float[] leftDelta = new float[nodes.Length];
			float[] rightDelta = new float[nodes.Length];

			int iterations = 2;

			for (int j = 0; j < iterations; j++)
			{

				//Propagate Outward from each node
				for (int i = 0; i < nodes.Length; i++)
				{

					Water2DSurfaceNode leftNode = null;
					Water2DSurfaceNode node = nodes[i];
					Water2DSurfaceNode rightNode = null;

					if (i + 1 < nodes.Length)
					{
						rightNode = nodes[i + 1];
					}

					if (i - 1 >= 0)
					{
						leftNode = nodes[i - 1];
					}

					//Multiply height difference by a spread factor
					if (leftNode != null)
					{
						leftDelta[i] = spread * (node.yPos - leftNode.yPos);
						leftNode.velocity += leftDelta[i];
					}

					if (rightNode != null)
					{
						rightDelta[i] = spread * (node.yPos - rightNode.yPos);
						rightNode.velocity += rightDelta[i];
					}

				}

			}

			//Finally Update Positions
			for (int i = 0; i < nodes.Length; i++)
			{

				//Left
				if (i - 1 >= 0)
				{
					nodes[i - 1].yPos += leftDelta[i];
				}

				//Right
				if (i + 1 < nodes.Length)
				{
					nodes[i + 1].yPos += rightDelta[i];
				}

			}

		}

		public void Collision(Water2DCollider waterCollider, float xPos, float momentum)
		{

			var leftNode = nodes[waterCollider.leftNode];
			var rightNode = nodes[waterCollider.rightNode];

			float velocity = momentum / nodeMass;

			float leftWeight = 1f;

			if (xPos < leftNode.xPos)
			{
				leftWeight = 1f;
			}
			else if (xPos > rightNode.xPos)
			{
				leftWeight = 0f;
			}
			else
			{
				float width = rightNode.xPos - leftNode.xPos;
				leftWeight = (xPos - leftNode.xPos) / width;
			}

			//Spawn Splash Partile
			leftNode.velocity += velocity * leftWeight;
			rightNode.velocity += velocity * (1f - leftWeight);

			if (splashFx != null)
			{
				float splashX = (rightNode.xPos + leftNode.xPos) * 0.5f;
				var pt = transform.TransformPoint(new Vector3(splashX, 0, -5));
				Instantiate(splashFx, pt, Quaternion.identity);
			}

		}

		public void Collision(Water3DCollider waterCollider, float xPos, float momentum)
		{

			var leftNode = nodes[waterCollider.leftNode];
			var rightNode = nodes[waterCollider.rightNode];

			float velocity = momentum / nodeMass;

			float leftWeight = 1f;

			if (xPos < leftNode.xPos)
			{
				leftWeight = 1f;
			}
			else if (xPos > rightNode.xPos)
			{
				leftWeight = 0f;
			}
			else
			{
				float width = rightNode.xPos - leftNode.xPos;
				leftWeight = (xPos - leftNode.xPos) / width;
			}

			//Spawn Splash Partile
			leftNode.velocity += velocity * leftWeight;
			rightNode.velocity += velocity * (1f - leftWeight);

			if (splashFx != null)
			{
				float splashX = (rightNode.xPos + leftNode.xPos) * 0.5f;
				var pt = transform.TransformPoint(new Vector3(splashX, 0, 0));
				Instantiate(splashFx, pt, Quaternion.identity);
			}

		}

	}

	[System.Serializable]
	public class Water2DSurfaceNode
	{
		public float xPos;
		public float yPos;
		public float velocity;
		public float acceleration;
	}

	[System.Serializable]
	public class Water2DMesh
	{
		public GameObject gameObject;
		public MeshRenderer renderer;
		public MeshFilter filter;
		public Mesh mesh;
		public BoxCollider2D collider;
		public BoxCollider collider3D;
	}

}