//#ifndef TICKRATE
//#define TICKS 60.0f
//#define TICKRATE 1.0f/TICKS
//#endif // !TICKRATE
#pragma once
#include <raylib.h>

float TICKRATE;
float CurrentTimeOfDay;
Color TimeOfDayGradient[16];

void TimeInit();
void TimeUpdate();
float GetCurrentTimeOfDay();
Color GetAmbientLightColor();