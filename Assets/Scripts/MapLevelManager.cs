using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MapLevelManager : MonoBehaviour {

	public Text score_text, buildingNameText;
    public GameObject buildingPanel;

    private string active_bldg_name, active_bldg_id, active_bldg_type;

	void Start () {
		SetScoreText();
	}

	void SetScoreText () {
		score_text.text = GameManager.instance.intel+" bytes";
	}

    public void LoadBuildingPanel (string bldg_name, string bldg_id, string photo_ref, string goog_type)
    {
        buildingNameText.text = bldg_name;
        active_bldg_name = bldg_name;
        active_bldg_id = bldg_id;
        active_bldg_type = goog_type;
        buildingPanel.SetActive(true);
    }

    public void CloseBuildingPanel ()
    {
        active_bldg_name = "";
        active_bldg_id = "";
        active_bldg_type = "";
        buildingPanel.SetActive(false);
    }

}
