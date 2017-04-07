using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MapLevelManager : MonoBehaviour {

    //variable for running the map/game/UI.
    public GameObject t2Panel, t1Button;
    public Slider t1Slider, t2Slider, t3Slider;
    public bool t1_down =false, t2_down=false, t3_down=false;
    public float t1_timer, t1_full_time, t2_timer, t2_full_time, t3_timer, t3_full_time;
    
    public Text t1SliderText, t2SliderText, t3SliderText, activeBuildingT1RateText;

	public Text score_text, buildingNameText;
    public GameObject buildingPanel;

    public string active_bldg_name, active_bldg_id, active_bldg_type;
    public int active_bldg_intel, active_bldg_t1_cost;

    void Start () {
		
	}

    void Update ()
    {
        score_text.text = GameManager.instance.intel+" bytes";

        if (t1_down){
            CountOffT1Slider();
        }else{ResetT1Slider();}

        if (t2_down)
        {
            CountOffT2Slider();
        }
        else { ResetT2Slider(); }

        if (t3_down)
        {
            CountOffT3Slider();
        }
        else { ResetT3Slider(); }
    }

    void CountOffT1Slider()
    {
        t1_timer -= Time.deltaTime;//reduce the timer
        float value_left = t1_timer / t1_full_time;
        float value_banked = 1.0f - value_left;
        if (value_banked >= 1.0f) { value_banked = 1.0f; PlantBug(); }
        t1Slider.value = value_banked;
        t1SliderText.text = Mathf.FloorToInt(value_banked * 100f) + "%";
        Debug.Log("value calc: " + value_left + " || and value bank: " + value_banked);
    }

    void ResetT1Slider()
    {
        t1_timer = t1_full_time;
        t1Slider.value = 0;
        t1SliderText.text = "0%";
        plantingBug = false;
        //Debug.Log("reset t1");
    }

	void CountOffT2Slider()
    {
        t2_timer -= Time.deltaTime;
        float value_left = t2_timer / t2_full_time;
        float value_banked = 1.0f - value_left;
        if (value_banked >= 1.0f) { value_banked = 1.0f; Debug.Log("Make T2 thing here"); }
        t2Slider.value = value_banked;
        t2SliderText.text = Mathf.FloorToInt(value_banked * 100f) +"%";
    }

    void ResetT2Slider ()
    {
        t2_timer = t2_full_time;
        t2Slider.value = 0;
        t2SliderText.text = "0%";

    }

    void CountOffT3Slider ()
    {
        t3_timer -= Time.deltaTime;
        float value_left = t3_timer / t3_full_time;
        float value_banked = 1.0f - value_left;
        if (value_banked >= 1.0f) { value_banked = 1.0f; Debug.Log("make t# thing here"); }
        t3Slider.value = value_banked;
        t3SliderText.text = Mathf.FloorToInt(value_banked * 100f) + "%";
    }

    void ResetT3Slider ()
    {
        t3_timer = t3_full_time;
        t3SliderText.text = "0%";
        t3Slider.value = 0;
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

    public void t1ButtonDown() {
        t1_down = true;
    }

    public void t1ButtonUp()
    {
        t1_down = false;
    }

    public void t2ButtonDown() {
        t2_down = true;
    }

    public void t2ButtonUp()
    {
        t2_down = false;
    }

    public void t3ButtonDown()
    {
        t3_down = true;
    }

    public void t3ButtonUp()
    {
        t3_down = false;
    }

    //this attempts to place a bug at current location
    private bool plantingBug = false;
    public void PlantBug ()
    {
        if (plantingBug)
        {
            return;
        }
        else
        {
            //disable the UI from starting again, until slider is reset and refilled
            plantingBug = true;

            if (GameManager.instance.intel == 0 && GameManager.instance.bugged_locations_json == "0")
            {
                //this is the first plant- it costs nothing
                Debug.Log("Fresh game detected- planting bug for free");
                StartCoroutine(GameManager.instance.PlaceT1Bug(0));
                return; //no need for the remaining checks.
            }

            //check the player can afford it.
        }
    }


}
