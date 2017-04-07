using UnityEngine;
using System.Collections;

public class PopulatedLocation : MonoBehaviour {

	public string buildingName, buildingID, photo_reference, google_type;
	public float myLat, myLng;

    private MapLevelManager myMapMgr;

    void Start ()
    {
        myMapMgr = MapLevelManager.FindObjectOfType<MapLevelManager>();
    }

    public void ClickedOn ()
    {
        myMapMgr = FindObjectOfType<MapLevelManager>();
        myMapMgr.LoadBuildingPanel(buildingName, buildingID, photo_reference, google_type);
    }

}
