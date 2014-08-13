using UnityEngine;
using System.Collections;

public class PlayerLinePart : MonoBehaviour {

	[SerializeField] float rotCycle;
	[SerializeField] float sclCycle;

	float frame = 0;

	// Use this for initialization
	void Start () {
		frame = Random.value;
	}
	
	// Update is called once per frame
	void Update () {
		transform.localScale = Vector3.one * (1 + 0.25f * Mathf.Sin(frame /sclCycle));
//		transform.localEulerAngles = Vector3.forward * 360 * (frame / rotCycle);
		frame += Time.deltaTime;
	}
}
