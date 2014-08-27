using UnityEngine;
using System.Collections;

public class PlayerChainAnchor : MonoBehaviour {

	public bool isHit = false;
	public float lifeTime = 5;
	public Vector3 contactPos = Vector3.zero;

	void Awake() {
		lifeTime = 5;
	}

	void Update() {
		if(lifeTime > 0)
			lifeTime -= Time.deltaTime;
	}

	void OnCollisionStay2D(Collision2D collision) {
		foreach(ContactPoint2D contact in collision.contacts)
			contactPos = contact.point + (contact.normal * 0.2f);
		isHit = true;
	}
}
