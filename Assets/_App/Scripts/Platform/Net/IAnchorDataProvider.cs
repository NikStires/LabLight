using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Interface for accessing anchor data
/// </summary>
public interface IAnchorDataProvider
{
    IObservable<AnchorData> GetOrCreateAnchorData();

    void SaveAnchorData(AnchorData anchorData);
}