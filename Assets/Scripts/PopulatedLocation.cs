using UnityEngine;
using System.Collections;
using System;

public class PopulatedLocation : MonoBehaviour {

	public string buildingName, buildingID, photo_reference, google_type;
	public float myLat, myLng;
    public int t1_count, t2_count, t3_count;
    public DateTime last_download_ts;

    private MapLevelManager myMapMgr;

    void Start ()
    {
        myMapMgr = MapLevelManager.FindObjectOfType<MapLevelManager>();
    }

    public void ClickedOn ()
    {
        myMapMgr = FindObjectOfType<MapLevelManager>();
        myMapMgr.LoadBuildingPanel(buildingName, buildingID, photo_reference, google_type, last_download_ts, t1_count, t2_count, t3_count);
    }

}
