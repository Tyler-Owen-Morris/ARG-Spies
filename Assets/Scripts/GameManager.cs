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
	public int intel=0;
	public DateTime last_intel_ts, lastLoginTime;

    //URL's for the game 
    private string placeT1BugURL = serverURL + "/PlaceT1Bug.php";
    private string loadAllGameDataURL = serverURL + "/LoadAllData.php";

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
                GameManager.instance.bugged_locations_json = JsonMapper.ToJson(allGameReturnJson[2]);
                Debug.Log("player_json: " + GameManager.instance.player_json);
                Debug.Log("location_json: " + GameManager.instance.bugged_locations_json);

                //load GameManager.instance data
                GameManager.instance.intel = (int)allGameReturnJson[1]["intel"];

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
                    GameManager.instance.player_json = bugPlacementJson[1].ToString();
                    GameManager.instance.bugged_locations_json = bugPlacementJson[2].ToString();
                    Debug.Log("player_json: " + GameManager.instance.player_json);
                    Debug.Log("location_json: " + GameManager.instance.bugged_locations_json);

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
        
    }

}
