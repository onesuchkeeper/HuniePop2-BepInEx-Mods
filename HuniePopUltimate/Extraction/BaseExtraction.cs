using System;
using ETAR.AssetStudioPlugin.Extractor;

namespace HuniePopUltimate;

public class BaseExtraction : IDisposable
{
    protected Extractor _extractor;

    private bool _populated = true;

    public BaseExtraction(string dataPath,
        string assemblyPath)
    {
        _extractor = Extractor.Create(dataPath, assemblyPath);
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
