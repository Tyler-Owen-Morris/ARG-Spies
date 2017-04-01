using System;

public class Player  {
	public string userID;
	public string displayName;
	public long intel;
	public DateTime intel_last_syc_ts;

	public Player() {}

	public Player(string usrID, string dispName, long intelig, DateTime intel_sync_ts) {
		this.userID = usrID;
		this.displayName = dispName;
		this.intel = intelig;
		this.intel_last_syc_ts=intel_sync_ts;
	}
	
}
