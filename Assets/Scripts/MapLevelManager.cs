using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class MapLevelManager : MonoBehaviour {

    //variables for running the UI interaction.
    public GameObject t2Panel, t1Button, t2Button, t3Button, d1Button, d2Button, d3Button, DL_Button;
    public Slider t1Slider, t2Slider, t3Slider, downloadSlider, capacitySlider;
    public bool t1_down =false, t2_down=false, t3_down=false, download_down=false;
    public float t1_timer, t1_full_time, t2_timer, t2_full_time, t3_timer, t3_full_time, t1_maxTime, t2_maxTime, t3_maxTime, download_timer, download_fulltime, download_maxTime;
    public PopulatedLocation active_location;


    //building panel objects used in Update()
    public Text t1SliderText, t2SliderText, t3SliderText, downloadSliderText  , activeBuildingT1RateText;
    public float t1_rate, t2_rate, t3_rate, t1_baseProductivity, t2_baseProductivity, t3_baseProductivity;
    public long t1_calculatedProductivity, t2_calculatedProductivity, t3_calculatedProductivity, active_bldg_totalProductivity;
    public int t1_baseCost, t2_baseCost, t3_baseCost;

    //building panel objects
    public Text score_text, buildingNameText, buildingIntelText, t1_plantedText, t2_plantedText, t3_plantedText, t1_costText, t2_costText, t3_costText, t1_count_text, t2_count_text, t3_count_text;
    public GameObject buildingPanel;

    //building data
    public string active_bldg_name, active_bldg_id, active_bldg_type;
    public int active_bldg_t1_count, active_bldg_t2_count, active_bldg_t3_count, active_bldg_t1_cost, active_bldg_t2_cost, active_bldg_t3_cost;
    public bool active_bldg_d1, active_bldg_d2, active_bldg_d3;
    public long active_bldg_intel, active_bldg_capacity;
    public DateTime active_bldg_lastIntel;

	private LocationSpawner my_locationSpawner; 

	void Start () {
    	//update the total counts.
		//UpdateTotalsText();
		my_locationSpawner = FindObjectOfType<LocationSpawner>();
		if (my_locationSpawner==null) {
			Debug.LogError("Map Level Manager not locating Location Spawner correctly");
		}

		//this will let me keep it active in the heirarchy while working
		buildingPanel.SetActive(false);
	}

    void Update ()
    {
        score_text.text = ByteStringHandler((long)GameManager.instance.intel);

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
    /*
    public void UpdateTotalsText () {
    	t1_totalsText.text = "Bugs: "+GameManager.instance.t1_total;
    	t2_totalsText.text = "Hax: " + GameManager.instance.t2_total;
    	t3_totalsText.text = "Bot: " + GameManager.instance.t3_total;
    }
    */

    public void CalculateCurrentBuildingPrices ()
    {
        float t1_cost = t1_baseCost * Mathf.Pow(t1_rate , active_bldg_t1_count);
        float t2_cost = t2_baseCost * Mathf.Pow(t2_rate, active_bldg_t2_count);
        float t3_cost = t3_baseCost * Mathf.Pow(t3_rate, active_bldg_t3_count);

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

            //clamp the building to current capacity
			if (active_bldg_intel >= active_bldg_capacity) {
            	active_bldg_intel = active_bldg_capacity;
            }

            //update the UI text
            buildingIntelText.text = "gathered: " + ByteStringHandler(active_bldg_intel );

            //handle activation or deacivation of the "gathered intel button"
            if (active_bldg_intel>0){
            	DL_Button.GetComponent<Button>().interactable=true;
            }else{
            	DL_Button.GetComponent<Button>().interactable = false;
            }

            //update the downloaded slider against the capacity of this building.



            double valu = ((double)active_bldg_intel / active_bldg_capacity);
            //Debug.Log("capcity slider used building intel:"+active_bldg_intel+"  and capacity calculated at: "+active_bldg_capacity+" calculated at: "+ valu);
            capacitySlider.value=(float)valu;
        }
        else
        {
            buildingIntelText.text = "gathered: 0 bytes";
        }//else this is an unregistered building an thus has no intel to update leave the 0 default
    }

    string ByteStringHandler (long the_bytes)
    {
        //this function takes in the # of bytes and returns the string with the correct suffix and formatting
        string the_string = "";
        if (the_bytes < 1000){
            //return in just bytes
            the_string += the_bytes.ToString() + " byte";
            if (the_bytes > 1) { the_string += "s"; }
        }else if ( the_bytes < 1000000){
            //return in KB
            string tmp = (the_bytes * .001).ToString();
            int lngth = 5;
            if (tmp.Length < 5) { lngth = tmp.Length; }
            the_string += tmp.Substring(0, lngth) + " KB";
        }else if (the_bytes < 1000000000){
            //return in MB
            string tmp = (the_bytes * .000001).ToString();
            int lngth = 5;
            if (tmp.Length < 5) { lngth = tmp.Length; }
            the_string += tmp.Substring(0, lngth) + " MB";
        }else if (the_bytes < 1000000000000){
            //return in GB
            string tmp = (the_bytes * .000000001).ToString();
            int lngth = 5;
            if (tmp.Length < 5) { lngth = tmp.Length; }
            the_string += tmp.Substring(0, 5) + " GB";
        }else if (the_bytes < 1000000000000000)  {
        	//return in TB
			string tmp = (the_bytes * .000000000001).ToString();
            int lnght = 5;
            if (tmp.Length < 5) { lnght = tmp.Length; }
            the_string += tmp.Substring(0, lnght) + " TB";
        }else{
            //return in PB
			string tmp = (the_bytes * .000000000000001).ToString();
            int lnght = 5;
            if (tmp.Length < 5) { lnght = tmp.Length; }
            the_string += tmp.Substring(0, lnght) + " PB";
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
        t1SliderText.gameObject.SetActive(true);
        //Debug.Log("value calc: " + value_left + " || and value bank: " + value_banked);
    }

    public void ResetT1Slider()
    {
    	//speed up the slider based on multiplier
    	t1_full_time = t1_maxTime; //initilize the max value
    	if (GameManager.instance.t1_total >= 25) {
    		t1_full_time = t1_full_time *0.5f;// half the time
    		if (GameManager.instance.t1_total >= 50 ){
    			t1_full_time = t1_full_time * 0.5f;
    			if (GameManager.instance.t1_total >= 100) {
    				t1_full_time = t1_full_time*0.5f;
    			}
    		}
    	}
        t1_timer = t1_full_time;
        t1Slider.value = 0;
        t1SliderText.text = "0%";
        plantingBug = false;
        t1SliderText.gameObject.SetActive(false);
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
        t2SliderText.gameObject.SetActive(true);
    }

    public void ResetT2Slider ()
    {
    	//speed up the slider based on multiplier
		t2_full_time = t2_maxTime; //initilize the max value
    	if (GameManager.instance.t2_total >= 25) {
    		t2_full_time = t2_full_time *0.5f;// half the time
    		if (GameManager.instance.t2_total >= 50 ){
    			t2_full_time = t2_full_time * 0.5f;
    			if (GameManager.instance.t2_total >= 100) {
    				t2_full_time = t2_full_time*0.5f;
    			}
    		}
    	}

        t2_timer = t2_full_time;
        t2Slider.value = 0;
        t2SliderText.text = "0%";
        t2SliderText.gameObject.SetActive(false);
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
           // Debug.Log("make t3 thing here");
        }
        t3Slider.value = value_banked;
        t3SliderText.text = Mathf.FloorToInt(value_banked * 100f) + "%";
        t3SliderText.gameObject.SetActive(true);
    }

    public void ResetT3Slider ()
    {
    	//speed up the slider based on multiplier
		t3_full_time = t3_maxTime; //initilize the max value
    	if (GameManager.instance.t3_total >= 25) {
    		t3_full_time = t3_full_time *0.5f;// half the time
    		if (GameManager.instance.t3_total >= 50 ){
    			t3_full_time = t3_full_time * 0.5f;
    			if (GameManager.instance.t3_total >= 100) {
    				t3_full_time = t3_full_time*0.5f;
    			}
    		}
    	}
    	
        t3_timer = t3_full_time;
        t3SliderText.text = "0%";
        t3Slider.value = 0;
        t3SliderText.gameObject.SetActive(false);
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
        downloadSliderText.gameObject.SetActive(true);
    }

    public void ResetDownload()
    {
    	//Debug.Log("RESEtting Downloader");
    	download_fulltime = download_maxTime;
    
    	if(this.active_bldg_d1) {
   			download_fulltime = download_fulltime *0.5f;
   			d1Button.SetActive(false);
   			//Debug.Log("button 1 off");
   		}else{
   			d1Button.SetActive(true);
   		}
    	if(this.active_bldg_d2){
    		download_fulltime=download_fulltime*0.5f;
   			d2Button.gameObject.SetActive(false);
   			//Debug.Log("button 2 off");
   		}else{
   			d2Button.gameObject.SetActive(true);
   			//Debug.Log("button 2 on");
    	}
    	if(active_bldg_d3){
   			download_fulltime=download_fulltime*0.5f;
   			d3Button.SetActive(false);
   			//Debug.Log("button 3 off");
   		}else{
   			d3Button.SetActive(true);
    	}
    	
        download_timer = download_fulltime;
        downloadSliderText.text = "0%";
        downloadSlider.value = 0;
        downloadSliderText.gameObject.SetActive(false);
    }

    public void LoadBuildingPanel (GameObject my_GameObject, string bldg_name, string bldg_id, string photo_ref, string goog_type, DateTime last_download, int t1, int t2, int t3)
    {
    	//save a reference to the populated location gameobject.PopulatedLocation
    	PopulatedLocation thisLocationScript = my_GameObject.GetComponent<PopulatedLocation>();
    	active_location = thisLocationScript;
    	active_bldg_d1=active_location.d1;
    	active_bldg_d2=active_location.d2;
    	active_bldg_d3=active_location.d3;
    	//load all of the other variables.
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
        UpdateBuildingPanelAvailableButtons();
        buildingPanel.SetActive(true);
    }

    void CalculateActiveBldgProduction ()
    {
    	//calculate each building's production
        long t1 = (long)((t1_baseProductivity * active_bldg_t1_count) * (float)GameManager.instance.t1_multiplier);
        long t2 = (long)((t2_baseProductivity * active_bldg_t2_count) * (float)GameManager.instance.t2_multiplier);
        long t3 = (long)((t3_baseProductivity * active_bldg_t3_count) * (float)GameManager.instance.t3_multiplier);

        Debug.Log("T1 production: "+t1+"  T2 production: "+t2+"  T3 production: "+t3);

        //store the producivity on the MapLevelManager
        t1_calculatedProductivity = t1;
        t2_calculatedProductivity = t2;
        t3_calculatedProductivity = t3;
        active_bldg_totalProductivity = t1_calculatedProductivity + t2_calculatedProductivity + t3_calculatedProductivity;

        //calculate this building's capacity
        //for now this is just a function of production*setTime=capacity
        long seconds_inAday = (long)(TimeSpan.FromHours(1.0f)).TotalSeconds;
        long capacity = seconds_inAday * active_bldg_totalProductivity;
        active_bldg_capacity=capacity;
    }

    public void UpdateBuildingPanelAvailableButtons(){
    	//turn off/on the download buttons first
    	if (GameManager.instance.intel < 1000000000000000) { //less than a petabyte
    		d3Button.GetComponent<Button>().interactable = false;
    	}else{
    		//check it's not already purchased for this location
    		if(!active_location.d3){
    			d3Button.GetComponent<Button>().interactable = true;
    		}else{
    			d3Button.GetComponent<Button>().interactable =false;
    		}
    	}
    	if (GameManager.instance.intel < 1000000000000) {
    		d2Button.GetComponent<Button>().interactable = false;
    		d2Button.SetActive(false);
    	}else{
    		if(!active_location.d2){
    			d2Button.GetComponent<Button>().interactable = true;
    			d2Button.SetActive(true);
    		}else{
    			d2Button.GetComponent<Button>().interactable = false;
    			d2Button.SetActive(false);
    		}
    	}
    	if (GameManager.instance.intel < 1000000000) {
    		d1Button.GetComponent<Button>().interactable = false;
    	}else{
    		if(!active_location.d1){
    			d1Button.GetComponent<Button>().interactable = true;
    		}else{
    			d1Button.GetComponent<Button>().interactable = false;
    		}
    	}

    	//turn off the bug buttons
    	if (GameManager.instance.intel < active_bldg_t1_cost) {
    		t1Button.GetComponent<Button>().interactable = false;
    	}else{
    		t1Button.GetComponent<Button>().interactable = true;
    	}
    	if (GameManager.instance.intel < active_bldg_t2_cost) {
    		t2Button.GetComponent<Button>().interactable = false;
    	}else{
    		t2Button.GetComponent<Button>().interactable = true;
    	}
    	if (GameManager.instance.intel < active_bldg_t3_cost){
    		t3Button.GetComponent<Button>().interactable=false;
    	}else{
    		t3Button.GetComponent<Button>().interactable=true;
    	}

    	//handle the download button in the update.
    }

    public void UpdateBuildingPanelText ()
    {
    	//update the current building counts.
    	t1_count_text.text = " Bugs: "+active_bldg_t1_count.ToString()+" ";
    	t2_count_text.text = " Hax: "+active_bldg_t2_count.ToString()+" ";
    	t3_count_text.text = " Bot: "+active_bldg_t3_count.ToString()+" " ;

    	//update the active building production math on the map level manager
        CalculateActiveBldgProduction();

        //update the rate text
        if (active_bldg_t1_count > 0)
        {
            string t1_string = "";
            t1_string += ByteStringHandler((int)t1_calculatedProductivity);
            t1_string += "/\nsec";
            t1_plantedText.text = t1_string;
            Debug.Log(t1_calculatedProductivity+ " : Calculated T1 Producitivy");
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
            Debug.Log(t2_calculatedProductivity+" : Calculated T2 Productivity");
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
            Debug.Log(t3_calculatedProductivity.ToString()+" : Calculated T3 Productivity ******");
        }else
        {
            t3_plantedText.text = "0";
        }

        //calculate current prices, and set the text.
        CalculateCurrentBuildingPrices();

        //ensure the totals are updated- this is used as the callback UI updater, and letting this update as well allows the totals to be updated from the GameManager.instance data
        //UpdateTotalsText();
    }

    public void CloseBuildingPanel ()
    {
        active_bldg_name = "";
        active_bldg_id = "";
        active_bldg_type = "";
        buildingPanel.SetActive(false);


        if (my_locationSpawner!= null) {
        	my_locationSpawner.UpdateLocations();
        }else{
        	my_locationSpawner = FindObjectOfType<LocationSpawner>();
        	if (my_locationSpawner!=null){
        		my_locationSpawner.UpdateLocations();
        	}else{
        		Debug.LogError("Unable to loacte the loaded Location spawner.");
        	}
        }
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

    #region Public functions for gear placement at locations

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


    public bool d1_upgrading = false;
    public void D1_Upgrade () {
    	if (d1_upgrading) {
    		return;
    	} else {
    		//check the player can afford
			long cost = 1000000000;//1,000,000bytes=1GB
    		if (GameManager.instance.intel >= cost){ 
    			GameManager.instance.intel = GameManager.instance.intel-cost;
    			Debug.Log("subtracting "+cost+" from GameManager.instance.intel: "+GameManager.instance.intel);
    			StartCoroutine(GameManager.instance.Upgrade_D1(cost));
    		}else{
    			d1_upgrading=false;
    			return;
    		}
    	}

    }

    public bool d2_upgrading = false;
    public void D2_Upgrade () {
    	if (d2_upgrading) {
    		return;
    	}else{
			long cost = 1000000000000;//==1TB
    		if (GameManager.instance.intel >= cost) { 
				GameManager.instance.intel = GameManager.instance.intel-cost;
				Debug.Log("subtracting "+cost+" from GameManager.instance.intel: "+GameManager.instance.intel);
    			StartCoroutine(GameManager.instance.Upgrade_D2(cost));
    		}else{
    			d2_upgrading=false;
    			return;
    		}
    	}
    }

    public bool d3_upgrading = false;
    public void D3_Upgrade () {
    	if (d3_upgrading){
    		return;
    	}else{
			long cost = 1000000000000000;
    		if (GameManager.instance.intel >= cost) { //==1PB
				GameManager.instance.intel = GameManager.instance.intel-cost;
				Debug.Log("subtracting "+cost+" from GameManager.instance.intel: "+GameManager.instance.intel);
    			StartCoroutine(GameManager.instance.Upgrade_D3(cost));
    		}else{
    			d3_upgrading=false;
    			return;
    		}
    	}
    }


    #endregion
}
