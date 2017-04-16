using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class PopulatedLocation : MonoBehaviour {

	public string buildingName, buildingID, photo_reference, google_type;
	public float myLat, myLng;
    public int t1_count=0, t2_count=0, t3_count=0;
    public bool d1=false, d2=false, d3=false;//these store download enhancer's presence
    public DateTime last_download_ts;
    public GameObject t1_image, t2_image, t3_image, d1_image, d2_image, d3_image;

    private MapLevelManager myMapMgr;

    void Start ()
    {
        myMapMgr = MapLevelManager.FindObjectOfType<MapLevelManager>();
    }

    public void ClickedOn ()
    {
        myMapMgr = FindObjectOfType<MapLevelManager>();
        myMapMgr.LoadBuildingPanel(this.gameObject , buildingName, buildingID, photo_reference, google_type, last_download_ts, t1_count, t2_count, t3_count);
    }

    public void SetUpContentIcons () {
    	if (t1_count>0){
    		t1_image.SetActive(true);
    		Text count_text = t1_image.GetComponentInChildren<Text>();
    		count_text.text=t1_count.ToString();
    	}else{
    		t1_image.SetActive(false);
    	}
    	if (t2_count>0){
    		t2_image.SetActive(true);
    		Text count_text = t2_image.GetComponentInChildren<Text>();
    		count_text.text = t2_count.ToString();
    	}else{
    		t2_image.SetActive(false);
    	}
    	if (t3_count>0){
    		t3_image.SetActive(true);
    		Text count_text = t3_image.GetComponentInChildren<Text>();
    		count_text.text = t3_count.ToString();
    	}else{
    		t3_image.SetActive(false);
    	}

    	if(d1){d1_image.SetActive(true);}else{d1_image.SetActive(false);}
		if(d2){d2_image.SetActive(true);}else{d2_image.SetActive(false);}
		if(d3){d3_image.SetActive(true);}else{d3_image.SetActive(false);}
    }
}
