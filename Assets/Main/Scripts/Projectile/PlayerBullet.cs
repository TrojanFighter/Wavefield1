using UnityEngine;
using System.Collections;
using WaveField.Entity;

namespace WaveField.Projectile
{
	public class PlayerBullet : MonoBehaviour
	{
		public float BulletDuration = 1f;
		public float BulletSpeed = 50f;
		public float SkinWidth = .1f;

		public LayerMask CollisionMask;

		//public float  PushForce = 300f, PushDistance = 0.4f;
		public int BulletDamage = 5;//,ForceRadius=2f;

		Transform _transform;

		RaycastHit _raycastHit;
		Vector2 _collisionPoint;

		float _startTime;
		bool _exploding;
		Vector3 _lastPos;

		void Awake()
		{
			_transform = this.transform;
		}

		void OnEnable()
		{
			_exploding = false;
			_startTime = Time.time;
		}

		void Update()
		{
			if (_exploding)
				return;

			_lastPos = _transform.position;
			_transform.position += _transform.TransformDirection(Vector3.up) * BulletSpeed * Time.deltaTime;
			//_transform.Translate( transform.TransformDirection(Vector3.up) * BulletSpeed * Time.deltaTime);

			/*if(Physics.Raycast(_lastPos, _transform.position - _lastPos, out _raycastHit, (_lastPos - _transform.position).magnitude + SkinWidth, CollisionMask))
			{
				_collisionPoint = _raycastHit.point;

				//_transform.up = _raycastHit.normal;

				Collide();
			}*/

			if (Time.time - _startTime > BulletDuration)
				Disable();
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			Collide(other);
		}

		void Collide(Collider2D other)
		{
			//_exploding = true;
			//_transform.position = _collisionPoint;

			if (other.GetComponent<HPEntity>())
			{
				_exploding = true;
				//_raycastHit.collider.SendMessageUpwards("Hit", BulletDamage, SendMessageOptions.DontRequireReceiver);
				//_raycastHit.collider.attachedRigidbody.AddExplosionForce(PushForce, transform.position, ForceRadius, 0F);
				//_raycastHit.collider.attachedRigidbody.AddForce(PushForce*_transform.forward,ForceMode.Impulse);
				//Debug.Log("_raycastHit.collider:" + _raycastHit.collider.gameObject.name);
				other.GetComponent<HPEntity>().Hit(BulletDamage);
				Disable();
			}
			
		}

		void Disable()
		{
			gameObject.SetActive(false);
		}
	}
}