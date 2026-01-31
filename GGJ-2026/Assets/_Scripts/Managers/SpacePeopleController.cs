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
		Vector2 pendingNormal;
		bool hasPendingBounce;

		void Awake() {
			rb = GetComponent<Rigidbody2D>();

			rb.gravityScale = 0f;
			rb.linearDamping = 0f;
			rb.angularDamping = 0f;
			rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		}

		void Start() {
			rb.AddForce(new Vector2(-1f, 1f).normalized * speed, ForceMode2D.Impulse);
			//rb.linearVelocity = new Vector2(1f, 1f).normalized * speed;
		}

		/*void OnCollisionEnter2D(Collision2D collision) {
			pendingNormal = collision.contacts[0].normal;
			hasPendingBounce = true;
		}*/
/*
		void FixedUpdate() {
			if (!hasPendingBounce)
				return;

			Vector2 v = rb.linearVelocity;

			Vector2 reflected = Vector2.Reflect(v, pendingNormal).normalized * speed;

			// Small positional push to escape the collider
			rb.position += pendingNormal * separationEpsilon;

			rb.linearVelocity = reflected;
			hasPendingBounce = false;
		}*/
	}
}
