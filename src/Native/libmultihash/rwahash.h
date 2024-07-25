#ifndef RWAHASH_H
#define RWAHASH_H

#ifdef __cplusplus
extern "C" {
#endif

#include <stdint.h>

void rwahash_hash(const char* input, char* output, uint32_t len);

#ifdef __cplusplus
}
#endif

#endif
