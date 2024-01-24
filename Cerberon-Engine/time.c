#include "time.h"
#include <math.h>


float MaxTimePerDay = 60.0f;

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

Color GetAmbientLightColor()
{
	return TimeOfDayGradient[(int)round(GetCurrentTimeOfDay() * 16)];
}