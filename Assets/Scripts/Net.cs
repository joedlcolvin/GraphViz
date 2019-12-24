using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Net : MonoBehaviour
{

	public int numNodes = 100;
	public float opinionRadius = 0.5f;
	public float deltaOpinion = 0.01f;
	public int opIterations = 10000;
	public float vizRadius = 10;
	public int interactionType = 0;
	public int arrangeIterations = 50;

	public bool paused;

	public string pipePath = @"netPipe";

	Network network;

	public GameObject Sphere;
	public GameObject Line;
	public GameObject SelectedSphere;

	public Text meanOpinionUI;

	public GameObject ForcedLeftSphere;
	public GameObject ForcedRightSphere;

	bool gameOver;

	float startTime1;
	float startTime2;

	IEnumerator opinionCoroutine;

	void Start()
	{
		gameOver = false;
		paused = false;
		startTime1 = Time.time;
		startTime2 = Time.time;

		// Get Settings
		GameObject settings = GameObject.Find("Settings");
		numNodes = settings.GetComponent<SettingsVars>().numNodes;
		opinionRadius = settings.GetComponent<SettingsVars>().opinionRadius;
		deltaOpinion = settings.GetComponent<SettingsVars>().deltaOp;
		opIterations = settings.GetComponent<SettingsVars>().totalInteractions;
		vizRadius = settings.GetComponent<SettingsVars>().vizRadius;
		interactionType = settings.GetComponent<SettingsVars>().interactionType;
		arrangeIterations = settings.GetComponent<SettingsVars>().arrangeIterations;

		network = new Network(numNodes);
		GenerateObjects(network, arrangeIterations);

		opinionCoroutine = OpinionRun(opIterations, network, opinionRadius, deltaOpinion, ForcedLeftSphere, ForcedRightSphere);
		StartCoroutine(opinionCoroutine);
	}

	void Update()
	{
		// WATCH REARRANGMENT

		//if (Time.time - startTime > 0.2f)
		//{
			//print("Rearrange");
			//DestroyLines();
			//RearrangeObjects(network, 1, frameX/10);
			//GenerateLines(network.edges);
			//startTime = Time.time;
		//}

		// Limit frameRate
		if (Time.time - startTime1 > 0.1f)
		{
			ColorSpheres(network.nodes);
			startTime1 = Time.time;
		}
		if (Time.time - startTime2 > 1f && !gameOver)
		{
			float meanOp = network.GetMeanOpinion();
			meanOpinionUI.text = "Mean Opinion: " + meanOp.ToString();
			startTime2 = Time.time;
		}
	}

	IEnumerator OpinionRun(int iterations, Network net, float radius, float deltaOp, GameObject ForcedLeftSphere, GameObject ForcedRightSphere)
	{
		using (StreamWriter pipe = File.CreateText(pipePath))
		{
			for(int i=0;i<iterations;i++)
			{
				while(paused) yield return null;
				// Force opinion
				ForceOpinion(ForcedLeftSphere, ForcedRightSphere);

				// Update opinions
				UpdateOpinion(net, radius, deltaOp);

				// Make JSON string
				string nodesJson = JsonUtility.ToJson(net.nodes);
				print(nodesJson);

				// Write to file
				pipe.Write(nodesJson);
				pipe.FlushAsync();

				yield return null;
			}
		}
		gameOver = true;
		print("Game Over. Player " + (Mathf.Sign(net.GetMeanOpinion()-0.5f)+1)/2 + " won!");
	}

	void UpdateOpinion(Network net, float radius, float deltaOp)
	{
		foreach(KeyValuePair<int, Edge> edge in net.edges)
		{
			Node fromNode = net.nodes[edge.Value.fromNodeId];
			Node toNode = net.nodes[edge.Value.toNodeId];
			float fromOp = fromNode.opinion;
			float toOp = toNode.opinion;
			(float, float) opPair = OpinionInteraction(fromOp, toOp, radius, deltaOp);
			(fromNode.opinion, toNode.opinion) = opPair;
		}
	}

	(float, float) OpinionInteraction(float fromOp, float toOp, float radius, float deltaOp)
	{
		// If within radius, fromOp moves deltaOp towards toOp
		// Otherwise, both opinions stay the same
		// Opinions stay in interval (0,1)
		if(interactionType == 0) {
			if(Mathf.Abs(fromOp-toOp)<radius)
			{
				fromOp += Mathf.Sign(toOp-fromOp)*deltaOp;
				fromOp = Mathf.Min(1f,fromOp);
				fromOp = Mathf.Max(0f,fromOp);
			}
			return (fromOp, toOp);
		} else if (interactionType == 1) {
			if(Mathf.Abs(fromOp-toOp)<radius)
			{
				float newFromOp = fromOp + Mathf.Sign(toOp-fromOp)*deltaOp;
				float newToOp = toOp + Mathf.Sign(fromOp-toOp)*deltaOp;
				//fromOp += Mathf.Sign(toOp-fromOp)*deltaOp;
				fromOp = newFromOp;
				fromOp = Mathf.Min(1f,fromOp);
				fromOp = Mathf.Max(0f,fromOp);

				toOp = newToOp;
				toOp = Mathf.Min(1f,toOp);
				toOp = Mathf.Max(0f,toOp);
			}
			return (fromOp, toOp);
		} else if (interactionType == 2) {
			if(Mathf.Abs(fromOp-toOp)<radius)
			{
				float newFromOp = fromOp + Mathf.Sign(toOp-fromOp)*deltaOp;
				float newToOp = toOp + Mathf.Sign(fromOp-toOp)*deltaOp;
				//fromOp += Mathf.Sign(toOp-fromOp)*deltaOp;
				fromOp = newFromOp;
				fromOp = Mathf.Min(1f,fromOp);
				fromOp = Mathf.Max(0f,fromOp);

				toOp = newToOp;
				toOp = Mathf.Min(1f,toOp);
				toOp = Mathf.Max(0f,toOp);
			} else {
				float newFromOp = fromOp - Mathf.Sign(toOp-fromOp)*deltaOp;
				float newToOp = toOp - Mathf.Sign(fromOp-toOp)*deltaOp;
				//fromOp += Mathf.Sign(toOp-fromOp)*deltaOp;
				fromOp = newFromOp;
				fromOp = Mathf.Min(1f,fromOp);
				fromOp = Mathf.Max(0f,fromOp);

				toOp = newToOp;
				toOp = Mathf.Min(1f,toOp);
				toOp = Mathf.Max(0f,toOp);
			}
			return (fromOp, toOp);
		} else {
			return (-1,-1);
		}
	}

	public void ForceOpinion(GameObject ForcedLeftSphere, GameObject ForcedRightSphere)
	{
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
				network.nodes[id].opinion = 1f;
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
				network.nodes[id].opinion = 0f;
				rightClickedNode.transform.parent = transform;
			}
		}

	}

	int IdFromName(string name)
	{
		return int.Parse(Regex.Match(name, @"\d+").Value);
	}

	void GenerateSphere(float opinion, int id)
	{
		Vector3 randomSpawnPosition = new Vector3(vizRadius+1, vizRadius+1, vizRadius+1);
		while (randomSpawnPosition.magnitude > vizRadius)
		{
			randomSpawnPosition = new Vector3(
							UnityEngine.Random.Range(-vizRadius,vizRadius),
							UnityEngine.Random.Range(-vizRadius,vizRadius),
							UnityEngine.Random.Range(-vizRadius,vizRadius));
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

	void GenerateObjects(Network net, int arrangeIterations)
	{
		// Generate Spheres for nodes
		GenerateSpheres(net.nodes);

		// Rearrange Nodes and edges
		RearrangeObjects(net, arrangeIterations, vizRadius/10);

		// Generate Lines for edges
		GenerateLines(net.edges);
	}

	void RearrangeObjects(Network net, int iterations, float t)
	{
		float area = 4f/3f*Mathf.PI*Mathf.Pow(vizRadius,2f);
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
				float newMagnitude = Mathf.Min(newPos.magnitude, vizRadius);
				newPos = newPos.normalized*newMagnitude;
				pos[vId].SetPositionAndRotation(newPos, Quaternion.identity);
				//pos[vId].SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			}
			// Reduce temperature as layout approaches a better configuration
			t = t*0.98f;
		}
	}

	class Network
	{
		//Network variables
		public Dictionary<int, Node> nodes;
		public Dictionary<int, Edge> edges;

		//Constructor
		public Network(int numNodes)
		{
			nodes = new Dictionary<int, Node>();
			edges = new Dictionary<int, Edge>();
			
			// Barabasi-Albert generation
			for(int n=0;n<numNodes;n++)
			{
				// Generate node id
				int id = n;

				// Generate initial opinion
				float opinion = UnityEngine.Random.Range(0f,1f);

				// Generate node and add to nodes list
				Node newNode = new Node(id, opinion);
				nodes.Add(id, newNode);

				// Generate edges for that node
				if (nodes.Count == 2)
				{
					// Generate edge id
					int edgeId = edges.Count;

					// Get the two node ids
					int[] ids = new int[nodes.Keys.Count];
					nodes.Keys.CopyTo(ids,0);

					Edge newEdge = new Edge(edgeId, ids[1], ids[0]);
					edges.Add(edgeId, newEdge);
					nodes[ids[1]].outDegree += 1;
					nodes[ids[0]].inDegree += 1;
				} 
				else if (nodes.Count > 2) 
				{
					foreach(KeyValuePair<int, Node> node in nodes)
					{
						float r = UnityEngine.Random.Range(0f,1f);
						float inDegreeAdj = node.Value.inDegree+1;
						float sumOfAllDegrees = edges.Count+nodes.Count;
						float p = inDegreeAdj/sumOfAllDegrees;
						if(r < p)
						{
							// Generate edge id
							int edgeId = edges.Count;

							Edge newEdge = new Edge(edgeId,
										id,
										node.Key);
							edges.Add(edgeId, newEdge);
							nodes[id].outDegree += 1;
							node.Value.inDegree += 1;
						}
					}
				}
			}
		}
		public float GetMeanOpinion()
		{
			float m = 0;
			for(int i=0;i<nodes.Count;i++)
			{
				m+= nodes[i].opinion;
			}
			m = m/nodes.Count;
			return m;
		}
	}

	[Serializable]
	public class Node
	{
		//Node variables
		public int id;
		public float opinion;

		//Node analytics variables (perhaps should be in network somehow
		public int inDegree;
		public int outDegree;

		//Constructor
		public Node(int idNumber, float opinionFloat)
		{
			id = idNumber;
			opinion = opinionFloat;
			inDegree = 0;
			outDegree = 0;
		}
	}

	[Serializable]
	public class Edge
	{
		//Edge variables
		public int id;
		public int fromNodeId;
		public int toNodeId;

		//Constructor
		public Edge(int idNumber, int fromId, int toId)
		{
			id = idNumber;
			fromNodeId = fromId;
			toNodeId = toId;
		}
	}

}
