using UnityEngine;
using System.Collections;

// individual packets
public struct packet {
	public Vector3 position;
	public Quaternion rotation;
	public int intstamp;
	public float timestamp;
}

public class netInterpolation : MonoBehaviour {
	NetworkViewID viewID;
	packet[] packets;
	int currentIntstamp;		// number of "current" packet in interpolation
	float currentTimestamp;		// time of recieving "current" packet in interpolation
	Vector3[] bezPosition;		// co-efs for position interpolation
	Quaternion[] bezRotation;	// co-efs for rotation interpolation
	int noOfPoints = 0;			// no of points we're interpolating from
	float normalizer = 1;		// cache a divisor for FixedUpdate
	// Binomial co-efs
	int[,] BinCoefs = new int[5,5] {
		{1,4,6,4,1},
		{1,3,3,1,0},
		{1,2,1,0,0},
		{1,1,0,0,0},
		{1,0,0,0,0}
	};

	// called to start it
	public void Init(NetworkViewID cartViewID) {
		viewID = cartViewID;
		NetworkView cgt = gameObject.AddComponent("NetworkView") as NetworkView;
		cgt.observed = this;					// track this script
		cgt.viewID = cartViewID;
		cgt.stateSynchronization = NetworkStateSynchronization.Unreliable;

		// fixed no of packets to interpolate from
		packets = new packet[5];
		currentIntstamp = 0;
		currentTimestamp = 0;
		bezPosition = new Vector3[5];
		bezRotation = new Quaternion[5];

		/*debug
		Vector3[] tmpvec = new Vector3[5] {
			new Vector3(0,0,0),
			new Vector3(1,0,0),
			new Vector3(2,7,0),
			new Vector3(1,0,0),
			new Vector3(0,0,0)
		};
		int startPacket = 0;
		noOfPoints = 5-startPacket;
		Debug.Log("START");
		for(int i=startPacket;i<5;i++) {
			bezPosition[i-startPacket] = BinCoefs[startPacket,i-startPacket] * tmpvec[i];
		}
		Debug.Log("lerps:");
		Debug.Log(BezPositionInterpolate(0f));
		Debug.Log(BezPositionInterpolate(0.25f));
		Debug.Log(BezPositionInterpolate(0.5f));
		Debug.Log(BezPositionInterpolate(0.75f));
		Debug.Log(BezPositionInterpolate(1f));
		Debug.Log("END");
		*/
	}

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
		// set up local copies
		Vector3 pPosition;
		Quaternion pRotation;
		int pIntstamp;
		float pTimestamp;

		// check if we need to update or if we are being updated (ie server vs client)
		if (stream.isWriting) {
			// server
			// update stuff
			currentIntstamp = currentIntstamp+1;

			// copy for streaming
			pPosition = gameObject.transform.position;
			//pRotation = gameObject.rigidbody.rotation;
			pIntstamp = currentIntstamp;		// this will tick over after about half a year, so don't leave it running too long

			// set stream
			stream.Serialize(ref pPosition);
			//stream.Serialize(ref pRotation);
			stream.Serialize(ref pIntstamp);

		} else {
			// client
			pPosition = new Vector3();
			pRotation = new Quaternion();
			pIntstamp = 0;
			pTimestamp = Time.time;

			// get stream
			stream.Serialize(ref pPosition);
			//stream.Serialize(ref pRotation);
			stream.Serialize(ref pIntstamp);

			// add the packet to the list
			// ordering: 0=oldest,4=latest
			for(int i=0;i<5;i++) {
				// check timestamp
				if (i!=4 && packets[i+1].intstamp < pIntstamp) {
					packets[i] = packets[i+1];
				} else {
					packets[i].position = pPosition;
					packets[i].rotation = pRotation;
					packets[i].timestamp = pTimestamp;
					packets[i].intstamp = pIntstamp;
					break;
				}
			}

			// set up the stuff needed to interpolate
			//Debug.Log(Time.time - packets[0].timestamp);
			//if (packets[1].timestamp-packets[0].timestamp>0.01) Debug.Log("Balls");
			/*string tmpstr = "";
			tmpstr += "0";
			tmpstr += "," + (packets[1].intstamp-packets[0].intstamp).ToString();
			tmpstr += "," + (packets[2].intstamp-packets[0].intstamp).ToString();
			tmpstr += "," + (packets[3].intstamp-packets[0].intstamp).ToString();
			tmpstr += "," + (packets[4].intstamp-packets[0].intstamp).ToString();
			if (tmpstr!="0,1,2,3,4") {
				Debug.LogError(tmpstr);
			}*/
			// set us to the last one
			currentIntstamp = packets[0].intstamp;
			currentTimestamp = packets[0].timestamp;

			// set up the Bezier curves
			// first find out the starting packet
			int startPacket = 0;
			for(int i=0;i<4;i++) {
				if (packets[i+1].timestamp > Time.time-0.1) {
					startPacket = i;
					break;
				}
			}
			// set the co-efs
			for(int i=startPacket;i<5;i++) {
				bezPosition[i-startPacket] = BinCoefs[startPacket,i-startPacket] * packets[i].position;
				//bezRotation[i-startPacket] = BinCoefs[startPacket,i-startPacket] * packets[i].rotation;
			}
			// set no of point
			noOfPoints = 5-startPacket;
			// do division now for speed
			normalizer = 1/(packets[4].timestamp-packets[startPacket].timestamp);
		}
	}

	// lerp it
	void FixedUpdate() {
		if (Network.isClient) {
			if (noOfPoints>1) {
				float t = Time.time - currentTimestamp;
				t = t*normalizer;	// normalize the timestep
				gameObject.transform.position = BezPositionInterpolate(t);
			}
		}
	}

	// get the interpolated position
	Vector3 BezPositionInterpolate(float t) {
		Vector3 tmp = new Vector3();
		for(int i=0;i<noOfPoints;i++) {
			tmp += bezPosition[i] * QuickPower(t,i) * QuickPower(1-t,noOfPoints-1-i);
			//Debug.Log(QuickPower(t,i) * QuickPower(1-t,noOfPoints-i));
		}
		//if(viewID.ToString()=="AllocatedID: 1") Debug.Log(tmp);
		return tmp;
	}
	// faster than math.pow I think
	float QuickPower(float t, int n) {
		if(n==0) return 1;
		if(t==0) return 0;
		float tmp=1;
		for(int i=n;i!=0;i--) {
			tmp *= t;
		}
		return tmp;
	}
}
