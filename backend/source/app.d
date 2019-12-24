void UpdateOpinion(Network net, float radius, float deltaOp)
{
	foreach (edgeValue; net.edges.byValue)
	{
		Node fromNode = net.nodes[edgeValue.fromNodeId];
		Node toNode = net.nodes[edgeValue.toNodeId];
		float fromOp = fromNode.opinion;
		float toOp = toNode.opinion;
		auto opPair = OpinionInteraction(fromOp, toOp, radius, deltaOp);
		fromNode.opinion = opPair[0];
		toNode.opinion = opPair[1];
	}
}

auto OpinionInteraction(float fromOp, float toOp, float radius, float deltaOp)
{
	import std.typecons : tuple;
	import std.math : abs, sign;
	import std.algorithm : min, max;

	// If within radius, fromOp moves deltaOp towards toOp
	// Otherwise, both opinions stay the same
	// Opinions stay in interval (0,1)
	if (interactionType == 0)
	{
		if (abs(fromOp - toOp) < radius)
		{
			fromOp += sign(toOp - fromOp) * deltaOp;
			fromOp = min(1f, fromOp);
			fromOp = max(0f, fromOp);
		}
		return tuple(fromOp, toOp);
	}
	else if (interactionType == 1)
	{
		if (abs(fromOp - toOp) < radius)
		{
			float newFromOp = fromOp + sign(toOp - fromOp) * deltaOp;
			float newToOp = toOp + sign(fromOp - toOp) * deltaOp;
			//fromOp += sign(toOp-fromOp)*deltaOp;
			fromOp = newFromOp;
			fromOp = min(1f, fromOp);
			fromOp = max(0f, fromOp);

			toOp = newToOp;
			toOp = min(1f, toOp);
			toOp = max(0f, toOp);
		}
		return tuple(fromOp, toOp);
	}
	else if (interactionType == 2)
	{
		if (abs(fromOp - toOp) < radius)
		{
			float newFromOp = fromOp + sign(toOp - fromOp) * deltaOp;
			float newToOp = toOp + sign(fromOp - toOp) * deltaOp;
			//fromOp += sign(toOp-fromOp)*deltaOp;
			fromOp = newFromOp;
			fromOp = min(1f, fromOp);
			fromOp = max(0f, fromOp);

			toOp = newToOp;
			toOp = min(1f, toOp);
			toOp = max(0f, toOp);
		}
		else
		{
			float newFromOp = fromOp - sign(toOp - fromOp) * deltaOp;
			float newToOp = toOp - sign(fromOp - toOp) * deltaOp;
			//fromOp += sign(toOp-fromOp)*deltaOp;
			fromOp = newFromOp;
			fromOp = min(1f, fromOp);
			fromOp = max(0f, fromOp);

			toOp = newToOp;
			toOp = min(1f, toOp);
			toOp = max(0f, toOp);
		}
		return tuple(fromOp, toOp);
	}
	else
	{
		return tuple(-1, -1);
	}
}

class Network
{
	//Network variables
	Node[int] nodes;
	Edge[int] edges;

	//Constructor
	this(int numNodes)
	{
		// Barabasi-Albert generation
		for (int n = 0; n < numNodes; n++)
		{
			// Generate node id
			int id = n;

			// Generate initial opinion
			float opinion = UnityEngine.Random.Range(0f, 1f);

			// Generate node and add to nodes list
			Node newNode = new Node(id, opinion);
			nodes[id] = newNode;

			// Generate edges for that node
			if (nodes.length == 2)
			{
				// Generate edge id
				int edgeId = edges.length;

				auto ids = nodes.keys;

				Edge newEdge = new Edge(edgeId, ids[1], ids[0]);
				edges[edgeId] = newEdge;
				nodes[ids[1]].outDegree += 1;
				nodes[ids[0]].inDegree += 1;
			}
			else if (nodes.length > 2)
			{
				foreach (node; nodes.byKeyValue)
				{
					float r = UnityEngine.Random.Range(0f, 1f);
					float inDegreeAdj = node.value.inDegree + 1;
					float sumOfAllDegrees = edges.length + nodes.length;
					float p = inDegreeAdj / sumOfAllDegrees;
					if (r < p)
					{
						// Generate edge id
						int edgeId = edges.length;

						Edge newEdge = new Edge(edgeId, id, node.key);
						edges[edgeId] = newEdge;
						nodes[id].outDegree += 1;
						node.value.inDegree += 1;
					}
				}
			}
		}
	}

	float GetMeanOpinion()
	{
		float m = 0;
		for (int i = 0; i < nodes.length; i++)
		{
			m += nodes[i].opinion;
		}
		m = m / nodes.length;
		return m;
	}
}

class Node
{
	//Node variables
	int id;
	float opinion;

	//Node analytics variables (perhaps should be in network somehow
	int inDegree;
	int outDegree;

	//Constructor
	this(int idNumber, float opinionFloat)
	{
		id = idNumber;
		opinion = opinionFloat;
		inDegree = 0;
		outDegree = 0;
	}
}

class Edge
{
	//Edge variables
	int id;
	int fromNodeId;
	int toNodeId;

	//Constructor
	this(int idNumber, int fromId, int toId)
	{
		id = idNumber;
		fromNodeId = fromId;
		toNodeId = toId;
	}
}
