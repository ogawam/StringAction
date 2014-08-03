using UnityEngine;
using System.Collections;

public class PlayerLine : MonoBehaviour {

	[SerializeField] float colliWidth = 0.2f;
	int waitForJointFrames = 0;

	HingeJoint2D hingeJoint2D;
	public HingeJoint2D MainJoint { get { return hingeJoint2D; } }

	BoxCollider2D boxCollider2D;
	public BoxCollider2D MainCollider { get { return boxCollider2D; } }

	void Awake () {
		hingeJoint2D = GetComponent<HingeJoint2D>();
		boxCollider2D = GetComponent<BoxCollider2D>();
	}

	// Use this for initialization
	void Start () {	
	}
	
	// Update is called once per frame
	void Update () {
		if(waitForJointFrames > 0) {
			waitForJointFrames--;
			if(waitForJointFrames == 0) {
				boxCollider2D.isTrigger = false;
			}
		}
	}

	// 接触情報
	[HideInInspector] public bool isContacted = false;
	bool isContactedFirst = false;

	Vector3 contactPos;
	public Vector3 GetContactPos() { return contactPos; }

	void OnCollisionStay2D(Collision2D collision) {
		foreach(ContactPoint2D contact in collision.contacts)
			contactPos = contact.point + (contact.normal * (colliWidth * 2));
		isContacted =
		isContactedFirst = true;
	}

	void OnCollisionExit2D(Collision2D collision) {
		isContacted = false;
	}

	void OnDrawGizmos() {
		if(isContactedFirst) {
			if(isContacted) 
				Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(contactPos, colliWidth);
			Gizmos.color = Color.white;
		}
	}

	public void Joint(Vector3 rootPos, Vector3 bodyPos) {
		gameObject.SetActive(true);

		Vector3 vec = rootPos - bodyPos;
		float angle = Vector3.Angle(rootPos, bodyPos);
		transform.position = bodyPos + (vec * 0.5f);
		transform.eulerAngles = Vector3.forward * angle;
		Debug.Log("joint angle "+ angle);
		
		hingeJoint2D.connectedAnchor = rootPos;
		hingeJoint2D.anchor = new Vector2(vec.magnitude * 0.5f, 0);
		boxCollider2D.size = new Vector2(vec.magnitude, colliWidth);
		boxCollider2D.isTrigger = true;

		isContacted = 
		isContactedFirst = false;
		waitForJointFrames = 2;		
	}

	public void Disjoint() {
		isContacted = false;
		gameObject.SetActive(false);
	}
}
