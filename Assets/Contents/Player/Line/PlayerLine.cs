using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerLine : MonoBehaviour {

	[SerializeField] GameObject prefabChain;
	HingeJoint2D chainJoint = null;

	[SerializeField] float colliWidth = 0.2f;

	HingeJoint2D[] hingeJoint2D;

	List<PlayerChain> chains = new List<PlayerChain>();

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
		if(chainJoint != null)
			chainJoint.enabled = true;
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
		if(chainJoint != null)
			chainJoint.connectedAnchor = bodyPos;
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
		HingeJoint2D joint = prefabChain.GetComponent<HingeJoint2D>();
		int num = Mathf.CeilToInt(Mathf.Abs(vec.magnitude / joint.connectedAnchor.y) / 2);
		num = Mathf.Max(num, 1);
		Vector3 add = vec / num;
		Vector3 pos = bodyPos;
		Debug.Log("num "+ num);

		PlayerChain prevChain = null;
		for(int i = 0; i < num; ++i) {
			PlayerChain chain = (Instantiate(prefabChain) as GameObject).GetComponent<PlayerChain>();
			chain.transform.parent = transform;
			chain.transform.position = pos;
			if(prevChain != null)
				chain.GetComponent<AnchoredJoint2D>().connectedBody = prevChain.rigidbody2D;
			else chain.GetComponent<AnchoredJoint2D>().connectedAnchor = rootPos;
			prevChain = chain;
			chains.Add(chain);
			pos += add;
		}		
		chainJoint = chains[chains.Count-1].gameObject.AddComponent<HingeJoint2D>();
		chainJoint.connectedAnchor = bodyPos;
		chainJoint.enabled = false;
	}

	public void DeleteChain() {
		foreach(PlayerChain chain in chains)
			Destroy(chain.gameObject);
		chains.Clear();
		chainJoint = null;		
	}
}
