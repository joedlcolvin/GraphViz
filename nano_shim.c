#include <nanomsg/nn.h>
#include <nanomsg/pair.h>
#include <assert.h>
#include <stdio.h>

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

int sendSettings(char* settings, size_t length)
{
	fprintf(logFile, "%s\n", settings);
	fprintf(logFile, "%ld\n", length);
	int ret = nn_send(backendSocket, settings, length, 0);
	int errCode = nn_errno();
	fprintf(logFile, "%d %s\n", ret, nn_strerror(errCode));
	fclose(logFile);
	if (ret < 0)
		return -1;
	return 0;
}

