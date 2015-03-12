using UnityEngine;
using System.Collections;

public class Voxel : MonoBehaviour {

	private bool active;
	// Use this for initialization
	void Start () {
		active = false;

	}

	public void SwitchActive()
	{
		active = !active;
		MeshRenderer m = this.GetComponent<MeshRenderer>();
		if(active)
		{
			m.material.SetColor("_Color", Color.black);
		}
		else
		{
			m.material.SetColor("_Color", Color.white);
		}
	}

	public bool IsActive()
	{
		return active;
	}
	

	// Update is called once per frame
	void Update () {
	
	}
}
