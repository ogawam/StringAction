using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerLine : MonoBehaviour {

	[SerializeField] GameObject prefabPart;
	HingeJoint2D partJoint = null;

	[SerializeField] float colliWidth = 0.2f;

	HingeJoint2D[] hingeJoint2D;

	List<PlayerLinePart> parts = new List<PlayerLinePart>();

	// todo : これ外に極力出さないでできるか？
	public HingeJoint2D MainJoint { get { return hingeJoint2D[0]; } }

	BoxCollider2D boxCollider2D;
	public BoxCollider2D MainCollider { get { return boxCollider2D; } }

	void Awake () {
		hingeJoint2D = GetComponents<HingeJoint2D>();
		boxCollider2D = GetComponent<BoxCollider2D>();
	}

	// Use this for initialization
	void Start () {	
	}
	
	// Update is called once per frame
	void Update () {
		if(partJoint != null)
			partJoint.enabled = true;
		if(gameObject.activeSelf)
			Debug.DrawLine(hingeJoint2D[0].connectedAnchor, transform.position - transform.rotation * boxCollider2D.size * 0.5f, Color.blue);
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
		float angle = Mathf.Rad2Deg * Mathf.Atan2(vec.x, vec.y);
		transform.position = bodyPos + (vec * 0.5f);
		transform.eulerAngles = Vector3.back * angle;
		
		// 接触点へのジョイント
		hingeJoint2D[0].connectedAnchor = rootPos;
		hingeJoint2D[0].anchor = new Vector2(0, vec.magnitude * 0.5f);
		hingeJoint2D[1].enabled = false;

		boxCollider2D.size = new Vector2(colliWidth, vec.magnitude);

		isContacted = 
		isContactedFirst = false;

		CreateChain(bodyPos);
	}

	public void UpdateJoint(Vector2 bodyPos) {
		if(partJoint != null)
			partJoint.connectedAnchor = bodyPos;
	}

	public void Hold(Vector2 holdPos) {
		CreateChain(holdPos);
	}

	public void Disjoint() {
		DeleteChain();
		isContacted = false;
		gameObject.SetActive(false);
	}

	public void CreateChain(Vector2 bodyPos) {
		DeleteChain();

		Vector2 rootPos = hingeJoint2D[0].connectedAnchor;		
		Vector2 vec = rootPos - bodyPos;
		HingeJoint2D joint = prefabPart.GetComponent<HingeJoint2D>();
		int num = Mathf.CeilToInt(Mathf.Abs(vec.magnitude / joint.connectedAnchor.y) / 2);
		Vector3 add = vec / num;
		Vector3 pos = bodyPos;
		Debug.Log("num "+ num);

		PlayerLinePart prevPart = null;
		for(int i = 0; i < num; ++i) {
			PlayerLinePart part = (Instantiate(prefabPart) as GameObject).GetComponent<PlayerLinePart>();
			part.transform.parent = transform;
			part.transform.position = pos;
			if(prevPart != null)
				part.GetComponent<AnchoredJoint2D>().connectedBody = prevPart.rigidbody2D;
			else part.GetComponent<AnchoredJoint2D>().connectedAnchor = rootPos;
			prevPart = part;
			parts.Add(part);
			pos += add;
		}		
		partJoint = parts[parts.Count-1].gameObject.AddComponent<HingeJoint2D>();
		partJoint.connectedAnchor = bodyPos;
		partJoint.enabled = false;
	}

	public void DeleteChain() {
		foreach(PlayerLinePart part in parts)
			Destroy(part.gameObject);
		parts.Clear();
		partJoint = null;		
	}
}
