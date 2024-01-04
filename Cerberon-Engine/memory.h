#pragma once

size_t MemoryAllocCount;

void* MCalloc(size_t count, size_t size);
void MFree(void* ptr);