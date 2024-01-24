#include "time.h"


void TimeInit()
{
	CurrentTimeOfDay = 0.25f;
}

void TimeUpdate()
{
	CurrentTimeOfDay += TICKRATE;
	if (CurrentTimeOfDay >= MaxTimePerDay)
		CurrentTimeOfDay = 0;
}

float GetCurrentTimeOfDay()
{
	return CurrentTimeOfDay / MaxTimePerDay;
}