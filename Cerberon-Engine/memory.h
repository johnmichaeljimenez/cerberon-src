#pragma once

size_t MemoryAllocCount;

void* MCalloc(size_t count, size_t size, char* label);
void* MRealloc(void* _Block, size_t count, size_t size, size_t prevsize, char* label);
void MFree(void* _Block, size_t count, size_t size, char* label);