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
		transform.localScale=new Vector3(defaultWidthScale,((m_Player1.position-m_Player2.position).magnitude-lengthOffset)/defaultLength,0);
		//transform.LookAt(m_Player1);
	}
}
