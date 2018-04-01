using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResonanceField : MonoBehaviour
{
	public Transform m_Player1, m_Player2;
	public float lengthOffset,defaultLength=2.8f, defaultWidthScale=0.5f;

	void Update()
	{
		transform.localPosition = m_Player1.position / 2 + m_Player2.position / 2;
		
		if((m_Player1.position-m_Player2.position).sqrMagnitude<0.1f){
			return;
		}
		transform.localScale=new Vector3(defaultWidthScale,((m_Player1.position-m_Player2.position).magnitude-lengthOffset)/defaultLength,0);
		//transform.LookAt(m_Player1);
		
		var offset = new Vector2(m_Player1.position.x - m_Player2.position.x, m_Player1.position.y - m_Player2.position.y);
		var angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
		transform.rotation =Quaternion.Euler( 0,0,90+angle); // Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 90-angle, 0), _TurnRateEase);
	}
}
