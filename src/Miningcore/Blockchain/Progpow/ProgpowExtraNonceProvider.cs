namespace Miningcore.Blockchain.Progpow;

public class EvrmoreExtraNonceProvider : ExtraNonceProviderBase
{
    public EvrmoreExtraNonceProvider(string poolId, byte? clusterInstanceId) : base(poolId, EvrmoreConstants.ExtranoncePlaceHolderLength, clusterInstanceId)
    {
    }
}

public class FiroExtraNonceProvider : ExtraNonceProviderBase
{
    public FiroExtraNonceProvider(string poolId, byte? clusterInstanceId) : base(poolId, FiroConstants.ExtranoncePlaceHolderLength, clusterInstanceId)
    {
    }
}

public class RavencoinExtraNonceProvider : ExtraNonceProviderBase
{
    public RavencoinExtraNonceProvider(string poolId, byte? clusterInstanceId) : base(poolId, RavencoinConstants.ExtranoncePlaceHolderLength, clusterInstanceId)
    {
    }
}