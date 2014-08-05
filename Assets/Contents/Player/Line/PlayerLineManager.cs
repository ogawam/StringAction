using UnityEngine;
using System.Collections;

// Boxコリジョンでできた接触点ごとに折れ曲がるチェイン
public class PlayerLineManager : MonoBehaviour {

	[SerializeField] PlayerLine prefabLine;
	[SerializeField] float distMax;
	[SerializeField] int lineMax;
	PlayerLine[] lines;
	GameObject strage;

	Vector3 jointPos;
	DistanceJoint2D bodyJoint;
	HingeJoint2D lineJoint;
	float totalDist;
	int lineUseNum = 0;

	// Use this for initialization
	void Start () {
		strage = new GameObject();
		strage.name = "LineStrage";
		strage.transform.parent = RootWorld.Get().transform;

		bodyJoint = gameObject.AddComponent<DistanceJoint2D>();
		bodyJoint.enabled = false;
		bodyJoint.distance = distMax;
		bodyJoint.maxDistanceOnly = true;	// フラグが逆な気がする

		lineUseNum = 0;
		lines = new PlayerLine[lineMax];
		for(int i = 0; i < lineMax; ++i) {
			PlayerLine line = Instantiate(prefabLine) as PlayerLine;
			line.transform.parent = strage.transform;
			lines[i] = line;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(bodyJoint.enabled) {
			Debug.DrawLine(transform.position, jointPos, Color.white);
		}

		if(lineUseNum > 0) {

			PlayerLine tailLine = lines[lineUseNum-1];
			Vector2 bgnPos = tailLine.MainJoint.connectedAnchor;
			Vector2 endPos = transform.position;
			Vector2 vec = bgnPos - endPos;

			// 本体への接続を更新
			lineJoint.anchor = new Vector2(0, -vec.magnitude * 0.5f);
			lineJoint.connectedAnchor = transform.position;
			lineJoint.enabled = true;		

			// 末端のラインを距離にあわせる
			tailLine.MainJoint.anchor = new Vector2(0, vec.magnitude * 0.5f);
			tailLine.MainCollider.size = new Vector2(0.1f, vec.magnitude);
			Debug.DrawLine(bgnPos, endPos, Color.green);
		//	Debug.Log(vec.magnitude);


			// 衝突チェックと中折れ
			if(tailLine.isContacted) {

				// 接触点から少し先に延ばす
				Vector2 hitPos = tailLine.GetContactPos();
				vec = hitPos - bgnPos;

				// 継続点とベクトルを更新
				endPos = hitPos + vec.normalized * 0.2f;
				vec = endPos - bgnPos;

				// 
				bodyJoint.connectedAnchor = endPos;
				bodyJoint.distance = bodyJoint.distance - vec.magnitude;

				// 
				tailLine.MainJoint.anchor = new Vector2(0, vec.magnitude * 0.5f);
				tailLine.MainCollider.size = new Vector2(0.1f, vec.magnitude);
				tailLine.isContacted = false;

				Joint(endPos);
			}
		}
	}

	public void Joint(Vector3 jointPos_) {

		if(lineUseNum < lines.Length) {
			jointPos = jointPos_;
			totalDist = Vector3.Distance(jointPos, transform.position);

			bodyJoint.enabled = true;
			bodyJoint.distance = totalDist;
			bodyJoint.connectedAnchor = jointPos;

			Destroy(lineJoint);

			lineJoint = lines[lineUseNum].gameObject.AddComponent<HingeJoint2D>();
			lineJoint.connectedAnchor = transform.position;
			lineJoint.anchor = new Vector2(0, -totalDist * 0.5f);
			lineJoint.enabled = false;						

			lines[lineUseNum].Joint(jointPos, transform.position);
			lineUseNum++;

		}
	}

	public void Disjoint() {
		bodyJoint.enabled = false;
		Destroy(lineJoint);
		lineJoint = null;
		lineUseNum = 0;
		foreach(PlayerLine line in lines)
			line.Disjoint();
	}

}
