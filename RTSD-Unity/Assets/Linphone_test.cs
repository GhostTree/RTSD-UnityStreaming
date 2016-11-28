using UnityEngine;
using System.Collections;

public class Linphone_test : MonoBehaviour {
	public WWW wwwData;
	public string url = "sip:linphone@10.20.207.204";
	public GUITexture gt;
	void Start() {
		wwwData = new WWW(url);
		gt = GetComponent<GUITexture>();
		gt.texture = wwwData.movie;
	}
	void Update() {
		MovieTexture m = gt.texture as MovieTexture;
		if (!m.isPlaying && m.isReadyToPlay)
			m.Play();

	}
}

