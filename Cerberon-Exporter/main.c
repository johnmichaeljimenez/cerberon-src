#include <raylib.h>
#include <stdio.h>
#include <string.h>

int export(char* directory);
void exportSprites(char* directory, char* pakName);

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
	
	printf("----- Exporting %s -----\n-\n-\n-\n", GetDirectoryPath(directory));
	char* spritesDir = TextFormat("%s\\sprites\\", directory);
	char* tilesDir = TextFormat("%s\\tiles\\", directory);

	exportSprites(spritesDir, TextFormat("%s\\sprites.pak", directory));
	exportSprites(tilesDir, TextFormat("%s\\tiles.pak", directory));

	getch();
	return 0;
}

void exportSprites(char* directory, char* pakName)
{
	if (!DirectoryExists(directory))
	{
		printf("Directory not exist: %s\n", directory);
		return;
	}

	printf("----- Exporting %s -----\n", GetFileName(pakName));

	FilePathList pathList = LoadDirectoryFilesEx(directory, ".png", false);
	for (int i = 0; i < pathList.count; i++)
	{
		char* file = pathList.paths[i];
		char* tName = GetFileNameWithoutExt(file);
		char* name[32];

		strcpy_s(name, 32, tName);

		printf("%s %s\n", file, name);
	}


	UnloadDirectoryFiles(pathList);
}