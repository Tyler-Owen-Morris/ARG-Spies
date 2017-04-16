using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using LitJson;
using System;

public class GameManager : MonoBehaviour {

	public static GameManager instance;
    public static string serverURL = "http://spygame.argzombie.com";

    //core game data
    public string userID, userName, mob_loginTime;
	public int t1_total, t2_total, t3_total, t1_multiplier, t2_multiplier, t3_multiplier;
	public long intel=0;//initilize intel to 0 every game
	public DateTime last_intel_ts, lastLoginTime;

	//

    //URL's for the game 
    private string placeT1BugURL = serverURL + "/PlaceT1Bug.php";
    private string hackWifiURL = serverURL + "/HackWifi.php";
    private string deployBotnetURL = serverURL + "/DeployBotnet.php";
    private string loadAllGameDataURL = serverURL + "/LoadAllData.php";
    private string downloadBuildingURL = serverURL + "/DownloadIntel.php";
    private string d1UpgradeURL = GameManager.serverURL + "/DownloadUpgrade1.php";
    private string d2UpgradeURL = GameManager.serverURL + "/DownloadUpgrade2.php";
    private string d3UpgradeURL = GameManager.serverURL + "/DownloadUpgrade3.php";

	//strings for JSON text of game data.
	public string player_json, bugged_locations_json, google_places_json;

	void Awake () {
		MakeSingleton();
	}

	void Start () {
		//launch location services.
		StartCoroutine(StartLocationServices());
	}

	void MakeSingleton () {
		if (instance != null) {
			Destroy( this );
		}else{
			GameManager.instance = this;
			DontDestroyOnLoad(this);
		}
	}

    public WWWForm SetUpLoginCredentials() {
        WWWForm frm = new WWWForm();
        frm.AddField("id", GameManager.instance.userID);
        //frm.AddField("login_ts", GameManager.instance.lastLoginTime.ToString());
        frm.AddField("login_ts", GameManager.instance.mob_loginTime);
        frm.AddField("client", "mob");
        frm.AddField("username", GameManager.instance.userName);
        return frm;
    }

    public void LoginToGameServer() {
        //lastLoginTime = DateTime.Parse("12/31/1999 11:59:59");//this should be the only time a fresh login is created using our magic timecode
        GameManager.instance.mob_loginTime = "12/31/1999 11:59:59";
        StartCoroutine(LoadAllGameData());
    }

    IEnumerator LoadAllGameData()
    {
        WWWForm form = SetUpLoginCredentials();

        WWW www = new WWW(loadAllGameDataURL, form);
        yield return www;

        Debug.Log(www.text);
        if (www.error == null)
        {
            JsonData allGameReturnJson = JsonMapper.ToObject(www.text);
            if (allGameReturnJson[0].ToString() == "Success")
            {
                //load the json objects onto their text holders
                GameManager.instance.player_json = JsonMapper.ToJson(allGameReturnJson[1]);
                if (allGameReturnJson[2] != null)
                {
                    GameManager.instance.bugged_locations_json = JsonMapper.ToJson(allGameReturnJson[2]);
                }
                else
                {
                    GameManager.instance.bugged_locations_json = "0";
                }
                Debug.Log("player_json: " + GameManager.instance.player_json);
                Debug.Log("location_json: " + GameManager.instance.bugged_locations_json);

                //load GameManager.instance data
                GameManager.instance.intel = (long)allGameReturnJson[1]["intel"];
                instance.CountBugTotals();

                //Load into the game.
                SceneManager.LoadScene("Map Screen");
            }else
            {
                Debug.LogWarning(allGameReturnJson[1].ToString());
            }
        }else
        {
            Debug.LogError(www.error);
        }
        
    }

    private bool downloadStarted = false;
    public void DownloadIntel (string bldg_id, long datum)
    {
        if (downloadStarted)
        {
            return;
        }else
        {
            downloadStarted = true;
            StartCoroutine(DownloadBuilding(bldg_id, datum));
        }
    }

    IEnumerator DownloadBuilding(string bld_id, long intelz)
    {
        MapLevelManager myMapMgr = FindObjectOfType<MapLevelManager>();
        WWWForm form = SetUpLoginCredentials();
        form.AddField("bldg_id", bld_id);
        form.AddField("data", intelz.ToString());

        WWW www = new WWW(downloadBuildingURL, form);
        yield return www;
        Debug.Log(www.text);

        if (www.error == null)
        {
            JsonData downloadReturnJSON = JsonMapper.ToObject(www.text);
            if (downloadReturnJSON[0].ToString() == "Success")
            {
                //reset the active building data on the currently active building.
                if (myMapMgr != null)
                {
                    myMapMgr.active_bldg_intel = 0;
                    myMapMgr.active_bldg_lastIntel = DateTime.Now;
                    myMapMgr.DownloadUp();
                }

                instance.player_json = JsonMapper.ToJson(downloadReturnJSON[1]);
                instance.bugged_locations_json = JsonMapper.ToJson(downloadReturnJSON[2]);

                //reload the buildings with the new building json data
                LocationSpawner myLocSpwnr = FindObjectOfType<LocationSpawner>();
                if (myLocSpwnr != null)
                {
                    myLocSpwnr.UpdateLocations();
                }

                instance.CountBugTotals();
                instance.intel = (long)downloadReturnJSON[1]["intel"];
                

            }else
            {
                Debug.LogWarning(downloadReturnJSON[1].ToString());
            }
        }
        else
        {
            Debug.LogError(www.error);
        }
        
        downloadStarted = false;//reset the bool
        
    }

    #region Placement Coroutine Region

    //****************************
    //***** Bug Placement ********
    //****************************

    public IEnumerator PlaceT1Bug (int cost)
    {
            MapLevelManager myMapMgr = FindObjectOfType<MapLevelManager>();
            WWWForm form = SetUpLoginCredentials();
            form.AddField("bldg_name", myMapMgr.active_bldg_name);
            form.AddField("bldg_id", myMapMgr.active_bldg_id);
            form.AddField("cost", myMapMgr.active_bldg_t1_cost);

            WWW www = new WWW(placeT1BugURL, form);
            yield return www;
            Debug.Log(www.text);

            if (www.error == null)
            {
                JsonData bugPlacementJson = JsonMapper.ToObject(www.text);
                if (bugPlacementJson[0].ToString() == "Success")
                {
                    //load the json objects onto their text holders
                    GameManager.instance.player_json = JsonMapper.ToJson(bugPlacementJson[1]);
                    GameManager.instance.bugged_locations_json = JsonMapper.ToJson(bugPlacementJson[2]);
                    Debug.Log("player_json: " + GameManager.instance.player_json);
                    Debug.Log("location_json: " + GameManager.instance.bugged_locations_json);

                    //reload the buildings with the new building json data
                    LocationSpawner myLocSpwnr = FindObjectOfType<LocationSpawner>();
                    if (myLocSpwnr != null)
                    {
                        myLocSpwnr.UpdateLocations();
                    }

                    instance.CountBugTotals(); //count from json- to update integers on GameManager
                    myMapMgr.active_bldg_t1_count++; //manually increment the active building up.

                    GameManager.instance.t1_total++; //manually increment the gamemanager
                    myMapMgr.UpdateBuildingPanelText();
                    //myMapMgr.UpdateTotalsText();
                    myMapMgr.UpdateBuildingPanelAvailableButtons();

                    //load GameManager.instance data
                    GameManager.instance.intel = (long)bugPlacementJson[1]["intel"];
                }
                else
                {
                    Debug.LogWarning(bugPlacementJson[1].ToString());
                }
            }
            else
            {
                Debug.LogError(www.error);
            }
        //button up resets the slider - resetting the boolean allows another to be sent
        myMapMgr.ResetT1Slider();
        myMapMgr.plantingBug = false;
    }

    public IEnumerator HackWifi (int cost)
    {
        MapLevelManager myMapMgr = FindObjectOfType<MapLevelManager>();
        WWWForm form = SetUpLoginCredentials();
        form.AddField("bldg_name", myMapMgr.active_bldg_name);
        form.AddField("bldg_id", myMapMgr.active_bldg_id);
        form.AddField("cost", cost);

        WWW www = new WWW(hackWifiURL, form);
        yield return www;
        Debug.Log(www.text);

        if (www.error == null)
        {
            JsonData bugPlacementJson = JsonMapper.ToObject(www.text);
            if (bugPlacementJson[0].ToString() == "Success")
            {
                //load the json objects onto their text holders
                GameManager.instance.player_json = JsonMapper.ToJson(bugPlacementJson[1]);
                GameManager.instance.bugged_locations_json = JsonMapper.ToJson(bugPlacementJson[2]);
                Debug.Log("player_json: " + GameManager.instance.player_json);
                Debug.Log("location_json: " + GameManager.instance.bugged_locations_json);

                //reload the buildings with the new building json data
                LocationSpawner myLocSpwnr = FindObjectOfType<LocationSpawner>();
                if (myLocSpwnr != null)
                {
                    myLocSpwnr.UpdateLocations();
                }

                instance.CountBugTotals(); //count from json- to update integers on GameManager
                myMapMgr.active_bldg_t2_count++; //manually increment the active building up.
                GameManager.instance.t2_total++; //manually increment the GameManager
                myMapMgr.UpdateBuildingPanelText();
                //myMapMgr.UpdateTotalsText();
                myMapMgr.UpdateBuildingPanelAvailableButtons();

                //load GameManager.instance data
                GameManager.instance.intel = (long)bugPlacementJson[1]["intel"];
            }
            else
            {
                Debug.LogWarning(bugPlacementJson[1].ToString());
            }
        }
        else
        {
            Debug.LogError(www.error);
        }
        myMapMgr.ResetT2Slider();
        myMapMgr.hackingWifi = false;
    }

    public IEnumerator DeployBotnet (int cost)
    {
        Debug.Log("Deploying Botnet");
        MapLevelManager myMapMgr = FindObjectOfType<MapLevelManager>();
        WWWForm form = SetUpLoginCredentials();
        form.AddField("bldg_name", myMapMgr.active_bldg_name);
        form.AddField("bldg_id", myMapMgr.active_bldg_id);
        form.AddField("cost", cost);

        WWW www = new WWW(deployBotnetURL, form);
        yield return www;
        Debug.Log(www.text);

        if (www.error == null)
        {
            JsonData bugPlacementJson = JsonMapper.ToObject(www.text);
            if (bugPlacementJson[0].ToString() == "Success")
            {
                //load the json objects onto their text holders
                GameManager.instance.player_json = JsonMapper.ToJson(bugPlacementJson[1]);
                GameManager.instance.bugged_locations_json = JsonMapper.ToJson(bugPlacementJson[2]);
                Debug.Log("player_json: " + GameManager.instance.player_json);
                Debug.Log("location_json: " + GameManager.instance.bugged_locations_json);

                //reload the buildings with the new building json data
                LocationSpawner myLocSpwnr = FindObjectOfType<LocationSpawner>();
                if (myLocSpwnr != null)
                {
                    myLocSpwnr.UpdateLocations();
                }

                instance.CountBugTotals(); //count from json- to update integers on GameManager
                myMapMgr.active_bldg_t3_count++; //manually increment the active building up on the game manager
                GameManager.instance.t3_total++;
                myMapMgr.UpdateBuildingPanelText(); //update the panel UI
                //myMapMgr.UpdateTotalsText();
                myMapMgr.UpdateBuildingPanelAvailableButtons();

                //load GameManager.instance data
                GameManager.instance.intel = (long)bugPlacementJson[1]["intel"];
            }
            else
            {
                Debug.LogWarning(bugPlacementJson[1].ToString());
            }
        }
        else
        {
            Debug.LogError(www.error);
        }
        myMapMgr.ResetT3Slider();
        myMapMgr.deployingBotnet = false;
    }

    //******************************
    //** Download Upgrades *********
    //******************************

    public IEnumerator Upgrade_D1 (long cost) {
    	//active bldg data is stored on MapLvlMgr:
    	MapLevelManager myMapMgr = FindObjectOfType<MapLevelManager>();
    	myMapMgr.d1_upgrading=true; //disable more coroutines from starting.
    	Debug.Log("Upgrading the Downloader lvl 1 for: "+ myMapMgr.active_bldg_name);

    	//build the web form
    	WWWForm form = SetUpLoginCredentials();
    	form.AddField("bldg_name", myMapMgr.active_bldg_name);
    	form.AddField("bldg_id", myMapMgr.active_bldg_id);
    	form.AddField("cost", cost.ToString());

    	//make server call
    	WWW www = new WWW(d1UpgradeURL, form);
    	yield return www;

    	if (www.error ==null) {
    		Debug.Log(www.text);
    		JsonData d1UpgradeJSON = JsonMapper.ToObject(www.text);
    		if (d1UpgradeJSON[0].ToString() == "Success"){
    			myMapMgr.active_location.d1=true;
    			myMapMgr.active_bldg_d1=true;
    			myMapMgr.UpdateBuildingPanelAvailableButtons();
    			Debug.Log("Don't forget to call the function on the map manager that updates the building panel here <<<<<=========---------*******************");

    			//update GameManager Json
				GameManager.instance.player_json = JsonMapper.ToJson(d1UpgradeJSON[1]);
                GameManager.instance.bugged_locations_json = JsonMapper.ToJson(d1UpgradeJSON[2]);
    		}else{
    			Debug.LogWarning(d1UpgradeJSON[1].ToString());
    		}

    	}else{
    		Debug.LogError(www.error);
    	}
    	myMapMgr.d1_upgrading=false;

    	myMapMgr.ResetDownload();
    }

	public IEnumerator Upgrade_D2 (long cost) {
    	//active bldg data is stored on MapLvlMgr:
    	MapLevelManager myMapMgr = FindObjectOfType<MapLevelManager>();
    	myMapMgr.d2_upgrading=true; //disable more coroutines from starting.
    	Debug.Log("Upgrading the Downloader lvl 2 for: "+ myMapMgr.active_bldg_name);

    	//build the web form
    	WWWForm form = SetUpLoginCredentials();
    	form.AddField("bldg_name", myMapMgr.active_bldg_name);
    	form.AddField("bldg_id", myMapMgr.active_bldg_id);
    	form.AddField("cost", cost.ToString());

    	//make server call
    	WWW www = new WWW(d2UpgradeURL, form);
    	yield return www;

    	if (www.error ==null) {
			Debug.Log(www.text);
    		JsonData d2UpgradeJSON = JsonMapper.ToObject(www.text);
    		if (d2UpgradeJSON[0].ToString() == "Success"){
    			myMapMgr.active_location.d2=true;
    			myMapMgr.active_bldg_d2=true;
    			myMapMgr.UpdateBuildingPanelAvailableButtons();
    			Debug.Log("Don't forget to call the function on the map manager that updates the building panel here <<<<<=========---------*******************");

    			//update GameManager Json
				GameManager.instance.player_json = JsonMapper.ToJson(d2UpgradeJSON[1]);
                GameManager.instance.bugged_locations_json = JsonMapper.ToJson(d2UpgradeJSON[2]);
    		}else{
    			Debug.LogWarning(d2UpgradeJSON[1].ToString());
    		}
    	}else{
    		Debug.LogError(www.error);
    	}
    	myMapMgr.d2_upgrading=false;
    	myMapMgr.ResetDownload();
    }

	public IEnumerator Upgrade_D3 (long cost) {
    	//active bldg data is stored on MapLvlMgr:
    	MapLevelManager myMapMgr = FindObjectOfType<MapLevelManager>();
    	myMapMgr.d3_upgrading=true; //disable more coroutines from starting.
    	Debug.Log("Upgrading the Downloader lvl 3 for: "+ myMapMgr.active_bldg_name);

    	//build the web form
    	WWWForm form = SetUpLoginCredentials();
    	form.AddField("bldg_name", myMapMgr.active_bldg_name);
    	form.AddField("bldg_id", myMapMgr.active_bldg_id);
    	form.AddField("cost", cost.ToString());

    	//make server call
    	WWW www = new WWW(d3UpgradeURL, form);
    	yield return www;

    	if (www.error ==null) {
			Debug.Log(www.text);
    		JsonData d3UpgradeJSON = JsonMapper.ToObject(www.text);
    		if (d3UpgradeJSON[0].ToString() == "Success"){
    			myMapMgr.active_location.d3=true;
    			myMapMgr.active_bldg_d3=true;
    			myMapMgr.UpdateBuildingPanelAvailableButtons();
    			Debug.Log("Don't forget to call the function on the map manager that updates the building panel here <<<<<=========---------*******************");

    			//update GameManager Json
				GameManager.instance.player_json = JsonMapper.ToJson(d3UpgradeJSON[1]);
                GameManager.instance.bugged_locations_json = JsonMapper.ToJson(d3UpgradeJSON[2]);
    		}else{
    			Debug.LogWarning(d3UpgradeJSON[1].ToString());
    		}
    	}else{
    		Debug.LogError(www.error);
    	}
    	myMapMgr.d3_upgrading=false;
    	myMapMgr.ResetDownload();
    }

    #endregion

    //count up the number of bugs -> calculate multipliers for each type
    public void CountBugTotals ()
    {
    	//verify that the player has data stored on the json_string
        if (GameManager.instance.bugged_locations_json != null && instance.bugged_locations_json != "" && instance.bugged_locations_json != "0")
        {
            JsonData bugedJSON = JsonMapper.ToObject(instance.bugged_locations_json);
            int t1 = 0;
            int t2 = 0;
            int t3 = 0;

            //cycle through the json- adding to our integers
            for (int i=0; i < bugedJSON.Count; i++)
            {
                t1 += (int)bugedJSON[i]["t1_bug_count"];
                t2 += (int)bugedJSON[i]["t2_bug_count"];
                t3 += (int)bugedJSON[i]["t3_bug_count"];
            }
            //set the data on the game manager
            instance.t1_total = t1;
            instance.t2_total = t2;
            instance.t3_total = t3;

            Debug.Log("T1 bugs: " + instance.t1_total + " T2 bugs: " + instance.t2_total + " T3 bugs: " + instance.t3_total);

        }else
        {
        	//this player has not placed any bugs
            instance.t1_total = 0;
            instance.t2_total = 0;
            instance.t3_total = 0;
        }

        //calculate multipliers
        //t1 multiplier
        if(t1_total>=25) {
        	t1_multiplier = 3;
        	if (t1_total >= 50){
        		t1_multiplier=t1_multiplier * 2;
        		if(t1_total >= 100) {
        			t1_multiplier = t1_multiplier * 10;
        		}
        	}
        }else{
        	t1_multiplier=1;
        }

        //t2 multiplier
        if (t2_total>=25){
        	t2_multiplier = 5;
        	if(t2_total>=50){
        		t2_multiplier = t2_multiplier * 10;
        		if(t2_total>=100){
        			t2_multiplier = t2_multiplier * 10;
        		}
        	}
        }else{
        	t2_multiplier=1;
        }

        //t3 multiplier
        if (t3_total>=25){
        	t3_multiplier=4;
        	if (t3_total>=50){
        		t3_multiplier = t3_multiplier * 8;
        		if(t3_total>=100){
        			t3_multiplier = t3_multiplier *8;
        		}
        	}
        }else{
        	t3_multiplier=1;
        }
    }

    //this coroutine is called from Start to initilize the location services.
	IEnumerator StartLocationServices () {
		if (!Input.location.isEnabledByUser){
			Debug.Log ("location services not enabled by user");
            yield break;
        }

		Input.location.Start(10f, 10f);

		//wait until Service initializes, or 20 seconds.
		int maxWait = 20;
		while (Input.location.status ==  LocationServiceStatus.Initializing && maxWait > 0) {
			yield return new WaitForSeconds(1);
			maxWait--;
		}

		// Service did not initialize within 20 seconds
		if (maxWait < 1) {
			print ("Location initialization timed out");
			yield break;
		} 

		// connection failed to initialize
		if (Input.location.status == LocationServiceStatus.Failed) {
			print ("Unable to determine location");
			yield break;
		} else if (Input.location.status == LocationServiceStatus.Running) {
			//access granted and location values can be retireved
			Debug.Log ("Location Services report running successfully");
			yield return Input.location.lastData;
			print ("location is: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude);
		}

	}

}
