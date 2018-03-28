using System.Collections;
using Rewired;
using UnityEngine;
using WaveField.ResourceManagement;

namespace WaveField.RewiredBase
{

	public class PlayerRewiredFire : RewiredBase
	{
		public Pool BulletPool;
		public Transform WeaponTip;

		public float FireRate = .1f;
		public float FireShakeForce = .8f;
		public float FireShakeDuration = .02f;
		public float lastLongBurstStartTime,  LongBurstGapTime=1f;

		public int LongBurstLength = 6;
		


		protected override void GetInput()
		{
			if (!_functionAllowed)
			{
				return;
			}

			if (Time.timeSinceLevelLoad - lastLongBurstStartTime > LongBurstGapTime)
			{
				if (player.GetButtonDown("Launch"))
				{
					StartCoroutine(Fire());
					lastLongBurstStartTime = Time.timeSinceLevelLoad;
				}
			}
		}
		
		IEnumerator Fire()
		{
			int LongBurstCount = LongBurstLength;
			while (_functionAllowed&&player.GetButton("Launch")&&LongBurstCount>=0)
			{
				var bullet = BulletPool.nextThing; 
				bullet.transform.position = WeaponTip.position;
				bullet.transform.rotation = _transform.rotation;

				var angle = _transform.rotation.eulerAngles.y;//- 90;
				var radians = angle * Mathf.Deg2Rad;
				var vForce = new Vector2((float)Mathf.Sin(radians), (float)Mathf.Cos(radians)) * FireShakeForce;

				//ProCamera2DShake.Instance.ApplyShakesTimed(new Vector2[]{ vForce }, new Vector3[]{Vector3.zero}, new float[]{ FireShakeDuration });

				LongBurstCount--;
				yield return new WaitForSeconds(FireRate);
			}
		}
	}
}
