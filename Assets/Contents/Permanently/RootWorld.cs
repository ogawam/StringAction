using UnityEngine;
using System.Collections;

public class RootWorld : Singleton<RootWorld> {
	void Awake () {
		instance = this;
	}

	// Use this for initialization
	void Start () {
		int layerIndex = LayerMask.NameToLayer("Ignore Raycast");
		Physics2D.IgnoreLayerCollision(layerIndex, layerIndex, true);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
