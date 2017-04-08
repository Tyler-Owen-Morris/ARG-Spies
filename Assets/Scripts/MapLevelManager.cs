using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class MapLevelManager : MonoBehaviour {

    //variable for running the map/game/UI.
    public GameObject t2Panel, t1Button;
    public Slider t1Slider, t2Slider, t3Slider, downloadSlider;
    public bool t1_down =false, t2_down=false, t3_down=false, download_down=false;
    public float t1_timer, t1_full_time, t2_timer, t2_full_time, t3_timer, t3_full_time, download_timer, download_fulltime;
    
    public Text t1SliderText, t2SliderText, t3SliderText, downloadSliderText  , activeBuildingT1RateText;
    public float t1_rate, t2_rate, t3_rate, t1_baseProductivity, t2_baseProductivity, t3_baseProductivity, t1_calculatedProductivity, t2_calculatedProductivity, t3_calculatedProductivity;
    public int t1_baseCost, t2_baseCost, t3_baseCost;

    public Text score_text, buildingNameText, buildingIntelText, t1_plantedText, t2_plantedText, t3_plantedText, t1_costText, t2_costText, t3_costText;
    public GameObject buildingPanel;

    public string active_bldg_name, active_bldg_id, active_bldg_type;
    public int active_bldg_intel, active_bldg_t1_count, active_bldg_t2_count, active_bldg_t3_count, active_bldg_t1_cost, active_bldg_t2_cost, active_bldg_t3_cost;
    public DateTime active_bldg_lastIntel;

    void Start () {
		
	}

    void Update ()
    {
        score_text.text = ByteStringHandler(GameManager.instance.intel);

        if (t1_down){
            CountOffT1Slider();
        }else{ResetT1Slider();}

        if (t2_down) {
            CountOffT2Slider();
        }
        else { ResetT2Slider(); }

        if (t3_down) {
            CountOffT3Slider();
        }
        else { ResetT3Slider(); }
        if (download_down)
        {
            CountOffDownload();
        }else
        {
            ResetDownload();
        }

        if (buildingPanel.activeInHierarchy)
        {
            UpdateCurrentBuildingIntel();
        }
    }

    public void CalculateCurrentBuildingPrices ()
    {
        float t1_cost = t1_baseCost * Mathf.Pow(t1_rate , GameManager.instance.t1_total);
        float t2_cost = t2_baseCost * Mathf.Pow(t2_rate, GameManager.instance.t2_total);
        float t3_cost = t3_baseCost * Mathf.Pow(t3_rate, GameManager.instance.t3_total);

        active_bldg_t1_cost = Mathf.FloorToInt(t1_cost);
        active_bldg_t2_cost = Mathf.FloorToInt(t2_cost);
        active_bldg_t3_cost = Mathf.FloorToInt(t3_cost);

        t1_costText.text = "price: " + ByteStringHandler(active_bldg_t1_cost );
        t2_costText.text = "price: " + ByteStringHandler(active_bldg_t2_cost);
        t3_costText.text = "price: " + ByteStringHandler(active_bldg_t3_cost );

        Debug.Log("Calculating current costs to be at: t1-" + active_bldg_t1_cost + " T2-" + active_bldg_t2_cost + " T3-" + active_bldg_t3_cost);
    }

    void UpdateCurrentBuildingIntel ()
    {
        int intel = 0;
        DateTime magic_time = DateTime.Parse("12/31/1999 12:59:59");
        if (active_bldg_lastIntel != magic_time)
        {

            TimeSpan sinceLastDownload = DateTime.Now - active_bldg_lastIntel;//determine the timespan

            //count up what's been accumulated since grabbed
            float t1_intel = (float)(sinceLastDownload.TotalSeconds * (t1_baseProductivity * active_bldg_t1_count));
            float t2_intel = (float)(sinceLastDownload.TotalSeconds * (t2_baseProductivity * active_bldg_t2_count));
            float t3_intel = (float)(sinceLastDownload.TotalSeconds * (t3_baseProductivity * active_bldg_t3_count));

            //add all the intel's together
            active_bldg_intel = Mathf.RoundToInt(t1_intel+t2_intel+t3_intel);

            //update the UI text
            buildingIntelText.text = "gathered: " + ByteStringHandler(active_bldg_intel );

        }
        else
        {
            buildingIntelText.text = "gathered: 0 bytes";
        }//else this is an unregistered building an thus has no intel to update leave the 0 default
    }

    string ByteStringHandler (int the_bytes)
    {
        //this function takes in the # of bytes and returns the string with the correct suffix and formatting
        string the_string = "";
        if (the_bytes < 1000)
        {
            //return in just bytes
            the_string += the_bytes.ToString() + " byte";
            if (the_bytes > 1) { the_string += "s"; }
        }else if ( the_bytes < 1000000)
        {
            //return in KB
            string tmp = (the_bytes * .001).ToString();
            int lngth = 5;
            if (tmp.Length < 5) { lngth = tmp.Length; }
            the_string += tmp.Substring(0, lngth) + " KB";
        }else if (the_bytes < 1000000000)
        {
            //return in MB
            string tmp = (the_bytes * .000001).ToString();
            int lngth = 5;
            if (tmp.Length < 5) { lngth = tmp.Length; }
            the_string += tmp.Substring(0, lngth) + " MB";
        }else if (the_bytes < 1000000000000)
        {
            //return in GB
            string tmp = (the_bytes * .000000001).ToString();
            int lngth = 5;
            if (tmp.Length < 5) { lngth = tmp.Length; }
            the_string += tmp.Substring(0, 5) + " GB";
        }else
        {
            //return in TB
            string tmp = (the_bytes * .000000000001).ToString();
            int lnght = 5;
            if (tmp.Length < 5) { lnght = tmp.Length; }
            the_string += tmp.Substring(0, lnght) + " TB";
        }

        return the_string;
    }

    void CountOffT1Slider()
    {
        t1_timer -= Time.deltaTime;//reduce the timer
        float value_left = t1_timer / t1_full_time;
        float value_banked = 1.0f - value_left;
        if (value_banked >= 1.0f)
        {
            value_banked = 1.0f;
            PlantBug();//this is T1 planted item
        }
        t1Slider.value = value_banked;
        t1SliderText.text = Mathf.FloorToInt(value_banked * 100f) + "%";
        //Debug.Log("value calc: " + value_left + " || and value bank: " + value_banked);
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
        if (value_banked >= 1.0f)
        {
            value_banked = 1.0f;
            HackWifi();
            Debug.Log("Make T2 thing here");
        }
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
        if (value_banked >= 1.0f)
        {
            value_banked = 1.0f;
            DeployBotnet();
            Debug.Log("make t3 thing here");
        }
        t3Slider.value = value_banked;
        t3SliderText.text = Mathf.FloorToInt(value_banked * 100f) + "%";
    }

    void ResetT3Slider ()
    {
        t3_timer = t3_full_time;
        t3SliderText.text = "0%";
        t3Slider.value = 0;
    }

    void CountOffDownload()
    {
        download_timer -= Time.deltaTime;
        float value_left = download_timer / download_fulltime;
        float value_banked = 1.0f - value_left;
        if (value_banked >= 1.0f) {
            value_banked = 1.0f;
            if (active_bldg_intel > 0)
            {
                //Debug.Log("CALLING FOR DOWNLOAD OF INTEL: " + active_bldg_intel);
                GameManager.instance.DownloadIntel(active_bldg_id, active_bldg_intel);
            }else
            {
                Debug.Log("NO INTEL TO DOWNLOAD");
            }
        }
        downloadSlider.value = value_banked;
        downloadSliderText.text = Mathf.FloorToInt(value_banked * 100f) + "%";
    }

    void ResetDownload()
    {
        download_timer = download_fulltime;
        downloadSliderText.text = "0%";
        downloadSlider.value = 0;
    }

    public void LoadBuildingPanel (string bldg_name, string bldg_id, string photo_ref, string goog_type, DateTime last_download, int t1, int t2, int t3)
    {
        buildingNameText.text = bldg_name;
        active_bldg_name = bldg_name;
        active_bldg_id = bldg_id;
        active_bldg_type = goog_type;
        active_bldg_lastIntel = last_download;
        active_bldg_t1_count = t1;
        active_bldg_t2_count = t2;
        active_bldg_t3_count = t3;
        active_bldg_intel = 0; //let this be calculated in update

        //update the rate's text
       
        UpdateBuildingPanelText();

        buildingPanel.SetActive(true);
    }

    void CalculateActiveBldgProduction ()
    {
        float t1 = (t1_baseProductivity * active_bldg_t1_count);
        float t2 = (t2_baseProductivity * active_bldg_t2_count);
        float t3 = (t3_baseProductivity * active_bldg_t3_count);

        t1_calculatedProductivity = t1;
        t2_calculatedProductivity = t2;
        t3_calculatedProductivity = t3;
    }

    public void UpdateBuildingPanelText ()
    {
        CalculateActiveBldgProduction();

        if (active_bldg_t1_count > 0)
        {
            string t1_string = "";
            t1_string += ByteStringHandler((int)t1_calculatedProductivity);
            t1_string += "/\nsec";
            t1_plantedText.text = t1_string;

        }else
        {
            t1_plantedText.text = "0";
        }

        if (active_bldg_t2_count > 0)
        {

            string t2_string = "";
            t2_string += ByteStringHandler((int)t2_calculatedProductivity);
            t2_string += "/\nsec";
            t2_plantedText.text = t2_string;

        }else
        {
            t2_plantedText.text = "0";
        }

        if (active_bldg_t3_count > 0)
        {

            string t3_string = "";
            t3_string += ByteStringHandler((int)t3_calculatedProductivity);
            t3_string += "/\nsec";
            t3_plantedText.text = t3_string;

        }else
        {
            t3_plantedText.text = "0";
        }

        CalculateCurrentBuildingPrices();
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

    public void DownloadDown ()
    {
        download_down = true;
    }

    public void DownloadUp()
    {
        download_down = false;
    }

    //this attempts to place a bug at current location
    public bool plantingBug = false;
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
            if (GameManager.instance.intel >= active_bldg_t1_cost)
            {
                Debug.Log("This player CAN buy this T1 Bug!! let them do it");
                StartCoroutine(GameManager.instance.PlaceT1Bug(active_bldg_t1_cost));
            }
            
        }
    }


    public bool hackingWifi = false;
    public void HackWifi ()
    {
        if (hackingWifi)
        {
            return;
        }else
        {
            hackingWifi = true;

            if (GameManager.instance.intel >= active_bldg_t2_cost)
            {
                Debug.Log("Player is HACKING WIFI!!! ");
                StartCoroutine(GameManager.instance.HackWifi(active_bldg_t2_cost));
            }else
            {
                Debug.Log("Player Cannot afford to hack the wifi");
            }
        }
    }

    public bool deployingBotnet = false;
    public void DeployBotnet ()
    {
        if (deployingBotnet)
        {
            return;
        }else
        {
            deployingBotnet = true;

            if (GameManager.instance.intel >= active_bldg_t3_cost)
            {
                Debug.Log("Player is Deploying a BOTNET!!!");
                StartCoroutine(GameManager.instance.DeployBotnet(active_bldg_t3_cost));
            }
            else
            {
                Debug.Log("Player CANNOT afford to Deploy Botnet!");
            }
        }
    }

}
