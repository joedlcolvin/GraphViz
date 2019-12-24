import std.typecons : Flag, Yes, No;
import nanomsg : NanoSocket;

void main()
{
	import std.json : parseJSON;
	import std.conv : text;
	import nanomsg : BindTo;
	import std.exception : enforce;
	import std.stdio;

	auto frontendSocket = NanoSocket(NanoSocket.Protocol.pair, BindTo("ipc:///tmp/graphViz.ipc"));

	writeln(__LINE__);

	auto settingsMessage = frontendSocket.getMessage();
	writeln(__LINE__);

	enforce(settingsMessage.messageType == FrontendMessageType.settings);
	writeln(__LINE__);

	auto settings = settingsMessage.settings;

	writeln(settings);

	auto network = new Network(settings.numNodes);

	// TODO: send network to frontend

	frontendSocket.waitForStartMessage();

	while (true)
	{
		auto maybeMsg = frontendSocket.tryGetMessage();
		if (!maybeMsg.isNull)
		{
			auto msg = maybeMsg.get;
			switch (msg.messageType)
			{
				case FrontendMessageType.pause:
					frontendSocket.waitForStartMessage();
					break;
				case FrontendMessageType.stop:
					network = new Network(settings.numNodes);
					frontendSocket.waitForStartMessage();
					break;
				default:
					throw new Exception("Unexpected message of type "
						~ msg.messageType.text);
			}
		}

		//updateOpinion();
		import std.stdio;
		writeln("HI");
		import core.thread : Thread;
		import core.time : dur;
		Thread.sleep(1.dur!"seconds");

		// TODO: send updates to frontend
	}
}

auto waitForStartMessage(ref NanoSocket socket)
{
	import std.exception : enforce;

	auto startMessage = socket.getMessage();
	enforce(startMessage.messageType == FrontendMessageType.start);
}
	
auto getMessage(ref NanoSocket socket)
{
	import std.json : parseJSON;
	auto jsonStr = cast(string) socket.receive().bytes;
	import std.stdio;
	writeln(jsonStr);
	return FrontendMessage.fromJson(
		parseJSON(jsonStr));
}

auto tryGetMessage(ref NanoSocket socket)
{
	import std.typecons : Nullable, No;
	import std.range : empty;
	import std.json : parseJSON;

	auto msg = socket.receive(No.blocking).bytes;
	if (msg.empty)
		return Nullable!FrontendMessage.init;
	return Nullable!FrontendMessage(
		FrontendMessage.fromJson(parseJSON((cast(string) msg))));
}

enum FrontendMessageType
{
	settings,
	start,
	pause,
	stop
}

struct FrontendMessage
{
	import std.json : JSONValue;

	FrontendMessageType messageType;
	Settings settings;

	static FrontendMessage fromJson(JSONValue json)
	{
		import std.conv : to;

		FrontendMessage msg;
		msg.messageType = json["messageType"].str.to!FrontendMessageType;
		if (msg.messageType == FrontendMessageType.settings)
			msg.settings = Settings.fromJson(json["settings"]);
		return msg;
	}
}

struct Settings
{
	import std.json : JSONValue;

	int interactionType;
	int numNodes;

	static Settings fromJson(JSONValue json)
	{
		return Settings(cast(int) json["interactionType"].integer, cast(int) json["numNodes"].integer);
	}
}

void updateOpinion(Settings settings, Network net, float radius, float deltaOp)
{
	foreach (edgeValue; net.edges.byValue)
	{
		Node fromNode = net.nodes[edgeValue.fromNodeId];
		Node toNode = net.nodes[edgeValue.toNodeId];
		float fromOp = fromNode.opinion;
		float toOp = toNode.opinion;
		auto opPair = opinionInteraction(settings, fromOp, toOp, radius, deltaOp);
		fromNode.opinion = opPair[0];
		toNode.opinion = opPair[1];
	}
}

auto opinionInteraction(Settings settings, float fromOp, float toOp, float radius, float deltaOp)
{
	import std.typecons : tuple;
	import std.math : abs, sgn;
	import std.algorithm : min, max;

	// If within radius, fromOp moves deltaOp towards toOp
	// Otherwise, both opinions stay the same
	// Opinions stay in interval (0,1)
	if (settings.interactionType == 0)
	{
		if (abs(fromOp - toOp) < radius)
		{
			fromOp += sgn(toOp - fromOp) * deltaOp;
			fromOp = min(1f, fromOp);
			fromOp = max(0f, fromOp);
		}
		return tuple(fromOp, toOp);
	}
	else if (settings.interactionType == 1)
	{
		if (abs(fromOp - toOp) < radius)
		{
			float newFromOp = fromOp + sgn(toOp - fromOp) * deltaOp;
			float newToOp = toOp + sgn(fromOp - toOp) * deltaOp;
			//fromOp += sgn(toOp-fromOp)*deltaOp;
			fromOp = newFromOp;
			fromOp = min(1f, fromOp);
			fromOp = max(0f, fromOp);

			toOp = newToOp;
			toOp = min(1f, toOp);
			toOp = max(0f, toOp);
		}
		return tuple(fromOp, toOp);
	}
	else if (settings.interactionType == 2)
	{
		if (abs(fromOp - toOp) < radius)
		{
			float newFromOp = fromOp + sgn(toOp - fromOp) * deltaOp;
			float newToOp = toOp + sgn(fromOp - toOp) * deltaOp;
			//fromOp += sgn(toOp-fromOp)*deltaOp;
			fromOp = newFromOp;
			fromOp = min(1f, fromOp);
			fromOp = max(0f, fromOp);

			toOp = newToOp;
			toOp = min(1f, toOp);
			toOp = max(0f, toOp);
		}
		else
		{
			float newFromOp = fromOp - sgn(toOp - fromOp) * deltaOp;
			float newToOp = toOp - sgn(fromOp - toOp) * deltaOp;
			//fromOp += sgn(toOp-fromOp)*deltaOp;
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
		return tuple(-1.0f, -1.0f);
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
		import std.random : uniform;

		// Barabasi-Albert generation
		for (int n = 0; n < numNodes; n++)
		{
			// Generate node id
			int id = n;

			// Generate initial opinion
			float opinion = uniform(0.0f, 1.0f);

			// Generate node and add to nodes list
			Node newNode = new Node(id, opinion);
			nodes[id] = newNode;

			// Generate edges for that node
			if (nodes.length == 2)
			{
				// Generate edge id
				auto edgeId = cast(int) edges.length;

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
					float r = uniform(0f, 1f);
					float inDegreeAdj = node.value.inDegree + 1;
					float sumOfAllDegrees = edges.length + nodes.length;
					float p = inDegreeAdj / sumOfAllDegrees;
					if (r < p)
					{
						// Generate edge id
						auto edgeId = cast(int) edges.length;

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
