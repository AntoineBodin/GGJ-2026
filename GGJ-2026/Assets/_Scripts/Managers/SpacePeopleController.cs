using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets._Scripts.Managers {
	internal class SpacePeopleController : MonoBehaviour {

		[Header("Movement")]
		[SerializeField] private float speed = 8f;
		[SerializeField] float separationEpsilon = 0.01f;

		Rigidbody2D rb;

		void Awake() {
			rb = GetComponent<Rigidbody2D>();

			rb.gravityScale = 0f;
			rb.linearDamping = 0f;
			rb.angularDamping = 0f;
			rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		}

		void Start() {
			rb.AddForce(new Vector2(-1f, 1f).normalized * speed, ForceMode2D.Impulse);
			float randomTorque = UnityEngine.Random.Range(-2f, 2f);
			rb.AddTorque(randomTorque * 0.02f, ForceMode2D.Impulse);
		}
	}
}
