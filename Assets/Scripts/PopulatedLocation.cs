using UnityEngine;
using System.Collections;
using System;

public class PopulatedLocation : MonoBehaviour {

	public string buildingName, buildingID, photo_reference, google_type;
	public float myLat, myLng;
    public int t1_count, t2_count, t3_count;
    public bool d1, d2, d3;//these store download enhancer's presence
    public DateTime last_download_ts;

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

}
