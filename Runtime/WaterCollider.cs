using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameframe.Water2D
{
  public abstract class WaterCollider : MonoBehaviour
  {
    [SerializeField]
    protected Water2D waterBody;
    public Water2D WaterBody
    {
      get => waterBody;
      set => waterBody = value;
    }
		
    [SerializeField]
    protected int leftNode;
    public int LeftNode
    {
      get => leftNode;
      set => leftNode = value;
    }
		
    [SerializeField]
    protected int rightNode;

    public int RightNode
    {
      get => rightNode;
      set => rightNode = value;
    }
  }
}

