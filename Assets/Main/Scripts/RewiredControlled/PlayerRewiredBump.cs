using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  WaveField.RewiredBase
{
	public class PlayerRewiredBump : RewiredBase
	{

		public float ExplosionForce = 50f;
		public float ExplosionRadius = 4f;
		public float FireRate = .1f;
		public float FireShakeForce = .8f;
		public float FireShakeDuration = .02f;

		public float FireGapTime = 4f;
		private float lastFireTime=-4f;

		public GameObject m_forceFieldIndicator;
		
		protected override void GetInput()
		{
			if (!_functionAllowed)
			{
				return;
			}

			if (Time.timeSinceLevelLoad - lastFireTime > FireGapTime)
			{
				if (player.GetButtonDown("Bump"))
				{
					StartCoroutine(Bump());
					lastFireTime = Time.timeSinceLevelLoad;
					//m_forceFieldIndicator.SetActive(false);
				}
				else
				{
					m_forceFieldIndicator.SetActive(true);
				}
			}
			else
			{
				m_forceFieldIndicator.SetActive(false);
			}
		}
		
		IEnumerator Bump()
		{
			while (_functionAllowed&&player.GetButton("Bump"))
			{
				Vector3 explosionPos = transform.position;
				Collider[] colliders = Physics.OverlapSphere(explosionPos, ExplosionRadius);
				foreach (Collider hit in colliders)
				{
					Rigidbody rb = hit.GetComponent<Rigidbody>();

					if (rb != null&&rb!=GetComponent<Rigidbody>())
						rb.AddExplosionForce(ExplosionForce, explosionPos, ExplosionRadius, 0F);
				}
				/*var angle = _transform.rotation.eulerAngles.y - 90;
				var radians = angle * Mathf.Deg2Rad;
				var vForce = new Vector2((float)Mathf.Sin(radians), (float)Mathf.Cos(radians)) * FireShakeForce;

				ProCamera2DShake.Instance.ApplyShakesTimed(new Vector2[]{ vForce }, new Vector3[]{Vector3.zero}, new float[]{ FireShakeDuration });*/

				yield return new WaitForSeconds(FireRate);
			}
		}
	}
}
