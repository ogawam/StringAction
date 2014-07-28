using UnityEngine;
using System.Collections;

public class DebugConfig : Singleton<DebugConfig> {
	void Awake () {
		instance = this;
	}
}
