using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Runtime.InteropServices;
using System.Reflection;
using Newtonsoft.Json;
using MyClasses;

public class Net : MonoBehaviour
{
	public bool paused;
	public bool stopped;

	Graph network;

	public GameObject Sphere;
	public GameObject Line;
	public GameObject SelectedSphere;

	public Text meanOpinionUI;

	public GameObject ForcedLeftSphere;
	public GameObject ForcedRightSphere;

	public GameObject InGameUi;
	public GameObject InGameMenu;

	Settings settings;

	float startTime;

	[DllImport ("libnano_shim.so")]
	private static extern int connect ();

	[DllImport ("libnano_shim.so")]
	private static extern int send (string a, long b);

	[DllImport ("libnano_shim.so")]
	private static extern string receive ();

	void Start()
	{
		startTime = Time.time;

		// Get Settings
		GameObject settingsGameObject = GameObject.Find("Settings");
		settings = settingsGameObject.GetComponent<SettingsVars>().settings;

		// Make nanomsg connection with backend
		int r = connect();
		Debug.Log(r + " connected.");
		
		// Turn settings into JSON string
		string settingsJsonString = settings.GetJsonForBackendSettings();
		string settingsMessage = JsonToMessageString("settings", settingsJsonString);
		
		// Send settings JSON string to backend over nanomsg connection
		r = send(settingsMessage, settingsMessage.Length);
		Debug.Log(r + " sent settings");

		// Listen for backend sending generated network
		// TODO DEBUG
		network = new Graph();

		string netJson = receive();
		print(netJson);
		BackendMessage instantiateMsg = JsonConvert.DeserializeObject<BackendMessage>(netJson);
		foreach(Node node in instantiateMsg.nodes)
		{
			network.nodes.Add(node.id, node);
		}
		foreach(Edge edge in instantiateMsg.edges)
		{
			network.edges.Add(edge.id, edge);
		}

		// Generate visualisation of network
		// arrIter = settings.arrangeIterations;
		int arrIter = 500;
		GenerateObjects(network, arrIter);

		// Find paused event in InGameUi and subscribe PauseSim() to it
		InGameUi.GetComponent<InGameUi>().pause.AddListener(PauseSim);
		// Find continue event in InGameMenu and subscribe StartSim() to it
		InGameMenu.GetComponent<InGameMenu>().continue_.AddListener(StartSim);
		// Find stop event in InGameMenu and subscribe StopSim() to it
		InGameMenu.GetComponent<InGameMenu>().stop.AddListener(StopSim);

		// Tell backend to start model
		StartSim();

		// Begin coroutine for recieving node state updates
		StartCoroutine(nodeStateUpdates());
	}

	void Update()
	{
		if(!paused)
		{
			// Check for forced nodes and send to backend
			Dictionary<int, float> forcing = ForceOpinion(ForcedLeftSphere, ForcedRightSphere);
			if (forcing.Count != 0)
			{
				// Create JSON string for dict and send to backend
				string forcingJsonString = NodesDictToJsonString(forcing);
				string forcingMessage = JsonToMessageString("forcing", forcingJsonString);
				int r = send(forcingMessage, forcingMessage.Length);
				Debug.Log(r + " sent forcing");
			}

			// With limited update rate, update the color of the spheres
			if (Time.time - startTime > 0.1f)
			{
				ColorSpheres(network.nodes);
				startTime = Time.time;
			}
		}
	}

	private IEnumerator nodeStateUpdates()
	{
		while(!stopped)
		{
			string netJson = receive();
			BackendMessage updateMsg = JsonConvert.DeserializeObject<BackendMessage>(netJson);
			foreach(Node node in updateMsg.nodes)
			{
				network.nodes[node.id] = node;
			}
			yield return null;
		}
		yield return null;
	}

	public void PauseSim()
	{
		paused = true;
		string pauseJson = "{ \"messageType\" : \"pause\" }";
		int r = send(pauseJson, pauseJson.Length);
		Debug.Log(r + " sent pause");
	}

	public void StartSim()

	{
		paused = false;
		stopped = false;
		string startJson = "{ \"messageType\" : \"start\" }";
		int r = send(startJson, startJson.Length);
		Debug.Log(r + " sent start");
	}

	public void StopSim()
	{
		stopped = true;
		string stopJson = "{ \"messageType\" : \"stop\" }";
		int r = send(stopJson, stopJson.Length);
		Debug.Log(r + " sent stop");
	}

	// TODO debug
	public Dictionary<int, float> ForceOpinion(GameObject ForcedLeftSphere, GameObject ForcedRightSphere)
	{
		var forcedNodes = new Dictionary<int, float>();

		// Check forcing
		int n = ForcedLeftSphere.transform.childCount;
		bool check = (n!=0);
		// Apply forcing if it exists
		if(check)
		{
			for(int j=0;j<n;j++)
			{
				GameObject clickedNode = ForcedLeftSphere.transform.GetChild(j).gameObject;
				int id = IdFromName(clickedNode.name);
				forcedNodes.Add(id, 1f);
				clickedNode.transform.parent = transform;
			}
		}

		// Check forcing
		int m = ForcedRightSphere.transform.childCount;
		bool checkRight = (m!=0);
		// Apply forcing if it exists
		if(checkRight)
		{
			for(int j=0;j<m;j++)
			{
				GameObject rightClickedNode = ForcedRightSphere.transform.GetChild(j).gameObject;
				int id = IdFromName(rightClickedNode.name);
				forcedNodes.Add(id, 0f);
				rightClickedNode.transform.parent = transform;
			}
		}
		return forcedNodes;
	}

	int IdFromName(string name)
	{
		return int.Parse(Regex.Match(name, @"\d+").Value);
	}

	void GenerateSphere(float opinion, int id)
	{
		Vector3 randomSpawnPosition = new Vector3(settings.vizRadius+1, settings.vizRadius+1, settings.vizRadius+1);
		while (randomSpawnPosition.magnitude > settings.vizRadius)
		{
			randomSpawnPosition = new Vector3(
							UnityEngine.Random.Range(-settings.vizRadius,settings.vizRadius),
							UnityEngine.Random.Range(-settings.vizRadius,settings.vizRadius),
							UnityEngine.Random.Range(-settings.vizRadius,settings.vizRadius));
		}
		GameObject newSphere = (GameObject) Instantiate(Sphere,
								randomSpawnPosition,
								Quaternion.identity);
		newSphere.transform.parent = transform;
		newSphere.gameObject.name = "Sphere "+ id.ToString();
		newSphere.gameObject.tag = "Sphere";
	}

	void GenerateLine(int inId, int outId, int id)
	{
		GameObject inSphere = GameObject.Find("Sphere " + inId.ToString());
		GameObject outSphere = GameObject.Find("Sphere " + outId.ToString());
		Vector3 inSpherePos = inSphere.transform.position;
		Vector3 outSpherePos = outSphere.transform.position;
		Vector3 linePosition = (inSpherePos + outSpherePos)/2;
		Vector3 relativePosition = inSpherePos - outSpherePos;
		Quaternion rotation = Quaternion.LookRotation(relativePosition);
		GameObject newLine = (GameObject) Instantiate(	Line,
								linePosition,
								rotation);
		float length = relativePosition.magnitude;
		newLine.transform.localScale = new Vector3(0.025f,0.025f, length);

		newLine.transform.parent = transform;
		newLine.gameObject.name = "Line "+ id.ToString();
		newLine.gameObject.tag = "Line";
	}

	void GenerateSpheres(Dictionary<int, Node> nodes)
	{
		foreach(KeyValuePair<int, Node> node in nodes)
		{
			GenerateSphere(node.Value.opinion, node.Key);
		}
		ColorSpheres(nodes);
	}

	void GenerateLines(Dictionary<int, Edge> edges)
	{
		foreach(KeyValuePair<int, Edge> edge in edges)
		{
			GenerateLine(edge.Value.toNodeId, edge.Value.fromNodeId, edge.Key);
		}
	}
	
	void DestroyLines()
	{
		GameObject[] lines = GameObject.FindGameObjectsWithTag("Line");
		for(int i=0; i<lines.Length;i++)
		{
			Destroy(lines[i]);
		}
	}

	void ColorSpheres(Dictionary<int, Node> nodes)
	{
		foreach(KeyValuePair<int, Node> node in nodes)
		{
			GameObject sphere = GameObject.Find("Sphere " + node.Key.ToString());
			sphere.GetComponent<Renderer>().material.color = 
				new Color(	node.Value.opinion,
						0f, 
						1-node.Value.opinion, 
						0f);
		}
	}

	void GenerateObjects(Graph net, int arrangeIterations)
	{
		// Generate Spheres for nodes
		GenerateSpheres(net.nodes);

		// Rearrange Nodes and edges
		RearrangeObjects(net, arrangeIterations, settings.vizRadius/10);

		// Generate Lines for edges
		GenerateLines(net.edges);
	}

	void RearrangeObjects(Graph net, int iterations, float t)
	{
		float area = 4f/3f*Mathf.PI*Mathf.Pow(settings.vizRadius,2f);
		float k = Mathf.Pow(area/net.nodes.Count,1f/3f);
		float fa(float x) {
			return Mathf.Pow(x,3f)/k;
		}
		float fr(float x) {
			return Mathf.Pow(k,3f)/x;
		}

		// Get positions
		Dictionary<int, Transform> pos = new Dictionary<int, Transform>();
		foreach(KeyValuePair<int, Node> node in net.nodes)
		{
			int id = node.Key;
			GameObject sphere = GameObject.Find("Sphere " + id.ToString());
			pos.Add(id, sphere.transform);
		}

		for(int i=0; i<iterations; i++)
		{
			// Define array for displacements
			Dictionary<int,Vector3> disp = new Dictionary<int, Vector3>();

			//Calculate repulsive forces
			foreach(KeyValuePair<int, Node> vEntry in net.nodes)
			{
				int vId = vEntry.Key;
				disp[vId]=Vector3.zero;
				foreach(KeyValuePair<int, Node> uEntry in net.nodes)
				{
					int uId = uEntry.Key;
					if(uId!=vId)
					{
						Vector3 diff = pos[vId].position-pos[uId].position;
						Vector3 newPos 	= disp[vId] 
								+ diff.normalized
								* fr(diff.magnitude);
						disp[vId] = newPos;
					}
				}
			}

			//Calculate attractive forces
			foreach(KeyValuePair<int, Edge> edge in net.edges)
			{
				int vId = edge.Value.fromNodeId;
				int uId = edge.Value.toNodeId;
				Vector3 diff = pos[vId].position-pos[uId].position;
				Vector3 vNewDisp = disp[vId] - diff.normalized*fa(diff.magnitude);
				Vector3 uNewDisp = disp[uId] + diff.normalized*fa(diff.magnitude);
				disp[vId] = vNewDisp;
				disp[uId] = uNewDisp;
			}

			// Limit max displacement to temp. t and prevent displacement out of frame
			foreach(KeyValuePair<int, Node> node in net.nodes)
			{
				int vId = node.Key;
				Vector3 newPos 	= pos[vId].position 
						+ disp[vId].normalized
						* Mathf.Min(disp[vId].magnitude,t);
				float newMagnitude = Mathf.Min(newPos.magnitude, settings.vizRadius);
				newPos = newPos.normalized*newMagnitude;
				pos[vId].SetPositionAndRotation(newPos, Quaternion.identity);
			}
			// Reduce temperature as layout approaches a better configuration
			t = t*0.98f;
		}
	}

	string NodesDictToJsonString (Dictionary<int, float> nodes)
	{
		string str = "{";
		foreach(KeyValuePair<int, float> node in nodes)
		{  
			str += "\"" + node.Key.ToString() + "\"";
			str += ":";
			str += "\"" + node.Value.ToString() + "\"";
			str += ",";
		}
		str += "}";
		return str;
	}

	string JsonToMessageString (string messageType, string json)
	{
		string str = "{";
		str += "\"messageType\":" + "\"" + messageType + "\"";
		str += ",";
		str += "\"" + messageType + "\"";
		str += ":";
		str += json;
		str += "}";
		return str;
	}
}
