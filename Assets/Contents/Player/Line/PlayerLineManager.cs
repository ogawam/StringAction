using UnityEngine;
using System.Collections;

// Boxコリジョンでできた接触点ごとに折れ曲がるチェイン
public class PlayerLineManager : MonoBehaviour {

	[SerializeField] PlayerLine prefabLine;
	[SerializeField] float distMax;
	[SerializeField] float tensionDist;
	[SerializeField] int lineMax;
	PlayerLine[] lines;
	GameObject strage;

	Vector3 jointPos;
	DistanceJoint2D bodyJoint;
	HingeJoint2D lineJoint;
	float totalDist = 0;
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
		if(chainAnchor != null) {
			if(chainAnchor.isHit) {
				Vector3 pos = chainAnchor.transform.position;
				Disjoint();
				Joint(pos);				
			}
			else if(chainAnchor.lifeTime < 0) {
				Disjoint();
			}
		}

		if(bodyJoint.enabled) {
			Debug.DrawLine(transform.position, jointPos, Color.white);
		}

		if(lineUseNum > 0) {

			PlayerLine tailLine = lines[lineUseNum-1];
			Vector2 bgnPos = tailLine.MainJoint.connectedAnchor;
			Vector2 endPos = transform.position;
			Vector2 vec = bgnPos - endPos;

			// 本体への接続を更新
			if(lineJoint != null) {
				lineJoint.anchor = new Vector2(0, -vec.magnitude * 0.5f);
				lineJoint.connectedAnchor = transform.position;
				lineJoint.enabled = true;		
			}

			// 末端のラインを距離にあわせる
			tailLine.MainJoint.anchor = new Vector2(0, vec.magnitude * 0.5f);
			tailLine.MainCollider.size = new Vector2(0.1f, vec.magnitude);
			tailLine.UpdateJoint(transform.position);

			float angle = Mathf.Rad2Deg * Mathf.Atan2(vec.x, vec.y);
			tailLine.transform.position = endPos + (vec * 0.5f);
			tailLine.transform.eulerAngles = Vector3.back * angle;
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

				// 折れた場所までの長さにする
				tailLine.MainJoint.anchor = new Vector2(0, vec.magnitude * 0.5f);
				tailLine.MainCollider.size = new Vector2(0.1f, vec.magnitude);
				tailLine.isContacted = false;

				// ブラブラさせない
				tailLine.Hold(endPos);

				Joint(endPos, bodyJoint.distance - vec.magnitude);
			}
			// 中折れがある場合
			else if(lineUseNum > 1) {
				bgnPos = lines[lineUseNum-2].MainJoint.connectedAnchor;
				endPos = transform.position;
				vec = bgnPos - endPos;

				// 親の根元まで障害物がなければ再び融合
				RaycastHit2D result = Physics2D.Raycast(endPos, vec, vec.magnitude, 1);
				if(result.collider != null) {
//					Debug.DrawRay(endPos, vec, Color.red);
//					Debug.DrawRay(result.point, result.normal * 10, Color.red);
				}
				else {
					Debug.DrawRay(endPos, vec, Color.yellow);
					lines[lineUseNum-1].Disjoint();

					lineUseNum--;
					tailLine = lines[lineUseNum-1];

					bodyJoint.connectedAnchor = tailLine.MainJoint.connectedAnchor;
					bodyJoint.distance = bodyJoint.distance + tailLine.MainCollider.size.y;

					tailLine.Joint(bgnPos, endPos);

					Destroy(lineJoint);
					lineJoint = tailLine.gameObject.AddComponent<HingeJoint2D>();
					lineJoint.anchor = new Vector2(0, -vec.magnitude * 0.5f);
					lineJoint.enabled = false;					
				}				
			}
		}
	}

	[SerializeField] PlayerChainAnchor prefabChainAnchor; 
	PlayerChainAnchor chainAnchor = null;

	public void Shoot(Vector2 impulse) {
		if(chainAnchor != null) 
			return;

		chainAnchor = Instantiate(prefabChainAnchor, transform.position, Quaternion.identity) as PlayerChainAnchor;
		chainAnchor.rigidbody2D.AddForce(impulse, ForceMode2D.Impulse);
		Joint(transform.position, 10);
	}

	public void Joint(Vector3 jointPos_) {
		Joint(jointPos_, -1);
	}

	public void Joint(Vector3 jointPos_, float distance) {

		if(lineUseNum < lines.Length) {
			jointPos = jointPos_;
			totalDist = Vector3.Distance(jointPos, transform.position);
			if(distance < 0)
				distance = totalDist;

			bodyJoint.enabled = true;
			bodyJoint.distance = distance;
			bodyJoint.connectedAnchor = jointPos;

			// 先端と接点を繋ぐ
			lines[lineUseNum].Joint(jointPos, transform.position);

			// 末端と本体を繋ぐ
			Destroy(lineJoint);
			lineJoint = lines[lineUseNum].gameObject.AddComponent<HingeJoint2D>();
			lineJoint.connectedAnchor = transform.position;
			lineJoint.anchor = new Vector2(0, -totalDist * 0.5f);
			lineJoint.enabled = false;						

			lineUseNum++;

		}
	}

	public void Disjoint() {
		if(chainAnchor != null) {
			Destroy(chainAnchor.gameObject);
			chainAnchor = null;
		}
		bodyJoint.enabled = false;
		Destroy(lineJoint);
		lineJoint = null;
		lineUseNum = 0;
		foreach(PlayerLine line in lines)
			line.Disjoint();
	}

	// つながっているか？
	public bool IsJointed() {
		return bodyJoint.enabled;
	}

	public bool IsTension() {
		if(IsJointed()) {
			float length = GetJointVector().magnitude;
			return (bodyJoint.distance - length < tensionDist);
		}
		return false;
	}

	public Vector2 GetJointVector() {
		Vector2 pos = transform.position;
		return bodyJoint.connectedAnchor - pos;
	}

	public void ControlLength(Vector2 inputPos) {
		Vector2 pos = transform.position;
		Vector2 jointVec = bodyJoint.connectedAnchor - pos;
		Vector2 inputVec = inputPos - pos;

		float dot = Vector2.Dot(jointVec, inputVec);

		Debug.DrawRay(pos, jointVec, Color.yellow);
		Debug.DrawRay(pos, inputVec, Color.magenta);

		if(dot < 0)
			bodyJoint.distance += 10 * Time.deltaTime;
		else bodyJoint.distance -= 10 * Time.deltaTime;

		lines[lineUseNum - 1].CreateChain(transform.position);
/*
		if(joint.distance < 0.5f && blockNum > 0)
			root = blocks[blockNum-1].mainJoint.connectedAnchor;
		Vector2 playerToRoot = root - inputPos;	// 接続位置への角度
		
		Debug.DrawLine(transform.position, posWorldMoved, Color.red);
		float distance = Vector2.Distance(posWorldMoved, root);
*/
	}
}
