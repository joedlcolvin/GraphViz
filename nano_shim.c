#include <nanomsg/nn.h>
#include <nanomsg/pair.h>
#include <assert.h>
#include <stdio.h>
#include <string.h>

int backendSocket;
FILE *logFile;

int connect()
{
	logFile = fopen("/tmp/nano_shim.log", "w");
	backendSocket = nn_socket(AF_SP, NN_PAIR);
	if (backendSocket < 0)
		return -1;

	int timeout = 5000;
	int ret;
	ret = nn_setsockopt(backendSocket, NN_SOL_SOCKET, NN_SNDTIMEO, &timeout, sizeof(timeout));
	if (ret < 0)
		return -2;

	ret = nn_connect(backendSocket, "ipc:///tmp/graphViz.ipc");
	if (ret < 0)
		return -3;

	return 0;
}

int send(char* jsonString, size_t length)
{
	fprintf(logFile, "%s\n", "send()");
	fprintf(logFile, "%s\n", jsonString);
	fprintf(logFile, "%ld\n", length);
	int ret = nn_send(backendSocket, jsonString, length, 0);
	if (ret < 0)
	{
		int errCode = nn_errno();
		fprintf(logFile, "%d %s\n", errCode, nn_strerror(errCode));
		return -1;
	}
	return 0;
}

char* receive()
{
	fprintf(logFile, "%s\n", "receive()");
	char* msg = NULL;
	void* buf = NULL;
	int n_bytes = nn_recv(backendSocket, &buf, NN_MSG, 0);
	if (n_bytes < 0)
	{
		int errCode = nn_errno();
		fprintf(logFile, "%d %s\n", n_bytes, nn_strerror(errCode));
	}
	else
	{
		msg = strndup(buf, n_bytes);
		fprintf(logFile, "%s\n", msg);
	}
	return msg;
}
