#pragma warning(disable:4996)
#include <raylib.h>
#include <stdio.h>
#include "log.h"

const char* LOGFILE = "log.txt";

void ClearLog()
{
	fclose(fopen(LOGFILE, "w"));
}

void DebugLog(char* msg)
{
	FILE* file = fopen(LOGFILE, "a");
	fprintf(file, msg);
	fclose(file);
}