//#ifndef TICKRATE
//#define TICKS 60.0f
//#define TICKRATE 1.0f/TICKS
//#endif // !TICKRATE
#pragma once
#include <raylib.h>

typedef enum TimeStatus
{
	TIMESTATUS_Midnight,
	TIMESTATUS_Morning,
	TIMESTATUS_Afternoon,
	TIMESTATUS_Evening
} TimeStatus;

float TICKRATE;
float CurrentTimeOfDay;
Color TimeOfDayGradient[16];
TimeStatus CurrentTimeStatus;

void TimeInit();
void TimeUpdate();
float GetCurrentTimeOfDay();
Color GetAmbientLightColor();
void DrawDebugTime();