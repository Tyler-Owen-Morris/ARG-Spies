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
	public int intel=0, t1_total, t2_total, t3_total;
	public DateTime last_intel_ts, lastLoginTime;

    //URL's for the game 
    private string placeT1BugURL = serverURL + "/PlaceT1Bug.php";
    private string hackWifiURL = serverURL + "/HackWifi.php";
    private string deployBotnetURL = serverURL + "/DeployBotnet.php";
    private string loadAllGameDataURL = serverURL + "/LoadAllData.php";
    private string downloadBuildingURL = serverURL + "/DownloadIntel.php";

	//strings for JSON text of game data.
	public string player_json, bugged_locations_json, google_places_json;

	void Awake () {
		MakeSingleton();
	}

	void Start () {
		
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
                GameManager.instance.intel = (int)allGameReturnJson[1]["intel"];
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
    public void DownloadIntel (string bldg_id, int datum)
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

    IEnumerator DownloadBuilding(string bld_id, int intelz)
    {
        MapLevelManager myMapMgr = FindObjectOfType<MapLevelManager>();
        WWWForm form = SetUpLoginCredentials();
        form.AddField("bldg_id", bld_id);
        form.AddField("data", intelz);

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
                instance.intel = (int)downloadReturnJSON[1]["intel"];
                

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
                    myMapMgr.UpdateBuildingPanelText();

                    //load GameManager.instance data
                    GameManager.instance.intel = (int)bugPlacementJson[1]["intel"];
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
        myMapMgr.t2ButtonUp();
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
                myMapMgr.UpdateBuildingPanelText();

                //load GameManager.instance data
                GameManager.instance.intel = (int)bugPlacementJson[1]["intel"];
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
        myMapMgr.t2ButtonUp();
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
                myMapMgr.active_bldg_t3_count++; //manually increment the active building up.
                myMapMgr.UpdateBuildingPanelText();

                //load GameManager.instance data
                GameManager.instance.intel = (int)bugPlacementJson[1]["intel"];
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
        myMapMgr.t3ButtonUp();
        myMapMgr.deployingBotnet = false;
    }

    public void CountBugTotals ()
    {//this function counts up total bug numbers out of all of the locations JSON data
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
            instance.t1_total = 0;
            instance.t2_total = 0;
            instance.t3_total = 0;
        }
    }

}
