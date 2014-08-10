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
	[SerializeField] float springPower;

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
				// 接地していれば歩ける
				else if(isStand) {
					Vector2 posWorldMoved = Camera.main.ScreenToWorldPoint(inputPosition);
					rigidbody2D.AddForce(Vector2.right * (posWorldMoved.x - transform.position.x) * movePower);
				}
				// 接続していたらスイングできる
				else if(lineManagers[0].IsJointed()) {
					Vector3 posWorldMoved = Camera.main.ScreenToWorldPoint(inputPosition);
					Vector3 vec = posWorldMoved - transform.position;
					float dot = Vector2.Dot(rigidbody2D.velocity, vec);
					Debug.Log("dot "+ dot);
					if(dot > 0 && dot < 90f) {
						Vector2 vel = rigidbody2D.velocity;
						float velSpd = vel.magnitude * velScl;
						vel = vel.normalized * Mathf.Max(velSpd, velMin) * 60 * Time.deltaTime;
						rigidbody2D.AddForce(vel);
						Debug.DrawRay(transform.position, vel, Color.red);
					}
				}
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

		if(isTapped) {
			foreach(PlayerLineManager lineManager in lineManagers)
				lineManager.Disjoint();				
		}
		else if(isFlicked) {
			Vector2 inputVec = posWorldEnded - posWorldBegan;

			// 張った状態でフリックすると接触点へ跳べる
			if(lineManagers[0].IsJointed()) {
				Vector2 jointVec = lineManagers[0].GetJointVector();
				Vector2 forceVec = Vector2.zero;

				float dot = Vector3.Dot(inputVec, jointVec);
				if(dot < -22.5f) {
					forceVec = jointVec * springPower;
				}
				rigidbody2D.AddForce(forceVec * forcePower, ForceMode2D.Impulse);
			}
			else 
			{
				RaycastHit2D result = Physics2D.Raycast(transform.position, inputVec, Mathf.Infinity, 1);
				if(result) {
					contactVec = result.normal;
					rootJointPos = result.point + result.normal * 0.2f;
					lineManagers[0].Joint(rootJointPos);
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

	bool isStand = false;
	void OnCollisionStay2D(Collision2D colli) {
		isStand = true;
	}

	void OnCollisionExit2D(Collision2D colli) {
		isStand = false;
	}

	void OnGUI() {}
}
