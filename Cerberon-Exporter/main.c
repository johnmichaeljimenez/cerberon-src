#pragma warning(disable:4996)

#include <raylib.h>
#include <stdio.h>
#include <string.h>

int export(char* directory);
void exportSprites(char* directory, char* pakName);

int main(int c, char** a)
{
	SetTraceLogLevel(LOG_NONE);

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
	
	printf("----- Exporting %s -----\n\n", GetDirectoryPath(directory));
	char* spritesDir = TextFormat("%s\\sprites\\", directory);
	char* tilesDir = TextFormat("%s\\tiles\\", directory);

	exportSprites(spritesDir, TextFormat("%s\\sprites.pak", directory));
	exportSprites(tilesDir, TextFormat("%s\\tiles.pak", directory));

	printf("----- Export Done -----\n");
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

	FILE* file = fopen(pakName, "wb");
	FilePathList pathList = LoadDirectoryFilesEx(directory, ".png", false);

	printf("----- Exporting %s (%d) -----\n", GetFileName(pakName), pathList.count);

	fwrite(&pathList.count, sizeof(int), 1, file);

	for (int i = 0; i < pathList.count; i++)
	{
		char* fileName = pathList.paths[i];
		char* tName = GetFileNameWithoutExt(fileName);
		char* name[32];
		strcpy_s(name, 32, tName);

		Image img = LoadImage(fileName);
		int size = 0;
		char* data = ExportImageToMemory(img, ".png", &size);
		UnloadImage(img);

		fwrite(&name, sizeof(char), 32, file);
		fwrite(&size, sizeof(int), 1, file);
		fwrite(data, sizeof(char), size, file);

		printf("%s\n", name);
	}

	printf("----- Export %s successful -----\n", GetFileName(pakName));

	fclose(file);
	UnloadDirectoryFiles(pathList);
	printf("\n");
}