using UnityEngine;
using System.Collections;

public class BoxCollisionTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	public Vector2 pos;
	public Vector2 size;
	public Vector2 direction;
	public float distance = Mathf.Infinity;
	public float angle;

	public GameObject box;
	public Vector2 boxPos;

	// Update is called once per frame
	void Update () {
		RaycastHit2D result = Physics2D.BoxCast(pos, size, angle, direction, distance);
		if(result != null) {
			Debug.DrawRay(result.point, result.normal);
		}	

		if(Input.anyKeyDown) {
			StartCoroutine(CreateBox());
		}
	}

	IEnumerator CreateBox() {
		GameObject go = Instantiate(box) as GameObject;
		go.SetActive(false);
		yield return 0;

		go.transform.position = boxPos;
		go.SetActive(true);
	}

	void OnDrawGizmos() {
		Quaternion rotation = Quaternion.identity;
		rotation.eulerAngles = Vector3.forward * angle;

		Vector2 view = new Vector2(Screen.width, Screen.height);
		float offset = Mathf.Min(distance, view.magnitude);
		Vector2 vec = direction.normalized * offset;

		Vector2[] edges = new Vector2[4];
		for(int i = 0; i < 4; ++i) {
			edges[i] = rotation * new Vector2(((i/2 > 0) ? size.x : -size.x) * 0.5f
											, (((i+1)%4/2 > 0) ? size.y : -size.y) * 0.5f);
		}

		Gizmos.color = Color.white;
		for(int i = 0; i < 4; ++i) {
			Gizmos.DrawLine(edges[i], edges[(i+1)%4]);
			Gizmos.DrawLine(edges[i] + vec, edges[(i+1)%4] + vec);
			Gizmos.DrawLine(edges[i], edges[i] + vec);
		}
	}
}
