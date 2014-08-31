using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerUnit : MonoBehaviour {
	enum Anim {
		Sit,
		Walk
	};
	Anim anim;
	void Animation(Anim anim_) {
		spriteRenderer.sprite = sprites[(int)anim_];
		anim = anim_;
	}
	[SerializeField] Sprite[] sprites;


	[SerializeField] PlayerChainAnchor prefabChainAnchor; 
	[SerializeField] GameObject prefabBackground;
	[SerializeField] SpriteRenderer prefabSprite; 
	[SerializeField] float tapSec;
	[SerializeField] float flickSec;
	[SerializeField] float flickDist;
	[SerializeField] float forcePower;
	[SerializeField] float weightPower;
	[SerializeField] float movePower;
	[SerializeField] float springPower;

	[SerializeField] float holdPlayerRange;
	[SerializeField] float holdPlayerExpand;

	[SerializeField] float stringLength;
	[SerializeField] float maxLength;

	[SerializeField] float velScl;
	[SerializeField] float velMin;

	[SerializeField] float minZoom = 6;
	[SerializeField] float maxZoom = 12;
	[SerializeField] float distToZoom = 480;

	PlayerChainAnchor chainAnchor;
	SpriteRenderer spriteRenderer;
	GameObject background;

	Vector3 rootJointPos;
	Vector3 tailJointPos;

	PlayerLineManager[] lineManagers;

	void Awake () {
		lineManagers = GetComponents<PlayerLineManager>();
	}

	// Use this for initialization
	void Start () {
		spriteRenderer = Instantiate(prefabSprite) as SpriteRenderer;
		background = Instantiate(prefabBackground) as GameObject;
		chainAnchor = null;
	}

	Vector2 posInputBegan = Vector2.zero;
	Vector2 posInputEnded = Vector2.zero;
	Vector2 posWorldBegan = Vector2.zero;
	Vector2 posWorldEnded = Vector2.zero;
	float pressCount = 0;

	// Update is called once per frame
	void Update () {
		spriteRenderer.transform.position = transform.position + Vector3.down * 0.75f;

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
				break;
			case TouchPhase.Moved:
				if(inputNum > 1) {

					if(lineManagers[0].IsJointed()) {
						Vector2 posWorldMoved = Camera.main.ScreenToWorldPoint(inputPosition);
						Vector2 screenPosition = Camera.main.WorldToScreenPoint(transform.position);

//						Debug.DrawLine(posWorldMoved, transform.position, Color.green);

						float distance = Vector2.Distance(screenPosition, inputPosition);
						if(distance > holdPlayerExpand) {
							lineManagers[0].ControlLength(posWorldMoved);
						}
					}
					break;
				}

				// 接地していれば歩ける
				if(isStand) {
					Vector2 posWorldMoved = Camera.main.ScreenToWorldPoint(inputPosition);
					rigidbody2D.AddForce(Vector2.right * (posWorldMoved.x - transform.position.x) * movePower);
					break;
				}

				// 接続していたらスイングできる
				if(lineManagers[0].IsTension()) {
					Vector3 posWorldMoved = Camera.main.ScreenToWorldPoint(inputPosition);
					Vector3 vec = posWorldMoved - transform.position;
					float dot = Vector2.Dot(rigidbody2D.velocity, vec);

					if(dot > 0 && dot < 90f) {
						Vector2 vel = rigidbody2D.velocity;
						float velSpd = vel.magnitude * velScl;
						vel = vel.normalized * Mathf.Max(velSpd, velMin) * 60 * Time.deltaTime;
						rigidbody2D.AddForce(vel);
						Debug.DrawRay(transform.position, vel, Color.red);
						break;
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

		if(chainAnchor != null) {
			if(chainAnchor.isHit) {
			//	contactVec = result.normal;
				rootJointPos = chainAnchor.transform.position;//result.point + result.normal * 0.2f;
				lineManagers[0].Joint(rootJointPos);				
				chainAnchor.isHit = false;
				Destroy(chainAnchor.gameObject);
			}
		}

		if(isTapped) {
			foreach(PlayerLineManager lineManager in lineManagers)
				lineManager.Disjoint();				
		}
		else if(isFlicked) {
			Vector2 inputVec = posWorldEnded - posWorldBegan;

			// 張った状態でフリックすると接触点へ跳べる
			if(lineManagers[0].IsJointed()) {
				if(lineManagers[0].IsTension()) {
					Vector2 jointVec = lineManagers[0].GetJointVector();
					Vector2 forceVec = Vector2.zero;

					float dot = Vector3.Dot(inputVec, jointVec);
					if(dot < -22.5f) {
						forceVec = jointVec * springPower;
					}
					rigidbody2D.AddForce(forceVec.normalized * forcePower, ForceMode2D.Impulse);
				}
			}
			else {
				lineManagers[0].Shoot(inputVec * 5);
			}
			isFlicked = false;
		}

		pressCount += Time.deltaTime;

		Anim anim_ = Anim.Walk;
		if(isStand) {
			if(rigidbody2D.velocity.magnitude < 2)
				anim_ = Anim.Sit;
		}

		if(anim_ != anim)
			Animation(anim_);

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

		background.transform.position = transform.position;
		background.transform.localScale = Vector3.one * (1 + (0.5f * rate));
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
