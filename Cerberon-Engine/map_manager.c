#pragma warning(disable:4996)
#include <raylib.h>
#include <raymath.h>
#include <stdio.h>
#include <stdlib.h>
#include "map_manager.h"

void LoadMap(char* filename)
{
	/*
	 * FORMAT:
	 *
	 * map name [32]
	 * 
	 * player pos x
	 * player pos y
	 * rotation
	 * 
	 * wall block count
	 * - wall block 1
	 * -- position x
	 * -- position y
	 * -- size x
	 * -- size y
	 * 
	 * - wall block 2
	 * -- ...
	 *
	 */

	FILE* file = fopen(filename, "rb");


	fclose(file);
}