using UnityEngine;
using System.Collections;

public class RootWorld : Singleton<RootWorld> {
	[SerializeField] PlayerUnit prefabPlayer;

	void Awake () {
		Instantiate(prefabPlayer);
		instance = this;
	}

	// Use this for initialization
	void Start () {
		int layerUnit = LayerMask.NameToLayer("Unit");
		int layerLine = LayerMask.NameToLayer("Line");
		int layerChain = LayerMask.NameToLayer("Chain");
		int layerAnchor = LayerMask.NameToLayer("Anchor");

		Physics2D.IgnoreLayerCollision(layerUnit, layerLine, true);
		Physics2D.IgnoreLayerCollision(layerUnit, layerChain, true);
		Physics2D.IgnoreLayerCollision(layerLine, layerLine, true);
		Physics2D.IgnoreLayerCollision(layerChain, layerChain, true);
		Physics2D.IgnoreLayerCollision(layerLine, layerChain, true);

		Physics2D.IgnoreLayerCollision(layerAnchor, layerUnit, true);
		Physics2D.IgnoreLayerCollision(layerAnchor, layerLine, true);
		Physics2D.IgnoreLayerCollision(layerAnchor, layerChain, true);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
