using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServiceBase {
	public ServiceBase()
	{
		Init();
	}

	// Use this for initialization
	public virtual void Init() {
		
	}
	
	// Update is called once per frame
	public virtual void Update() {
		
	}

	public virtual void Destroy()
	{
	}
}

public class ServiceBase<Object> {
	public ServiceBase()
	{
		Init();
	}

	// Use this for initialization
	public virtual void Init() {
		
	}
	
	// Update is called once per frame
	public virtual void Update() {
		
	}

	public virtual void Destroy()
	{
	}
}
