using UnityEngine;
using System.Collections;
using Facebook.Unity;
using UnityEngine.UI;
using System.Collections.Generic;

public class LoginLevelManager : MonoBehaviour {


	public GameObject registerEmailPanel, loginSuccessPanel, newGamePromptPanel;
	public Text emailText, passwordText, codenameText;
	public string userID, userName;

	void Start () {
		if (FB.IsInitialized) {
			FB.ActivateApp();
		} else {
			//Handle FB.Init
			FB.Init(SetInit, OnHideUnity);
		}
	}



	public void AuthComplete () {
		loginSuccessPanel.SetActive(true);
	}

	public void BeginNewGamePrompt () {
		if (newGamePromptPanel.activeInHierarchy) {
			newGamePromptPanel.SetActive(false);
		}else{
			newGamePromptPanel.SetActive(true);
		}
	}

	public void StartNewCharacter () {
		if (codenameText.text != ""){
            //GameManager.instance.StartNewPlayer(codenameText.text);
            Debug.LogWarning("You still need to set up StartGame and ContinueGame");
		}
	}

	//this is called automatically on mobile- as user is persistently logged in after 1st login
	void SetInit () {
		FB.ActivateApp();
		if (FB.IsLoggedIn) {
			Debug.Log ("FB is logged in- SetInit");

            FB.API("/me?fields=id", HttpMethod.GET, UpdateUserId);
            FB.API("/me", HttpMethod.GET, UpdateUserName);

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

                FB.API("/me?fields=id", HttpMethod.GET, UpdateUserId);
                FB.API("/me", HttpMethod.GET, UpdateUserName);

            } else {
				Debug.Log ("FB is NOT logged in- authCallback");
			}
		}
	}


    private void UpdateUserId(IResult result)
    {
        if (result.Error == null)
        {
            GameManager.instance.userID = result.ResultDictionary["id"].ToString();
        }
        else
        {
            Debug.Log(result.Error);
        }

        //ping the game server
        GameManager.instance.LoginToGameServer();
    }

    private void UpdateUserName(IResult result)
    {
        if (result.Error == null)
        {
            GameManager.instance.userName = result.ResultDictionary["name"].ToString();
        }
        else
        {
            Debug.Log(result.Error);
        }
    }

}
