using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using LitJson;
using System;
using Firebase;
//using Firebase.Unity.Editor;
//using Firebase.Database;

using SimpleFirebaseUnity;
using SimpleFirebaseUnity.MiniJSON;

public class GameManager : MonoBehaviour {

	public static GameManager instance;
	public static FirebaseHandler fireBaseRef;

	//firebase data
	public Firebase.Auth.FirebaseAuth auth;
	public Firebase.Auth.FirebaseUser user;
	//public DatabaseReference db_reference;
	public string last_login_ts;


	//integers and timestamps for game data/processing
	public int intel;
	public DateTime last_intel_ts;

	//strings for JSON text of game data.
	public string player_json, bugged_locations_json, google_places_json;

	void Awake () {
		MakeSingleton();
	}

	void Start () {
		//initialize the database connection
//		FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://arg-spies.firebaseio.com/");
//		db_reference = FirebaseDatabase.DefaultInstance.RootReference;

		fireBaseRef = GetComponent<FirebaseHandler>();
		if (fireBaseRef==null){
			Debug.LogWarning("Unable to locate the firebase handler on the game-manager object");
		}
	}

	void MakeSingleton () {
		if (instance != null) {
			Destroy( this );
		}else{
			GameManager.instance = this;
			DontDestroyOnLoad(this);
		}
	}

	public void AuthCompleted () {
		
	}

	//load into the game screen
	void LoadGame () {
		


	}

	public void StartNewPlayer (string codeName) {
		Player player = new Player(auth.CurrentUser.UserId ,codeName, 0, DateTime.Now);
		string json = JsonUtility.ToJson(player);


		//db_reference.Child("players").Child(user.UserId).SetRawJsonValueAsync(json);
		Debug.Log("apparently we just created our player?... maybe?");
		SceneManager.LoadScene("Map Screen");
	}

	// Track state changes of the auth object.
	public void AuthStateChanged(object sender, System.EventArgs eventArgs) {
		if (GameManager.instance.auth.CurrentUser != user) {
			bool signedIn = user != GameManager.instance.auth.CurrentUser && GameManager.instance.auth.CurrentUser != null;
			if (!signedIn && GameManager.instance.user != null) {
				Debug.Log("Signed out " + GameManager.instance.user.UserId);
				SceneManager.LoadScene("Login");
			}
			GameManager.instance.user = auth.CurrentUser;
			if (signedIn) {
				Debug.Log("Signed in " + GameManager.instance.user.UserId);
				LoginLevelManager loginMgr = FindObjectOfType<LoginLevelManager>();
				if (loginMgr !=null){
					loginMgr.AuthComplete();
				}else{
					Debug.LogError("Unable to locate loginLvl manager- new login detected");
				}
			}
		}
	}


}
