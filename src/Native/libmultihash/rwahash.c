#include "rwahash.h"
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <stdio.h>

#include "sha3/sph_jh.h"
#include "sha3/sph_bmw.h"
#include "sha3/sph_skein.h"
#include "sha3/sph_cubehash.h"
#include "sha3/sph_luffa.h"
#include "sha3/sph_shavite.h"
#include "sha3/sph_simd.h"
#include "sha3/sph_echo.h"
#include "sha3/sph_hamsi.h"
#include "sha3/sph_fugue.h"
#include "sha3/sph_whirlpool.h"
#include "sha3/sph_shabal.h"
#include "sha3/sph_sha2.h"
#include "sha3/gost_streebog.h"
#include "Lyra2.h"

void rwahash_hash(const char* input, char* output, uint32_t len)
{
	unsigned char hash[128] = { 0 };
	unsigned char hashA[64] = { 0 };
	unsigned char hashB[64] = { 0 };

	sph_cubehash512_context  ctx_cubehash;	
	sph_luffa512_context     ctx_luffa;
	sph_gost512_context      ctx_gost;
	sph_echo512_context      ctx_echo;		
	sph_simd512_context      ctx_simd;	
	sph_shavite512_context   ctx_shavite;
	sph_hamsi512_context     ctx_hamsi;
	sph_fugue512_context     ctx_fugue;
	sph_whirlpool_context    ctx_whirlpool; 
	sph_skein512_context     ctx_skein;
	sph_bmw512_context       ctx_bmw;			
	sph_sha256_context       ctx_sha;	
	sph_shabal512_context    ctx_shabal;	
	sph_jh512_context        ctx_jh;

	sph_cubehash512_init(&ctx_cubehash);
	sph_cubehash512(&ctx_cubehash, input, len);
	sph_cubehash512_close(&ctx_cubehash, (void*)hashB);

	LYRA2(&hashA[ 0], 32, &hashB[ 0], 32, &hashB[ 0], 32, 1, 8, 8);
	LYRA2(&hashA[32], 32, &hashB[32], 32, &hashB[32], 32, 1, 8, 8);

	sph_luffa512_init(&ctx_luffa);
	sph_luffa512(&ctx_luffa, (const void*) hashA, 64);
	sph_luffa512_close (&ctx_luffa, hash);	

	if (hash[0] & 1) 
	{
		sph_gost512_init(&ctx_gost);
		sph_gost512(&ctx_gost, (const void*)hash, 64);
		sph_gost512_close(&ctx_gost, (void*)hash);
	} 
	else 
	{
		sph_echo512_init(&ctx_echo);
		sph_echo512(&ctx_echo, (const void*)hash, 64);
		sph_echo512_close(&ctx_echo, (void*)hash);

		sph_echo512_init(&ctx_echo);
		sph_echo512(&ctx_echo, (const void*)hash, 64);
		sph_echo512_close(&ctx_echo, (void*)hash);
	}

	sph_simd512_init(&ctx_simd);
	sph_simd512(&ctx_simd, (const void*) hash, 64);
	sph_simd512_close(&ctx_simd, hash);

	sph_echo512_init(&ctx_echo);
	sph_echo512(&ctx_echo, (const void*) hash, 64);
	sph_echo512_close(&ctx_echo, hash);	

	sph_cubehash512_init(&ctx_cubehash);
	sph_cubehash512(&ctx_cubehash, (const void*) hash, 64);
	sph_cubehash512_close(&ctx_cubehash, hash);

	sph_shavite512_init(&ctx_shavite);
	sph_shavite512(&ctx_shavite, (const void*) hash, 64);
	sph_shavite512_close(&ctx_shavite, hash);

	sph_hamsi512_init(&ctx_hamsi);
	sph_hamsi512(&ctx_hamsi, (const void*) hash, 64);
	sph_hamsi512_close(&ctx_hamsi, hash);

	sph_fugue512_init(&ctx_fugue);
	sph_fugue512(&ctx_fugue, (const void*) hash, 64);
	sph_fugue512_close(&ctx_fugue, hash);
	
	sph_whirlpool_init(&ctx_whirlpool);
	sph_whirlpool (&ctx_whirlpool, (const void*) hash, 64);
	sph_whirlpool_close(&ctx_whirlpool, hash);
	
	sph_sha256_init(&ctx_sha);
	sph_sha256(&ctx_sha, (const void*)hash, 64);
	sph_sha256_close(&ctx_sha, (void*)hash);
		
	sph_sha256_init(&ctx_sha);
	sph_sha256(&ctx_sha, (const void*)hash, 64);
	sph_sha256_close(&ctx_sha, (void*)hash);		
	
	sph_shabal512_init(&ctx_shabal);
	sph_shabal512(&ctx_shabal, (const void*) hash, 64);
	sph_shabal512_close(&ctx_shabal, hash);
	
	sph_skein512_init(&ctx_skein);
	sph_skein512(&ctx_skein, (const void*)hash, 64);
	sph_skein512_close(&ctx_skein, (void*)hash);	

	sph_bmw512_init(&ctx_bmw);
	sph_bmw512(&ctx_bmw, (const void*) hash, 64);
	sph_bmw512_close(&ctx_bmw, hash);	
	
	sph_jh512_init(&ctx_jh);
	sph_jh512(&ctx_jh, (const void*) hash, 64);
	sph_jh512_close(&ctx_jh, hash);	
	      
	for (int i=0; i<32; i++)
		hash[i] ^= hash[i+32];

	memcpy(output, hash, 32);
}
