#include "time.h"
#include <math.h>
#include "utils.h"
#include <raymath.h>

static float MaxTimePerDay = 30.0f;
static float MorningTime = 0.25f;
static float EveningTime = 0.85f;

static int _currentTimeIndex;
static Color _previousAmbientColor;
static Color _currentAmbientColor;
static float lerpColorAmount;
static float maxSegment;

void TimeInit()
{
	maxSegment = Remap(1, 0, 16, 0, MaxTimePerDay);
	CurrentTimeOfDay = 0.25f;
	lerpColorAmount = 1;
}

void TimeUpdate()
{
	float d = GetCurrentTimeOfDay();
	CurrentTimeOfDay += TICKRATE;
	if (CurrentTimeOfDay >= MaxTimePerDay)
		CurrentTimeOfDay = 0;

	_currentTimeIndex = (int)floorf(d * 16);

	lerpColorAmount += TICKRATE;
	if (lerpColorAmount >= maxSegment)
	{
		lerpColorAmount = 0;
		_previousAmbientColor = _currentAmbientColor;
		_currentAmbientColor = TimeOfDayGradient[_currentTimeIndex];
	}

	if (d < MorningTime)
		CurrentTimeStatus = TIMESTATUS_Midnight;
	else if (d < 0.5f)
		CurrentTimeStatus = TIMESTATUS_Morning;
	else if (d < EveningTime)
		CurrentTimeStatus = TIMESTATUS_Afternoon;
	else
		CurrentTimeStatus = TIMESTATUS_Evening;
}

float GetCurrentTimeOfDay()
{
	return CurrentTimeOfDay / MaxTimePerDay;
}

Color GetAmbientLightColor()
{
	return LerpColor(_previousAmbientColor, _currentAmbientColor, lerpColorAmount / maxSegment);
}

void DrawDebugTime()
{
	for (int i = 0; i < 16; i++)
	{
		DrawRectangle(4 + (32 * i), 60, 32, 32, TimeOfDayGradient[i]);
		if (i == _currentTimeIndex)
			DrawRectangleLines(4 + (32 * i), 60, 32, 32, GREEN);
	}


	DrawRectangle(4, 100, 32, 32, _previousAmbientColor);
	DrawRectangle(4, 140, 32, 32, _currentAmbientColor);

	DrawText(TextFormat("%d %.2f/%.2f = %.2f ---> %.2f %.2f", CurrentTimeStatus, CurrentTimeOfDay, MaxTimePerDay, GetCurrentTimeOfDay(), lerpColorAmount, maxSegment), 4, 50, 32, GREEN);
}