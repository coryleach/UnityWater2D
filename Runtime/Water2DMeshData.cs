using UnityEngine;

namespace Gameframe.Water2D
{
  [System.Serializable]
  public class Water2DMeshData
  {
  	public GameObject gameObject;
  	public MeshRenderer renderer;
  	public MeshFilter filter;
  	public Mesh mesh;
  	public BoxCollider2D collider;
  	public BoxCollider collider3D;
  }
}
