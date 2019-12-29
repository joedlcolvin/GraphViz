using System;
using UnityEngine;
using System.Collections.Generic;


namespace MyClasses
{
	public class Settings
	{
		public int numNodes;
		public int totalInteractions;
		public int interactionType;
		//public int arrangeIterations;
		public float opinionRadius;
		public float deltaOp;
		public float vizRadius;

		public Settings()
		{
			this.numNodes = 100;
			this.totalInteractions = 10000;
			this.interactionType = 2;
			//this.arrangeIterations = 50;
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
		public string messageType { get; set; }
		public List<Node> nodes{ get; set; }
		public List<Edge> edges{ get; set; }
	}

	class Graph
	{
		//Graph variables
		public Dictionary<int, Node> nodes;
		public Dictionary<int, Edge> edges;

		public Graph()
		{
			this.nodes = new Dictionary<int, Node>();
			this.edges = new Dictionary<int, Edge>();
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

	public class Node
	{
		//Node variables
		public int id{ get; set; }
		public float opinion{ get; set; }

		//Node analytics variables (perhaps should be in network somehow
		public int inDegree{ get; set; }
		public int outDegree{ get; set; }
	}

	public class Edge
	{
		//Edge variables
		public int id{ get; set; }
		public int fromNodeId{ get; set; }
		public int toNodeId{ get; set; }
	}
}
