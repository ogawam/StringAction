using UnityEngine;
using System.Collections;

public class DebugConfig : Singleton<DebugConfig> {
	void Awake () {
		instance = this;
	}

	void Update() {

	}

	void OnGUI() {
		GUILayout.Label("fps "+ Time.deltaTime * 60 * 60);
	}
}
