#include <stdlib.h>
#include "memory.h"
#include "log.h"
#include <stdio.h>
#include <raylib.h>

void* MCalloc(size_t count, size_t size, char* label)
{
	MemoryAllocCount += count * size;
	DebugLog(TextFormat("[MEMORY] MCALLOC Allocation size (%d * %d) = +%d -> %d for %s\n", count, size, (count * size), MemoryAllocCount, label));
	return calloc(count, size);
}

void* MRealloc(void* _Block, size_t count, size_t size, size_t prevsize, char* label)
{
	MemoryAllocCount -= prevsize;
	MemoryAllocCount += count * size;
	DebugLog(TextFormat("[MEMORY] MREALLOC Allocation size (%d * %d) = +%d -> %d for %s\n", count, size, (count * size), MemoryAllocCount, label));
	return realloc(_Block, count * size);
}

void MFree(void* _Block, size_t count, size_t size, char* label)
{
	MemoryAllocCount -= size * count;
	DebugLog(TextFormat("[MEMORY] FREE Allocation size (%d * %d) = +%d -> %d for %s\n", count, size, (count * size), MemoryAllocCount, label));
	free(_Block);
}