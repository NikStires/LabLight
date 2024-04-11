using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Interface for accessing procedure summary and the actual procedure definition
/// </summary>
public interface IProcedureDataProvider
{
    /// <summary>
    /// Retrieve list of available procedures as short ProcedureItem
    /// </summary>
    /// <returns></returns>
    public Task<List<ProcedureDescriptor>> GetProcedureList();
    
    /// <summary>
    /// Retrieve complete ProcedureDefinition
    /// </summary>
    /// <param name="procedureName"></param>
    /// <returns></returns>
    IObservable<ProcedureDefinition> GetOrCreateProcedureDefinition(string procedureName);

    /// <summary>
    /// Save the procedure 
    /// </summary>
    /// <param name="procedureName"></param>
    /// <param name="procedure"></param>
    void SaveProcedureDefinition(string procedureName, ProcedureDefinition procedure);
}