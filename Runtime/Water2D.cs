using UnityEngine;

namespace Gameframe.Water2D
{

	public class Water2D : MonoBehaviour
	{
		[SerializeField, Range(0f, 1f)] private float springConstant = 0.236f;
		[SerializeField, Range(0f, 1f)] private float damping = 0.878f;
		[SerializeField, Range(0f, 0.2f)] private float spread = 0.0173f;

		[SerializeField] private GameObject splashFx;
		[SerializeField] private Material material;

		[SerializeField] private int nodeDensity = 1;

		[SerializeField] private float zOffset;

		[SerializeField] private float width = 10;
		[SerializeField] private float height = 10;

		[SerializeField] private bool useCollider3D;
		[SerializeField] private float nodeMass = 40;
		public float NodeMass => nodeMass;
		
		[SerializeField] private float waterDrag = 2f;
		public float WaterDrag => waterDrag;

		private Water2DSurfaceNodeData[] _nodes;
		private Water2DMeshData[] _meshes;

		private void Start()
		{
			Build();
		}

		private void FixedUpdate()
		{
			PropagateWaves();
			UpdateNodes();
		}

		private void Update()
		{
			UpdateMeshes();
		}

		private const float Left = 0f;
		private const float Top = 0f;
		private const float BaseHeight = 0;
		
		private int _edgeCount;
		private int _nodeCount;

		[ContextMenu("Build")]
		public void Build()
		{
			ClearChildren();
			SpawnWater();
		}

		private void ClearChildren()
		{
			var children = gameObject.transform.childCount;
			for (var i = children - 1; i >= 0; i--)
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

		private void SpawnWater()
		{
			_edgeCount = Mathf.RoundToInt(width) * nodeDensity;
			_nodeCount = _edgeCount + 1;

			var meshWidth = width / _edgeCount;

			_nodes = new Water2DSurfaceNodeData[_nodeCount];
			_meshes = new Water2DMeshData[_edgeCount];

			//Build Nodes
			for (var i = 0; i < _nodes.Length; i++)
			{

				var node = new Water2DSurfaceNodeData();
				_nodes[i] = node;

				node.yPos = Top;
				node.xPos = Left + (width * i) / _edgeCount;
				node.acceleration = 0;
				node.velocity = 0;

			}

			//Build Meshes
			for (var i = 0; i < _meshes.Length; i++)
			{

				var leftNode = _nodes[i];
				var rightNode = _nodes[i + 1];

				_meshes[i] = new Water2DMeshData();
				var edgeMesh = _meshes[i];

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

				var triangles = new int[6] {0, 1, 3, 3, 1, 2};

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
					waterCollider.WaterBody = this;
					waterCollider.LeftNode = i;
					waterCollider.RightNode = i + 1;
				}
				else
				{
					edgeMesh.collider = edgeMesh.gameObject.AddComponent<BoxCollider2D>();
					edgeMesh.collider.size = new Vector2(meshWidth, height);
					edgeMesh.collider.offset = new Vector2(meshWidth * 0.5f, height * -0.5f);
					edgeMesh.collider.isTrigger = true;

					var waterCollider = edgeMesh.collider.gameObject.AddComponent<Water2DCollider>();
					waterCollider.WaterBody = this;
					waterCollider.LeftNode = i;
					waterCollider.RightNode = i + 1;
				}

			}


		}

		private void UpdateMeshes()
		{
			var meshWidth = width / _edgeCount;

			for (var i = 0; i < _meshes.Length; i++)
			{

				var leftNode = _nodes[i];
				var rightNode = _nodes[i + 1];
				var edgeMesh = _meshes[i];

				//Build Mesh
				var verts = new Vector3[4];
				verts[0] = new Vector3(0, leftNode.yPos, zOffset);
				verts[1] = new Vector3(meshWidth, rightNode.yPos, zOffset);
				verts[2] = new Vector3(meshWidth, -height, zOffset);
				verts[3] = new Vector3(0, -height, zOffset);

				edgeMesh.mesh.vertices = verts;

			}

		}

		private void UpdateNodes()
		{

			//Update node velocity and yPos
			for (var i = 0; i < _nodes.Length; i++)
			{

				var node = _nodes[i];
				var force = springConstant * (node.yPos - BaseHeight) + node.velocity * damping;
				node.acceleration = -force / nodeMass;
				node.velocity += node.acceleration;
				node.yPos += node.velocity;
			}

		}

		private void PropagateWaves()
		{

			var leftDelta = new float[_nodes.Length];
			var rightDelta = new float[_nodes.Length];

			const int iterations = 2;

			for (var j = 0; j < iterations; j++)
			{

				//Propagate Outward from each node
				for (var i = 0; i < _nodes.Length; i++)
				{

					Water2DSurfaceNodeData leftNode = null;
					Water2DSurfaceNodeData node = _nodes[i];
					Water2DSurfaceNodeData rightNode = null;

					if (i + 1 < _nodes.Length)
					{
						rightNode = _nodes[i + 1];
					}

					if (i - 1 >= 0)
					{
						leftNode = _nodes[i - 1];
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
			for (var i = 0; i < _nodes.Length; i++)
			{

				//Left
				if (i - 1 >= 0)
				{
					_nodes[i - 1].yPos += leftDelta[i];
				}

				//Right
				if (i + 1 < _nodes.Length)
				{
					_nodes[i + 1].yPos += rightDelta[i];
				}

			}

		}

		public void Collision(Water2DCollider waterCollider, float xPos, float momentum)
		{
			var leftNode = _nodes[waterCollider.LeftNode];
			var rightNode = _nodes[waterCollider.RightNode];

			var velocity = momentum / nodeMass;

			float leftWeight;

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
				var localWidth = rightNode.xPos - leftNode.xPos;
				leftWeight = (xPos - leftNode.xPos) / localWidth;
			}

			//Spawn Splash Partile
			leftNode.velocity += velocity * leftWeight;
			rightNode.velocity += velocity * (1f - leftWeight);

			if (splashFx != null)
			{
				var splashX = (rightNode.xPos + leftNode.xPos) * 0.5f;
				var pt = transform.TransformPoint(new Vector3(splashX, 0, -5));
				Instantiate(splashFx, pt, Quaternion.identity);
			}

		}

		public void Collision(Water3DCollider waterCollider, float xPos, float momentum)
		{

			var leftNode = _nodes[waterCollider.LeftNode];
			var rightNode = _nodes[waterCollider.RightNode];

			var velocity = momentum / nodeMass;

			float leftWeight;

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
				var localWidth = rightNode.xPos - leftNode.xPos;
				leftWeight = (xPos - leftNode.xPos) / localWidth;
			}

			//Spawn Splash Particle
			leftNode.velocity += velocity * leftWeight;
			rightNode.velocity += velocity * (1f - leftWeight);

			if (splashFx != null)
			{
				var splashX = (rightNode.xPos + leftNode.xPos) * 0.5f;
				var pt = transform.TransformPoint(new Vector3(splashX, 0, 0));
				Instantiate(splashFx, pt, Quaternion.identity);
			}

		}

	}

}