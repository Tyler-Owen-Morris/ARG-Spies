using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MapLevelManager : MonoBehaviour {

	public Text score_text;


	void Start () {
		SetScoreText();
	}

	void SetScoreText () {
		score_text.text = GameManager.instance.intel+" bytes";
	}

}
