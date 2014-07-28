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
	
	}

	public void Joint(Vector3 jointPos_) {

		// 既にジョイントしている場合は除外する
		if(lineUseNum > 0)
			return;

		jointPos = jointPos_;
		totalDist = Vector3.Distance(jointPos, transform.position);

		bodyJoint.enabled = true;
		bodyJoint.distance = totalDist;
		bodyJoint.connectedAnchor = jointPos;

		lines[lineUseNum].Joint(jointPos, transform.position);

		Destroy(lineJoint);
		lineJoint = lines[lineUseNum].gameObject.AddComponent<HingeJoint2D>();
		lineJoint.anchor = new Vector2(-totalDist * 0.5f, 0);
		lineJoint.enabled = false;						
		lineUseNum++;
	}

}
