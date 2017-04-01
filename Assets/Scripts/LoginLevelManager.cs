using UnityEngine;
using System.Collections;
using Firebase;
using Facebook.Unity;
using UnityEngine.UI;
using System.Collections.Generic;

public class LoginLevelManager : MonoBehaviour {



	public Text emailText, passwordText;
	public string userID, userName;

	void Start () {
		InitializeFirebase ();

		if (FB.IsInitialized) {
			FB.ActivateApp();
		} else {
			//Handle FB.Init
			FB.Init(SetInit, OnHideUnity);
		}
	}

	// Handle initialization of the necessary firebase modules:
	void InitializeFirebase() {
		Debug.Log("Setting up Firebase Auth");
		GameManager.instance.auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
		GameManager.instance.auth.StateChanged += GameManager.instance.AuthStateChanged;
		GameManager.instance.AuthStateChanged(this, null); //initialize auth-state
	}



	public void RegisterNewEmailUser () {
		string email = emailText.text;
		string password = passwordText.text;

		if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
		{
			//Error handling
			Debug.LogError("User must input email and password");
			return;
		}
	
		GameManager.instance.auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
			if (task.IsCanceled) {
				Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
				return;
			}
			if (task.IsFaulted) {
				Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
				return;
			}

			// Firebase user has been created.
			Firebase.Auth.FirebaseUser newUser = task.Result;


			GameManager.instance.LoadGame();
			/*
			GameManager.instance.user = newUser;
			userName = newUser.DisplayName;
			userID = newUser.UserId;
			Debug.Log("user id is seen as: "+newUser.UserId);
			Debug.LogFormat("Firebase user created successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
			*/
		});
	}

	public void SignInEmailUser () {
		string email = emailText.text;
		string password = passwordText.text;

		if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
		{
			//Error handling
			Debug.LogWarning("User must input email and password.");
			return;
		}

		GameManager.instance.auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
			if (task.IsCanceled) {
				Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
				return;
			}
			if (task.IsFaulted) {
				Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
				return;
			}

			Firebase.Auth.FirebaseUser newUser = task.Result;

			GameManager.instance.LoadGame();
			/*
			GameManager.instance.user = newUser;
			userName = GameManager.instance.user.DisplayName;
			userID = GameManager.instance.user.UserId;
			Debug.Log("user id is seen as: "+GameManager.instance.user.UserId);
			Debug.LogFormat("User signed in successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
			*/
		});
	}



	//this is called automatically on mobile- as user is persistently logged in after 1st login
	void SetInit () {
		FB.ActivateApp();
		if (FB.IsLoggedIn) {
			Debug.Log ("FB is logged in- SetInit");

			AuthenticateFirebaseFB(Facebook.Unity.AccessToken.CurrentAccessToken.TokenString);


		} else {
			Debug.Log ("FB is not logged in");

		}

	}

	void OnHideUnity (bool isGameShown) {

		if (!isGameShown) {
			Time.timeScale = 0;
		} else {
			Time.timeScale = 1;
		}
	}

	public void FBlogin ()  {

		List<string> permissions = new List<string>();
		permissions.Add("public_profile");
		permissions.Add("user_friends");
		//permissions.Add("email");


		FB.LogInWithReadPermissions (permissions, AuthCallBack);

	}

	//this is only called after manual login
	void AuthCallBack (IResult result) {

		if (result.Error != null) {
			Debug.Log (result.Error);
		} else {

			if (FB.IsLoggedIn) {
				Debug.Log ("FB is logged in- authCallback");
				AuthenticateFirebaseFB(Facebook.Unity.AccessToken.CurrentAccessToken.TokenString);

			} else {
				Debug.Log ("FB is NOT logged in- authCallback");

			}

		}

	}


	void AuthenticateFirebaseFB (string token) {
		Firebase.Auth.Credential credential =
			Firebase.Auth.FacebookAuthProvider.GetCredential(token);
		GameManager.instance.auth.SignInWithCredentialAsync(credential).ContinueWith(task => {
			if (task.IsCanceled) {
				Debug.LogError("SignInWithCredentialAsync was canceled.");
				return;
			}
			if (task.IsFaulted) {
				Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
				return;
			}

			Firebase.Auth.FirebaseUser newUser = task.Result;

			//Call gamemanager to load into the game
			GameManager.instance.LoadGame();
			/*
			GameManager.instance.user = newUser;
			userName = GameManager.instance.user.DisplayName;
			userID = GameManager.instance.user.UserId;
			Debug.Log("user id is seen as: "+GameManager.instance.user.UserId);
			Debug.LogFormat("User signed in successfully: {0} ({1})",
				newUser.DisplayName, newUser.UserId);
			*/
		});
	}
}
