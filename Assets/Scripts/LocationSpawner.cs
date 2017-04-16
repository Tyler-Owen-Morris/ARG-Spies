using UnityEngine;
using System.Collections;
using System.IO;
using LitJson;
using UnityEngine.UI;
using System;

public class LocationSpawner : MonoBehaviour {

	private float minX, maxX, minY, maxY;
	public double m_per_deg_lat, m_per_deg_lon, lastGoogleLat, lastGoogleLng;
	public bool locationsNeedUpdate;

	private Vector3 screenCenter = new Vector3((Screen.width*0.5f), (Screen.height*0.5f), 0.0f);
	private string googlePlacesAPIURL = "https://maps.googleapis.com/maps/api/place/nearbysearch/json";
    private string googleNextPagePlacesURL = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?pagetoken=";
	private string googleAPIKey = "AIzaSyC0Ly6W_ljFCk5sV4n-T73e-rhRdNpiEe4";

	private static GameObject populatedLocationPrefab;

	void Start () {
		locationsNeedUpdate=true;
		populatedLocationPrefab = Resources.Load<GameObject>("Prefabs/Populated Location");
		UpdateLocations();
	}

	public void UpdateLocations () {
		if (locationsNeedUpdate==true) {
			//update the nearby location information
			StartCoroutine(GetGooglePlaces());
		} else {
			//don't update the google information, just remap the existing buildings.
			SpawnLocations();
		}
	}

	IEnumerator GetGooglePlaces () {
		string myWwwString = googlePlacesAPIURL;
		myWwwString += "?location=";
		if (Input.location.status == LocationServiceStatus.Running) {
			myWwwString += Input.location.lastData.latitude +","+Input.location.lastData.longitude;
			lastGoogleLat = Input.location.lastData.latitude;
			lastGoogleLng = Input.location.lastData.longitude;

		}  else {
			myWwwString += "37.70897,-122.4292";
			//this is assuming my home location
		}
		myWwwString += "&radius=400";
		//myWwwString += "&keyword=things";
		myWwwString += "&key="+ googleAPIKey;

		Debug.Log(myWwwString);
		WWW www = new WWW(myWwwString);
		yield return www;
		//Debug.Log(www.text);

		//File.WriteAllText(Application.dataPath + "/Resources/googlelocations.json", www.text.ToString());
		//googleJsonReturn = www.text;
		GameManager.instance.google_places_json = www.text;
		SpawnLocations();
		locationsNeedUpdate = false;
	}
	
	public void SpawnLocations () {
		//destroy the existing buildings
		GameObject[] oldBldgs = GameObject.FindGameObjectsWithTag("location");
		foreach (GameObject oldBldg in oldBldgs) {
			Destroy(oldBldg.gameObject);
		}
    	
		//string jsonString = File.ReadAllText(Application.dataPath + "/Resources/googlelocations.json");
		Debug.Log(GameManager.instance.google_places_json);
		JsonData bldgJson = JsonMapper.ToObject(GameManager.instance.google_places_json);
		Debug.Log(JsonMapper.ToJson(bldgJson));
        //JsonData foursquareJson = JsonMapper.ToObject(jsonReturn);

        //set up the map math before beginning building spawning
        double m_per_pixel_mapBG = GetMetersPerPixelOfGoogleMapImage();
        GoogleMapLoader my_GM = FindObjectOfType<GoogleMapLoader>();
        //int map_img_size = 560;//this worked for iPhone 5
        int map_img_size = Mathf.FloorToInt(Screen.width/2);
        double map_width_in_meters = (m_per_pixel_mapBG*map_img_size);
        double m_per_screen_pixel = map_width_in_meters / Screen.width;

        //**** DELETE THESE MANUAL VALUES ***** THESE SHOULD BE CALCULATED AT RUNTIME- TEMP VALUES BELOW SHOULD BE REMOVED.
        m_per_pixel_mapBG = 1.88973311814137f;
		map_width_in_meters = 621.722195868511f;
		m_per_screen_pixel = 0.944866559070685f;

        Debug.Log("Calculating m/px-BG: " + m_per_pixel_mapBG + "  Map width in meters: "+map_width_in_meters+"  and meters/pixel for building placement: " + m_per_screen_pixel);

        //Go through the JSON and spawn the nearby locations according to Google Places
        for (int i = 0; i < bldgJson["results"].Count; i++) {
			string myName = (string)bldgJson["results"][i]["name"];
			string myBldgID = (string)bldgJson["results"][i]["id"];
            JsonData thisEntry = JsonMapper.ToObject(JsonMapper.ToJson(bldgJson["results"][i]));
            string my_photo_ref = "";
            if (thisEntry.Keys.Contains("photos")) {
                my_photo_ref = (string)bldgJson["results"][i]["photos"][0]["photo_reference"];
            }
        	float lat = (float)(double)bldgJson["results"][i]["geometry"]["location"]["lat"];
			float lng = (float)(double)bldgJson["results"][i]["geometry"]["location"]["lng"];

			
			//Debug.Log (name + lat + lng);

			//calculate the average latitude between the two locations, and then calculate the meters/DEGREE lat/lon
			float latMid =(Input.location.lastData.latitude + lat)/2f;
			m_per_deg_lat = 111132.954 - 559.822 * Mathf.Cos( 2 * latMid ) + 1.175 * Mathf.Cos( 4 * latMid);
			m_per_deg_lon = 111132.954 * Mathf.Cos( latMid );

			//Debug.Log ("for the " + name + " building, meters per degree calculated as " + m_per_deg_lat + " m/deg lat, and " + m_per_deg_lon +" m/deg lon");
			double deltaLatitude = 0;
			double deltaLongitude = 0;
			if (Input.location.status == LocationServiceStatus.Running){
				deltaLatitude = (Input.location.lastData.latitude - lat);
				deltaLongitude = (Input.location.lastData.longitude - lng);
			}  else {
				deltaLatitude = (37.70883f - lat);
				deltaLongitude = (-122.4293 - lng);
			}
			double xDistMeters = deltaLongitude * m_per_deg_lon;
			double yDistMeters = deltaLatitude * m_per_deg_lat;
            float xScreenDist = (float)(xDistMeters * m_per_screen_pixel);
            float yScreenDist = (float)(yDistMeters * m_per_screen_pixel);
			

			
			GameObject instance = Instantiate(populatedLocationPrefab, gameObject.transform) as GameObject;
			PopulatedLocation my_locationScript = instance.GetComponent<PopulatedLocation>();
			my_locationScript.name = myName;
			my_locationScript.buildingName = myName;
			my_locationScript.buildingID = myBldgID;
			my_locationScript.photo_reference = my_photo_ref;
			my_locationScript.myLat = lat;
			my_locationScript.myLng = lng;
            my_locationScript.last_download_ts = DateTime.Parse("12/31/1999 12:59:59");//set unmatched buildings to default datetime
			float xCoord = (float)(screenCenter.x - (xScreenDist));
			float yCoord = (float)(screenCenter.y - (yScreenDist));
			Vector3 pos = new Vector3 (xCoord, yCoord, 0);

			instance.transform.SetParent(gameObject.transform);//set this gameobject to it's parent
			instance.transform.position = pos;

			//determine the loot class of the building
			string type_bldg = bldgJson["results"][i]["types"][0].ToString();
			my_locationScript.google_type = type_bldg;
//			if (type_bldg == "bakery" || type_bldg == "cafe" || type_bldg == "convenience_store" || type_bldg == "food" || type_bldg == "grocery_or_supermarket" || type_bldg == "restaurant") {
//				//food likely
//				instance.loot_code = "F";
//			} else if (type_bldg == "aquarium" || type_bldg == "bar" || type_bldg == "liquor_store" || type_bldg == "spa" || type_bldg == "zoo") {
//				//water likely
//				instance.loot_code = "W";
//			} else if (type_bldg == "bicycle_store" || type_bldg == "bowling_alley" || type_bldg == "car_repair" || type_bldg == "electrician" || type_bldg == "general_contractor" || type_bldg == "hardware_store" || type_bldg == "hospital" || type_bldg == "police" || type_bldg == "plumber") {
//				//supply likely
//				instance.loot_code = "S";
//			} else {
//				//generic loot class
//				instance.loot_code = "G";
//			}

			my_locationScript.SetUpContentIcons(); //this should set the default of all icons being off- before we look for matches

			
		}

        //AFTER placing all nearby buildings - Attempt to match with loaded game data.
        GameObject[] spawnedLocations = GameObject.FindGameObjectsWithTag("location"); //gather up the spawned locations to cycled through

        foreach (GameObject location in spawnedLocations) //cycle through the existing buildings for matches with loaded game data.
        {
            if(GameManager.instance.bugged_locations_json == "0" || GameManager.instance.bugged_locations_json == "")
            {
                Debug.Log("locations all spawned, but player is found to have NO locations bugged...");


                break;//leave the foreach loop- there's no json to load
            }else
            {
                PopulatedLocation my_location = location.GetComponent<PopulatedLocation>(); //load the existing game object's location script. (we need the building_id)
                JsonData buggedLocationsJSON = JsonMapper.ToObject(GameManager.instance.bugged_locations_json);

                for (int i=0;i<buggedLocationsJSON.Count; i++) //cycle through the buildings with bugs
                {
                    if (buggedLocationsJSON[i]["building_id"].ToString() == my_location.buildingID)
                    {
                        Debug.Log("MATCH FOUND ! - " + buggedLocationsJSON[i]["building_name"].ToString());
                        my_location.t1_count = (int)buggedLocationsJSON[i]["t1_bug_count"];
                        my_location.t2_count = (int)buggedLocationsJSON[i]["t2_bug_count"];
                        my_location.t3_count = (int)buggedLocationsJSON[i]["t3_bug_count"];
                        my_location.last_download_ts = DateTime.Parse(buggedLocationsJSON[i]["last_download_ts"].ToString());
                        //setup the download speedifyers
                        if((int)buggedLocationsJSON[i]["d1"]==1) {
                        	my_location.d1 =true;
                        }else{
                        	my_location.d1=false;
                        }
						if((int)buggedLocationsJSON[i]["d2"]==1) {
                        	my_location.d2 = true;
                        }else{
                        	my_location.d2 = false;
                        }
						if((int)buggedLocationsJSON[i]["d3"]==1) {
                        	my_location.d3 = true ;
                        }else{
                        	my_location.d3 = false ;
                        }
						my_location.SetUpContentIcons();//now that we've updated this location's content- update it's icons.
                        //break; //go to the next spawned location
                    }
                }
            }
        }
		
		

	}

	double GetMetersPerPixelOfGoogleMapImage ()
    {
        GoogleMapLoader my_GoogleMap = FindObjectOfType<GoogleMapLoader>();
        int zoom = 0;
        if (my_GoogleMap != null)
        {
            zoom = my_GoogleMap.zoom;
        }else
        {
            zoom = 16;
        }
        float my_lat = 0.0f;
        if (Input.location.status==LocationServiceStatus.Running)
        {
            my_lat = Input.location.lastData.latitude;
        }else
        {
            my_lat = 37.70897f;
        }
        //double my_value = 156543.03392f *( Mathf.Cos((my_lat*Mathf.PI) / 180 ) / Mathf.Pow(2,zoom));
        double my_value = (Mathf.Cos(my_lat*Mathf.PI/180)*2*Mathf.PI*6378137)/(256*Mathf.Pow(2, zoom));
        //my_value = my_value * FindObjectOfType<MapLevelManager>().zoomSlider.value; //scale the value to the current zoom level
        //my_value = 2.38865f;
        Debug.Log("Calculating m/pixel of original google image to be: " + my_value);
        return my_value;
    }

}
