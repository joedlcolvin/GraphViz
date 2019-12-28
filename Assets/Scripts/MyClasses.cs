using System.Collections.Generic;

namespace MyClasses
{
	public class Settings
	{
		public int numNodes;
		public int totalInteractions;
		public int interactionType;
		public int arrangeIterations;
		public float opinionRadius;
		public float deltaOp;
		public float vizRadius;

		public Settings()
		{
			this.numNodes = 100;
			this.totalInteractions = 10000;
			this.interactionType = 2;
			this.arrangeIterations = 50;
			this.opinionRadius = 0.6f;
			this.deltaOp = 0.01f;
			this.vizRadius = 10f;
		}

		public string GetJsonForBackendSettings()
		{
			string str = "{";
			str += "\"numNodes\"";
			str += ":";
			str += numNodes.ToString();
			str += ",";
			str += "\"totalInteractions\"";
			str += ":";
			str += totalInteractions.ToString();
			str += ",";
			str += "\"interactionType\"";
			str += ":";
			str += interactionType.ToString();
			str += ",";
			str += "\"opinionRadius\"";
			str += ":";
			str += opinionRadius.ToString();
			str += ",";
			str += "\"deltaOp\"";
			str += ":";
			str += deltaOp.ToString();
			str += "}";
			return str;
		}
	}
	
	public class BackendMessage
	{
		BackendMessageType messageType;
		object[] arr;

		public Node[] GetNodes()
		{
			Node[] nodes = new Node[arr.Length];
			if(messageType == BackendMessageType.Nodes)
			{
				for(int i=0;i<arr.Length;i++)
				{
					nodes[i] = (Node)arr[i];
				}
			}
			return (Node[]) arr;
		}
		public Edge[] GetEdges()
		{
			Edge[] edges = new Edge[arr.Length];
			if(messageType == BackendMessageType.Edges)
			{
				for(int i=0;i<arr.Length;i++)
				{
					edges[i] = (Edge)arr[i];
				}
			}
			return (Edge[]) arr;
		}
	}

	enum BackendMessageType
	{
		Nodes,
		Edges
	}

	class Graph
	{
		//Graph variables
		public Dictionary<int, Node> nodes;
		public Dictionary<int, Edge> edges;

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

	public class Node
	{
		//Node variables
		public int id;
		public float opinion;

		//Node analytics variables (perhaps should be in network somehow
		public int inDegree;
		public int outDegree;
	}

	public class Edge
	{
		//Edge variables
		public int id;
		public int fromNodeId;
		public int toNodeId;
	}
}
