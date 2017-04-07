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
	public int intel;
	public DateTime last_intel_ts, lastLoginTime;

    //URL's for the game 
    private string loginURL = serverURL + "/LoginToGame.php";
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
                GameManager.instance.player_json = allGameReturnJson[1].ToString();
                GameManager.instance.bugged_locations_json = allGameReturnJson[2].ToString();
                Debug.Log("player_json: " + GameManager.instance.player_json);
                Debug.Log("location_json: " + GameManager.instance.bugged_locations_json);

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

}
