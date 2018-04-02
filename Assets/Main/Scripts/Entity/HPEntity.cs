using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaveField.Entity
{
	public class HPEntity : EntityBase
	{
		private const float MaxHealth = 100;
		[SerializeField] private float _health = MaxHealth;

		public float resonanceDamage = 100f;

		public ResonanceType m_resonanceType = ResonanceType.None;

		public override bool Init()
		{
			_health = MaxHealth;
			return true;
		}

		public bool bIsInjured()
		{
			return _health <MaxHealth;
		}

		public void Hit(float hitPoint)
		{
			_health -= hitPoint;
			if (_health <= 0)
			{
				SelfDestroy();
			}
		}
		
		
		public bool ResonanceHit(ResonanceType hitType)
		{
			if(hitType==m_resonanceType)
			{
				Hit(resonanceDamage*Time.deltaTime);
				Debug.Log("resonanceDamage");
				return true;
			}
			else
			{
				return false;
			}
		}


	}
}
