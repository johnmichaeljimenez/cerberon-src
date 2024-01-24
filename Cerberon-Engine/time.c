#include "time.h"
#include <math.h>
#include "utils.h"


float MaxTimePerDay = 60.0f;
static int _currentTimeIndex;
static Color _previousAmbientColor;
static Color _currentAmbientColor;
static float lerpColorAmount;
static float maxSegment;

void TimeInit()
{
	maxSegment = MaxTimePerDay * 0.0625f;
	CurrentTimeOfDay = 0.25f;
	lerpColorAmount = 1;
}

void TimeUpdate()
{
	CurrentTimeOfDay += TICKRATE;
	if (CurrentTimeOfDay >= MaxTimePerDay)
		CurrentTimeOfDay = 0;

	_currentTimeIndex = (int)round(GetCurrentTimeOfDay() * 16);

	lerpColorAmount += TICKRATE;
	if (lerpColorAmount >= maxSegment)
	{
		lerpColorAmount = 0;
		_previousAmbientColor = _currentAmbientColor;
		_currentAmbientColor = TimeOfDayGradient[_currentTimeIndex];
	}
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

	DrawText(TextFormat("%.2f/%.2f = %.2f ---> %.2f %.2f", CurrentTimeOfDay, MaxTimePerDay, GetCurrentTimeOfDay(), lerpColorAmount, maxSegment), 4, 50, 32, GREEN);
}