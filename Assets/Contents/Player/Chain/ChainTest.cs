using UnityEngine;
using System.Collections;

public class ChainTest : MonoBehaviour {

	public HingeJoint2D mainJoint; 

	// Use this for initialization
	public Collider2D collider2D;
	
	// Update is called once per frame
	void Update () {
//		isContacted = false;
	}

	Vector3 contactPos;
	public bool isContacted = false;
//	public bool IsContacted() { return isContacted; }
	public Vector3 GetContactPos() { return contactPos; }

	void OnCollisionStay2D(Collision2D colli) {
		isContacted = true;
		foreach(ContactPoint2D contact in colli.contacts) {
			contactPos = contact.point + contact.normal * 0.2f;
		}
	}

	void OnCollisionExit2D(Collision2D colli) {
		isContacted = false;
	}

	void OnDrawGizmos() {
		if(PlayerUnit.Get() && PlayerUnit.Get().isDispGizmos) {
			if(isContacted) 
				Gizmos.color = Color.red;
			Gizmos.DrawSphere(contactPos, 8);
			Gizmos.color = Color.white;
		}
	}

	public void Disappear() {
		isContacted = false;
		gameObject.SetActive(false);
	}
}
