using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using LitJson;
using System;
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;

public class GameManager : MonoBehaviour {

	public static GameManager instance;

	public Firebase.Auth.FirebaseAuth auth;
	public Firebase.Auth.FirebaseUser user;
	public DatabaseReference db_reference;
	public string last_login_ts;

	//integers and timestamps for game data/processing
	public int intel;
	public DateTime last_intel_ts;

	//strings for JSON text of game data.
	public string player_json, bugged_locations_json;

	public static string ServerURL = "http://spygame.argzombie.com"; //the base string for all URLS

	//urls to target php scripts
	private string loadAllGameDataURL = GameManager.ServerURL+"/LoadAllData.php";


	void Awake () {
		MakeSingleton();
	}

	void Start () {
		//initialize the database connection
		FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://arg-spies.firebaseio.com/");
		db_reference = FirebaseDatabase.DefaultInstance.RootReference;
	}

	void MakeSingleton () {
		if (instance != null) {
			Destroy( this );
		}else{
			GameManager.instance = this;
			DontDestroyOnLoad(this);
		}
	}

	WWWForm ReturnGameCredentials () {
		//update user from auth
		user = auth.CurrentUser;
		//load web form credentials
		WWWForm form = new WWWForm();
		form.AddField("id", user.UserId);
		form.AddField("username", user.DisplayName);
		form.AddField("login_ts", last_login_ts);
		form.AddField("client", "mob");
		return form;
	}

	//load into the game screen
	public void LoadGame () {
		

		StartCoroutine(LoadAllGameData());
	}

	// Track state changes of the auth object.
	public void AuthStateChanged(object sender, System.EventArgs eventArgs) {
		if (GameManager.instance.auth.CurrentUser != user) {
			bool signedIn = user != GameManager.instance.auth.CurrentUser && GameManager.instance.auth.CurrentUser != null;
			if (!signedIn && GameManager.instance.user != null) {
				Debug.Log("Signed out " + GameManager.instance.user.UserId);
			}
			GameManager.instance.user = auth.CurrentUser;
			if (signedIn) {
				Debug.Log("Signed in " + GameManager.instance.user.UserId);
				LoadGame();
			}
		}
	}

	IEnumerator LoadAllGameData () {
		GameManager.instance.last_login_ts = "12/31/1999 11:59:59";//setup the magic timestamp
		WWWForm form = ReturnGameCredentials();

		WWW www = new WWW(loadAllGameDataURL, form);
		yield return www;

		if (www.error ==null){
			Debug.Log(www.text);
			JsonData gameData = JsonMapper.ToObject(www.text);
			if(gameData[0].ToString() == "Success") {
				Debug.Log("Loading into game ... Stdby");
				intel = (int)gameData[1]["intel"];
				last_intel_ts = DateTime.Parse(gameData[1]["intel_ts"].ToString());
				last_login_ts = (gameData[1]["mob_login_ts"].ToString());

				player_json = gameData[1].ToJson();
				bugged_locations_json = gameData[2].ToJson();

				SceneManager.LoadScene("Map Screen");

			} else {
				Debug.LogWarning(gameData[1].ToString());
			}
		}else{
			Debug.LogWarning(www.error);
		}
	}
}
