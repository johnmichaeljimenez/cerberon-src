#include <stdlib.h>
#include "memory.h"
#include <raylib.h>

void* MCalloc(size_t count, size_t size)
{
	MemoryAllocCount += count;
	TraceLog(LOG_INFO, "[MEMORY] Allocation size +%d = %d", count, MemoryAllocCount);
	return calloc(count, size);
}

void MFree(void* _Block, int count)
{
	MemoryAllocCount -= count;
	TraceLog(LOG_INFO, "[MEMORY] Allocation size -%d = %d", count, MemoryAllocCount);
	free(_Block);
}