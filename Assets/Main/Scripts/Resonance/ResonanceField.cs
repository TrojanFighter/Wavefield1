using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaveField.Entity;

namespace WaveField.Resonance
{
	public class ResonanceField : MonoBehaviour
	{
		public Transform m_Player1, m_Player2;
		public float lengthOffset, defaultLength = 2.8f, defaultWidthScale = 0.5f;
		public ResonanceType m_resonanceType;
		private float m_currentScaleY;
		public float[] resonanceScaleYRange;
		public SpriteRenderer[] resonanceFieldSprite;


		void Update()
		{
			ResonanceFieldMoveAndRotate();
			ResonanceFieldStateCheck();
		}

		void ResonanceFieldMoveAndRotate()
		{
			transform.localPosition = m_Player1.position / 2 + m_Player2.position / 2;

			if ((m_Player1.position - m_Player2.position).sqrMagnitude < 0.1f)
			{
				return;
			}

			m_currentScaleY = ((m_Player1.position - m_Player2.position).magnitude - lengthOffset) / defaultLength;

			transform.localScale = new Vector3(defaultWidthScale,m_currentScaleY, 0);
			//transform.LookAt(m_Player1);

			var offset = new Vector2(m_Player1.position.x - m_Player2.position.x, m_Player1.position.y - m_Player2.position.y);
			var angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
			transform.rotation =Quaternion.Euler(0, 0,90 + angle); // Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 90-angle, 0), _TurnRateEase);
		}

		void ResonanceFieldStateCheck()
		{
			if (resonanceScaleYRange.Length < (int) ResonanceType.None-1)
			{
				Debug.LogError("resonanceLength too short");
				return;
			}

			if (m_currentScaleY < resonanceScaleYRange[0])
			{
				SwitchState(ResonanceType.Yellow);
			}
			else if (m_currentScaleY < resonanceScaleYRange[1])
			{
				SwitchState(ResonanceType.Green);
			}
			else if (m_currentScaleY < resonanceScaleYRange[2])
			{
				SwitchState(ResonanceType.Red);
			}
			else if (m_currentScaleY < resonanceScaleYRange[3])
			{
				SwitchState(ResonanceType.Purple);
			}
			else
			{
				SwitchState(ResonanceType.None);
			}

		}

		void SwitchState(ResonanceType state)
		{
			m_resonanceType = state;
			switch (state)
			{
				case ResonanceType.None :
					resonanceFieldSprite[0].color = Color.clear;
					resonanceFieldSprite[1].color = Color.clear;
					break;
				case ResonanceType.Yellow :
					resonanceFieldSprite[0].color = Color.yellow;
					resonanceFieldSprite[1].color = Color.yellow;
					break;
				case ResonanceType.Green : 
					resonanceFieldSprite[0].color = Color.green;
					resonanceFieldSprite[1].color = Color.green;
					break;
				case ResonanceType.Red :
					resonanceFieldSprite[0].color = Color.red;
					resonanceFieldSprite[1].color = Color.red;
					break;
				case ResonanceType.Purple : 
					resonanceFieldSprite[0].color = Color.magenta;
					resonanceFieldSprite[1].color = Color.magenta;
					break;
			}
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			Debug.Log(other.gameObject.name);
		}

		private void OnTriggerStay2D(Collider2D other)
		{
			//if (other.gameObject.layer == 1 << LayerMask.NameToLayer("EnemyCollider"))
			{
				Debug.Log(other.gameObject.name);
				other.gameObject.GetComponent<HPEntity>().ResonanceHit(m_resonanceType);
			}
		}
	}
}
