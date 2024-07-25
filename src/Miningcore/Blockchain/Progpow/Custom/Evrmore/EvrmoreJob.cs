using System.Globalization;
using System.Text;
using Miningcore.Blockchain.Bitcoin;
using Miningcore.Blockchain.Bitcoin.DaemonResponses;
using Miningcore.Crypto;
using Miningcore.Crypto.Hashing.Progpow;
using Miningcore.Extensions;
using Miningcore.Stratum;
using Miningcore.Time;
using Miningcore.Util;
using NBitcoin;
using NBitcoin.DataEncoders;
using Newtonsoft.Json.Linq;
using NLog;
using Transaction = NBitcoin.Transaction;

namespace Miningcore.Blockchain.Progpow.Custom.Evrmore;

public class EvrmoreJob : ProgpowJob
{
    public override (Share Share, string BlockHex) ProcessShareInternal(ILogger logger,
        StratumConnection worker, ulong nonce, string inputHeaderHash, string mixHash)
    {
        var context = worker.ContextAs<ProgpowWorkerContext>();
        var extraNonce1 = context.ExtraNonce1;

        // build coinbase
        var coinbase = SerializeCoinbase(extraNonce1);
        Span<byte> coinbaseHash = stackalloc byte[32];
        coinbaseHasher.Digest(coinbase, coinbaseHash);

        // hash block-header
        var headerBytes = SerializeHeader(coinbaseHash);
        Span<byte> headerHash = stackalloc byte[32];
        headerHasher.Digest(headerBytes, headerHash);
        headerHash.Reverse();

        var headerHashHex = headerHash.ToHexString();

        if(headerHashHex != inputHeaderHash)
            throw new StratumException(StratumError.MinusOne, $"bad header-hash");

        if(!progpowHasher.Compute(logger, (int) BlockTemplate.Height, headerHash.ToArray(), nonce, out var mixHashOut, out var resultBytes))
            throw new StratumException(StratumError.MinusOne, "bad hash");

        if(mixHash != mixHashOut.ToHexString())
            throw new StratumException(StratumError.MinusOne, $"bad mix-hash");

        resultBytes.ReverseInPlace();
        mixHashOut.ReverseInPlace();

        var resultValue = new uint256(resultBytes);
        var resultValueBig = resultBytes.AsSpan().ToBigInteger();
        // calc share-diff
        var shareDiff = (double) new BigRational(EvrmoreConstants.Diff1, resultValueBig) * shareMultiplier;
        var stratumDifficulty = context.Difficulty;
        var ratio = shareDiff / stratumDifficulty;

        // check if the share meets the much harder block difficulty (block candidate)
        var isBlockCandidate = resultValue <= blockTargetValue;

        // test if share meets at least workers current difficulty
        if(!isBlockCandidate && ratio < 0.99)
        {
            // check if share matched the previous difficulty from before a vardiff retarget
            if(context.VarDiff?.LastUpdate != null && context.PreviousDifficulty.HasValue)
            {
                ratio = shareDiff / context.PreviousDifficulty.Value;

                if(ratio < 0.99)
                    throw new StratumException(StratumError.LowDifficultyShare, $"low difficulty share ({shareDiff})");

                // use previous difficulty
                stratumDifficulty = context.PreviousDifficulty.Value;
            }

            else
                throw new StratumException(StratumError.LowDifficultyShare, $"low difficulty share ({shareDiff})");
        }

        var result = new Share
        {
            BlockHeight = BlockTemplate.Height,
            NetworkDifficulty = Difficulty,
            Difficulty = stratumDifficulty / shareMultiplier,
        };

        if(!isBlockCandidate)
        {
            return (result, null);
        }

        result.IsBlockCandidate = true;
        
        var nonceBytes = (Span<byte>) nonce.ToString("X").HexToReverseByteArray();
        var mixHashBytes = (Span<byte>) mixHash.HexToReverseByteArray();
        // concat headerBytes, nonceBytes and mixHashBytes
        Span<byte> headerBytesNonceMixHasBytes = stackalloc byte[headerBytes.Length + nonceBytes.Length + mixHashBytes.Length];
        headerBytes.CopyTo(headerBytesNonceMixHasBytes);
        var offset = headerBytes.Length;
        nonceBytes.CopyTo(headerBytesNonceMixHasBytes[offset..]);
        offset += nonceBytes.Length;
        mixHashBytes.CopyTo(headerBytesNonceMixHasBytes[offset..]);
        
        Span<byte> blockHash = stackalloc byte[32];
        blockHasher.Digest(headerBytesNonceMixHasBytes, blockHash);
        result.BlockHash = blockHash.ToHexString();

        var blockBytes = SerializeBlock(headerBytes, coinbase, nonce, mixHashOut);
        var blockHex = blockBytes.ToHexString();

        return (result, blockHex);
    }

}