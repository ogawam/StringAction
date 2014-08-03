using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerUnit : MonoBehaviour {
	static private PlayerUnit instance;
	static public PlayerUnit Get() { return instance; }

	[SerializeField] float tapSec;
	[SerializeField] float flickSec;
	[SerializeField] float flickDist;
	[SerializeField] float forcePower;
	[SerializeField] float weightPower;
	[SerializeField] float movePower;

	[SerializeField] ChainTest prefabChain;
	[SerializeField] ChainTest prefabBlock;

	int blockNum;
	int waitForJointFrames;

	[SerializeField] float holdPlayerRange;
	[SerializeField] float holdPlayerExpand;

	[SerializeField] GameObject sea;
	[SerializeField] float stringLength;
	[SerializeField] float maxLength;

	[SerializeField] float velScl;
	[SerializeField] float velMin;

	GameObject blockStacks;
	HingeJoint2D blockTailJoint;
	
	[SerializeField] float minZoom = 6;
	[SerializeField] float maxZoom = 12;
	[SerializeField] float distToZoom = 480;

	List<ChainTest> blocks = new List<ChainTest>();
//	DistanceJoint2D joint;

	Vector3 rootJointPos;
	Vector3 tailJointPos;

	Vector3 contactPos;
	Vector2 contactVec;

	PlayerLineManager[] lineManagers;

	float holdCount;

	void Awake () {
		lineManagers = GetComponents<PlayerLineManager>();

		blockStacks = new GameObject();
		blockStacks.name = "blockStacks";
		instance = this;
	}

	// Use this for initialization
	void Start () {
/*
		int layerIndex = LayerMask.NameToLayer("Ignore Raycast");
		Physics2D.IgnoreLayerCollision(layerIndex, layerIndex, true);

		joint = gameObject.AddComponent<DistanceJoint2D>();
		joint.enabled = false;
		joint.distance = stringLength;
		joint.maxDistanceOnly = true;

		for(int i = 0; i < 10; ++i) {
			ChainTest block = Instantiate(prefabBlock) as ChainTest;
			block.transform.parent = blockStacks.transform;
			block.gameObject.SetActive(false);
			blocks.Add(block);
		}
*/
	}

	Vector2 posInputBegan = Vector2.zero;
	Vector2 posInputEnded = Vector2.zero;
	Vector2 posWorldBegan = Vector2.zero;
	Vector2 posWorldEnded = Vector2.zero;
	float pressCount = 0;

	Vector3[] blockPos = new Vector3[2];

	// Update is called once per frame
	void Update () {
		bool isTapped = false;
		bool isFlicked = false;
		bool isInputed = false;
		bool isReset = false;
		TouchPhase phase = TouchPhase.Canceled;

		Vector2 inputPosition = Vector2.zero;

		int inputNum = 0;

#if UNITY_EDITOR
		if(true) {
#else
		if(false) {
#endif
			if(Input.GetKeyDown(KeyCode.Escape)) {
				isReset = true;
			}
			else if(Input.GetMouseButtonDown(0)) {
				isInputed = true;
				phase = TouchPhase.Began;
				inputPosition = Input.mousePosition;
			}
			else if(Input.GetMouseButton(0)) {
				isInputed = true;
				phase = TouchPhase.Moved;
				inputPosition = Input.mousePosition;			
			}
			else if(Input.GetMouseButtonUp(0)) {
				isInputed = true;
				phase = TouchPhase.Ended;
				inputPosition = Input.mousePosition;			
			}
			if(isInputed) {
				inputNum++;
				if(Input.GetKey(KeyCode.LeftShift))
					inputNum++;
			}
		}
		else {
			if(Input.touchCount > 0) {
				inputNum = Input.touchCount;
				foreach(Touch touch in Input.touches) {
					switch(touch.phase) {
					case TouchPhase.Began:
						if(touch.fingerId == 2)
							isReset = true;
						isInputed = true;
						phase = TouchPhase.Began;
						inputPosition = touch.position;			
						break;

					case TouchPhase.Moved:
					case TouchPhase.Stationary:
						isInputed = true;
						phase = TouchPhase.Moved;
						inputPosition = touch.position;			
						break;
					
					case TouchPhase.Ended:
						isInputed = true;
						phase = TouchPhase.Ended;
						inputPosition = touch.position;			
						break;
					}
				}
			}
		}

		if(isReset) {
			transform.position = Vector3.zero;
			rigidbody2D.velocity = Vector2.zero;
		}

		if(isInputed) {
			switch(phase) {
			case TouchPhase.Began:
				posInputBegan = inputPosition;
				posWorldBegan = Camera.main.ScreenToWorldPoint(posInputBegan);
				pressCount = 0;			
				holdCount = 0;
				break;
			case TouchPhase.Moved:
				if(inputNum > 1) {
/*
					if(joint.enabled) {
						Vector2 posWorldMoved = Camera.main.ScreenToWorldPoint(inputPosition);
						Vector2 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
//							Vector2 movedToBegin = inputPosition - posInputBegan;	// 入力始点への角度
						Vector2 pos2D = transform.position;
						Vector2 root = blocks[blockNum].mainJoint.connectedAnchor;
						if(joint.distance < 0.5f && blockNum > 0)
							root = blocks[blockNum-1].mainJoint.connectedAnchor;
						Vector2 playerToRoot = root - pos2D;	// 接続位置への角度
						
						Debug.DrawLine(transform.position, posWorldMoved, Color.red);
						if(Vector3.Distance(screenPosition, inputPosition) > holdPlayerExpand) 
						{
							float distance = Vector2.Distance(posWorldMoved, root);
							Debug.DrawRay(root, Vector3.up, Color.blue);
							SetChainLength(distance);
						}
					}
*/
				}
				else if(isStand) {
					Vector2 posWorldMoved = Camera.main.ScreenToWorldPoint(inputPosition);
					rigidbody2D.AddForce(Vector2.right * (posWorldMoved.x - transform.position.x) * movePower);
				}
/*
				else if(joint.enabled) {
					Vector2 posWorldMoved = Camera.main.ScreenToWorldPoint(inputPosition);
					if(Vector2.Dot(rigidbody2D.velocity, posWorldMoved - inputPosition) < 22.5f) {
						Vector2 vel = rigidbody2D.velocity;
						float velSpd = vel.magnitude * velScl;
						rigidbody2D.AddForce(vel.normalized * Mathf.Max(velSpd, velMin) * 60 * Time.deltaTime);
					}
				}
*/
				break;
			case TouchPhase.Ended:
				posInputEnded = inputPosition;
				posWorldEnded = Camera.main.ScreenToWorldPoint(posInputEnded);

				if(Vector3.Distance(posInputBegan, posInputEnded) < flickDist)
					isTapped = (pressCount < tapSec);
				else isFlicked = (pressCount < flickSec);
				break;
			}
			Debug.DrawLine(posWorldBegan, posWorldEnded);
		}
/*
		if(joint.enabled) {

			{
				// チェインのデバッグ表示
				for(int i = 0; i <= blockNum; ++i) {
					Vector2 bgnPos = blocks[i].mainJoint.connectedAnchor;
					Vector2 endPos = Vector2.zero;
					if(i < blockNum)
						endPos = blocks[i+1].mainJoint.connectedAnchor;
					else endPos = transform.position;
					Debug.DrawLine(bgnPos, endPos, Color.green);
				}

				// 
				Vector2 pos = transform.position;
				Vector2 vec = blocks[blockNum].mainJoint.connectedAnchor - pos;
				blocks[blockNum].mainJoint.anchor = new Vector2(vec.magnitude * 0.5f, 0);
				(blocks[blockNum].collider2D as BoxCollider2D).size = new Vector2(vec.magnitude, 0.1f);

				// 
				if(blockTailJoint != null) {
					blockTailJoint.anchor = new Vector2(-vec.magnitude * 0.5f, 0);
					blockTailJoint.connectedAnchor = transform.position;
					blockTailJoint.enabled = true;
				}
				if(waitForJointFrames > 0) {
					waitForJointFrames--;
					if(waitForJointFrames == 0)
						blocks[blockNum].collider2D.isTrigger = false;
				}

				if(blockNum >= 1) {
					pos = transform.position;
					vec = blocks[blockNum-1].mainJoint.connectedAnchor - pos;
					RaycastHit2D result = Physics2D.Raycast(pos, vec, vec.magnitude, 1);
					if(result.collider != null) {
						Debug.DrawRay(pos, vec, Color.red);
						Debug.DrawRay(result.point, result.normal * 10, Color.red);
					}
					else {
						Debug.DrawRay(pos, vec, Color.yellow);
						blocks[blockNum].gameObject.SetActive(false);

						blockNum--;
						joint.connectedAnchor = blocks[blockNum].mainJoint.connectedAnchor;
						joint.distance = joint.distance + (blocks[blockNum].collider2D as BoxCollider2D).size.x;

						blocks[blockNum].rigidbody2D.isKinematic = false;
						blocks[blockNum].mainJoint.anchor = new Vector2(vec.magnitude * 0.5f, 0);
						(blocks[blockNum].collider2D as BoxCollider2D).size = new Vector2(vec.magnitude, 0.1f);
						blocks[blockNum].collider2D.isTrigger = true;
						waitForJointFrames = 2;

//						SetChainLength(joint.distance);


						Destroy(blockTailJoint);
						blockTailJoint = blocks[blockNum].gameObject.AddComponent<HingeJoint2D>();
						blockTailJoint.anchor = new Vector2(-vec.magnitude * 0.5f, 0);
						blockTailJoint.enabled = false;					
					}
				}

				if(blocks[blockNum].isContacted && blockNum+1 < blocks.Count) {
					Vector2 bgnPos = blocks[blockNum].mainJoint.connectedAnchor;
					Vector2 endPos = blocks[blockNum].GetContactPos();
					vec = endPos - bgnPos;

					endPos += vec.normalized * 0.2f;
					vec = endPos - bgnPos;

					joint.connectedAnchor = endPos;
					joint.distance = joint.distance - vec.magnitude;
//					SetChainLength(joint.distance);

					blocks[blockNum].isContacted = false;
					blocks[blockNum].mainJoint.anchor = new Vector2(vec.magnitude * 0.5f, 0);
					(blocks[blockNum].collider2D as BoxCollider2D).size = new Vector2(vec.magnitude, 0.1f);
					Destroy(blockTailJoint);
					blockNum++;

					bgnPos = endPos;
					endPos = transform.position;
					vec = endPos - bgnPos;

					blocks[blockNum].transform.position = bgnPos + vec * 0.5f;
					blocks[blockNum].transform.eulerAngles = Vector3.forward * Vector3.Angle(endPos, bgnPos);
					blocks[blockNum].gameObject.SetActive(true);
					blocks[blockNum].mainJoint.connectedAnchor = bgnPos;
					blocks[blockNum].mainJoint.anchor = new Vector2(vec.magnitude * 0.5f, 0);
					(blocks[blockNum].collider2D as BoxCollider2D).size = new Vector2(vec.magnitude, 0.1f);
					blocks[blockNum].transform.position = endPos + vec * 0.5f;
					blocks[blockNum].collider2D.isTrigger = true;
					waitForJointFrames = 2;

					Destroy(blockTailJoint);
					blockTailJoint = blocks[blockNum].gameObject.AddComponent<HingeJoint2D>();
					blockTailJoint.anchor = new Vector2(-vec.magnitude * 0.5f, 0);
					blockTailJoint.enabled = false;					
				}
			}
			Debug.DrawLine(blockPos[0], blockPos[1], Color.blue);
		}
*/

		Debug.DrawRay(transform.position, rigidbody2D.velocity);

		if(isTapped) {
			foreach(PlayerLineManager lineManager in lineManagers)
				lineManager.Disjoint();				
/*
			joint.enabled = false;
			foreach(ChainTest block in blocks) {
				block.Disappear();
			}			
*/
		}
		else if(isFlicked) {
			Vector3 vec = posWorldEnded - posWorldBegan;
			Vector2 vec2D = vec;
/*
			if(joint.enabled) {
				Vector2 pos = transform.position;
				float dot = Vector3.Dot(vec, joint.connectedAnchor - pos);
				if(dot < -22.5f) {
					vec2D = joint.connectedAnchor - pos;
					vec2D = vec2D * 2;
				}
				else {
					vec2D = Vector2.zero;
				}
				rigidbody2D.AddForce(vec2D * forcePower, ForceMode2D.Impulse);

			}
			else 
*/			{
				RaycastHit2D result = Physics2D.Raycast(transform.position, vec, Mathf.Infinity, 1);
				if(result) {
					contactVec = result.normal;
					rootJointPos = result.point + result.normal * 0.2f;
					lineManagers[0].Joint(rootJointPos);
/*					
					contactPos = rootJointPos;

					joint.enabled = true;
					joint.distance = Vector3.Distance(rootJointPos, transform.position);
					joint.connectedAnchor = rootJointPos;
					stringLength = joint.distance;

					vec = rootJointPos - transform.position;
					blocks[0].transform.position = transform.position + vec * 0.5f;
					blocks[0].transform.eulerAngles = Vector3.forward * Vector3.Angle(transform.position, rootJointPos);
					blocks[0].gameObject.SetActive(true);
					blocks[0].mainJoint.connectedAnchor = rootJointPos;
					blocks[0].mainJoint.anchor = new Vector2(vec.magnitude * 0.5f, 0);
					(blocks[0].collider2D as BoxCollider2D).size = new Vector2(vec.magnitude, 0.1f);
					blocks[0].transform.position = transform.position + vec * 0.5f;
					blocks[0].collider2D.isTrigger = true;
					blocks[0].isContacted = false;
					waitForJointFrames = 2;

					blockNum = 0;
					Destroy(blockTailJoint);
					blockTailJoint = blocks[blockNum].gameObject.AddComponent<HingeJoint2D>();
					blockTailJoint.anchor = new Vector2(-vec.magnitude * 0.5f, 0);
					blockTailJoint.enabled = false;						
*/
				}
			}
			isFlicked = false;
		}

		Debug.DrawRay(rootJointPos, contactVec * 1);

		pressCount += Time.deltaTime;

		Vector3 posCam = Camera.main.transform.position;
		Vector3 lookAt = transform.position;
/*
		if(joint.enabled) 
			lookAt += (rootJointPos - lookAt) * 0.5f;
*/
		posCam.x += (lookAt.x - posCam.x) * 0.5f;
		posCam.y += (lookAt.y - posCam.y) * 0.5f;
		Camera.main.transform.position = posCam;

		Vector3 difScreen = Camera.main.WorldToScreenPoint(rootJointPos - transform.position);
		float dist = difScreen.magnitude;
		float rate = Mathf.Min(1, dist / distToZoom);
		Camera.main.orthographicSize = minZoom + (maxZoom - minZoom) * rate;

		sea.transform.position = transform.position;
		sea.transform.localScale = Vector3.one * (1.5f + (2.0f * rate));
	}
/*
	void SetChainLength(float length) {
		length = Mathf.Clamp(length, 0, maxLength);
		Debug.Log("length "+ length);
		float offset = (length - stringLength);
		if(offset < 0) {
			if(joint.distance < -offset) {
				offset += joint.distance;
				blocks[blockNum].gameObject.SetActive(false);

				blockNum--;
				joint.connectedAnchor = blocks[blockNum].mainJoint.connectedAnchor;
				joint.distance = (blocks[blockNum].collider2D as BoxCollider2D).size.x;

				blocks[blockNum].rigidbody2D.isKinematic = false;
				blocks[blockNum].mainJoint.anchor = new Vector2(joint.distance * 0.5f, 0);
				(blocks[blockNum].collider2D as BoxCollider2D).size = new Vector2(joint.distance, 0.1f);
				blocks[blockNum].collider2D.isTrigger = true;
				waitForJointFrames = 2;

				Destroy(blockTailJoint);
				blockTailJoint = blocks[blockNum].gameObject.AddComponent<HingeJoint2D>();
				blockTailJoint.anchor = new Vector2(-joint.distance * 0.5f, 0);
				blockTailJoint.enabled = false;						
			}
		}
		joint.distance = joint.distance + offset;
		stringLength = length;
	}
*/
	bool isStand = false;
	void OnCollisionStay2D(Collision2D colli) {
		isStand = true;
	}

	void OnCollisionExit2D(Collision2D colli) {
		isStand = false;
	}


	void OnGUI() {
/*
		GUILayout.Label("fps "+ (Time.deltaTime * 60 * 60));
		GUILayout.Label("velocity "+ rigidbody2D.velocity.magnitude);
		GUILayout.Label("stringLength "+ stringLength);

		float totalLength = 0;
		GUILayout.Label("joint.distance "+ joint.distance);
		for(int i = 0; i <= blockNum; ++i) {
			float length = (blocks[i].collider2D as BoxCollider2D).size.x;
			GUILayout.Label("block "+ i+ " length "+ length);
			totalLength += length;
		}
		GUILayout.Label("total length "+ totalLength);
*/
//		isDispGizmos ^= GUILayout.Button("disp gizmos", GUILayout.Height(64));
	}

	public bool isDispGizmos = false;
}
