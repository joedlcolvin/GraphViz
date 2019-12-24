#!/usr/bin/env bash

gcc -shared -o libnano_shim.so nano_shim.c -fPIC -lnanomsg
