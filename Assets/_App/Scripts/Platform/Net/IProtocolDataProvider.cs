using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Interface for accessing protocol summary and the actual protocol definition
/// </summary>
public interface IProtocolDataProvider
{
    /// <summary>
    /// Retrieve list of available protocols as short ProtocolItem
    /// </summary>
    /// <returns></returns>
    public Task<List<ProtocolDescriptor>> GetProtocolList();
    
    /// <summary>
    /// Retrieve complete ProtocolDefinition
    /// </summary>
    /// <param name="protocolName"></param>
    /// <returns></returns>
    IObservable<ProtocolDefinition> GetOrCreateProtocolDefinition(ProtocolDescriptor protocolDescription);

    /// <summary>
    /// Save the protocol 
    /// </summary>
    /// <param name="protocolName"></param>
    /// <param name="protocol"></param>
    void SaveProtocolDefinition(string protocolName, ProtocolDefinition protocol);

    //void DeleteProtocolDefinition(string protocolName);
}