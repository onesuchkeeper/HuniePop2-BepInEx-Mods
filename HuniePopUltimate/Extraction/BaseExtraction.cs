using System;
using AssetStudio.Extractor;
using Hp2BaseMod;

namespace HuniePopUltimate;

public class BaseExtraction : IDisposable
{
    protected Extractor _extractor;

    private bool _populated = true;

    public BaseExtraction(string dataPath,
        string assemblyPath)
    {
        _extractor = Extractor.Load(dataPath, assemblyPath);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_populated)
        {
            if (disposing)
            {
                _extractor.Dispose();
            }

            _populated = false;
        }
    }
}
