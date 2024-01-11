#include <raylib.h>
#include <stdio.h>
#include <string.h>

int export(char* directory);

int main(int c, char** a)
{
	if (c != 2)
	{
		printf("Directory is empty!\n");
		getch();
		return 0;
	}

	char* directory = a[1];
	return export(directory);
}


int export(char* directory)
{
	if (!DirectoryExists(directory))
	{
		printf("Directory not found: %s", directory);
		return 1;
	}

	char* spritesDir = TextFormat("%s\\sprites\\\n", directory);
	char* tilesDir = TextFormat("%s\\tiles\\\n", directory);
	printf(spritesDir);
	printf(tilesDir);

	if (!DirectoryExists(spritesDir))
	{

	}

	if (!DirectoryExists(tilesDir))
	{

	}

	getch();
	return 0;
}