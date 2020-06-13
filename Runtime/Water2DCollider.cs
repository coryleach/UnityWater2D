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

namespace Gameframe.Water2D
{

	public class Water2DCollider : WaterCollider
	{

		void OnTriggerEnter2D(Collider2D collider)
		{
			var rigidbody2d = collider.GetComponent<Rigidbody2D>();
			if (rigidbody2d != null)
			{
				rigidbody2d.drag = waterBody.WaterDrag;

				var momentum = rigidbody2d.velocity.y * rigidbody2d.mass;
				var xPos = rigidbody2d.position.x;

				waterBody.Collision(this, xPos, momentum);

				//Set a new V for this object
				var v = rigidbody2d.velocity;
				v.y = momentum / waterBody.NodeMass;
				rigidbody2d.velocity = v;
			}
		}

		void OnTriggerStay2D(Collider2D collider)
		{
			var rigidbody2d = collider.GetComponent<Rigidbody2D>();
			if (rigidbody2d != null)
			{
				rigidbody2d.drag = waterBody.WaterDrag;
			}
		}

		void OnTriggerExit2D(Collider2D collider)
		{
			var rigidbody2d = collider.GetComponent<Rigidbody2D>();
			if (rigidbody2d != null)
			{
				rigidbody2d.drag = 0f;
			}
		}

	}
	
}