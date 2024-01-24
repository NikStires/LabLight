using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Implements IDataProvider interface but delegate to multiple FileDataProvider implementations
/// </summary>
public class AggregateFileDataProvider : IProcedureDataProvider
{
    private List<IProcedureDataProvider> procedureDataProviders;

    /// <summary>
    /// A map of which provider can provide the given procedure names
    /// </summary>
    private Dictionary<string, IProcedureDataProvider> procedureLookup = new Dictionary<string, IProcedureDataProvider>();

    public AggregateFileDataProvider(List<IProcedureDataProvider> providers)
    {
        procedureDataProviders = providers;
    }

    public async Task<List<ProcedureDescriptor>> GetProcedureList()
    {
        procedureLookup.Clear();

        List<ProcedureDescriptor> list = new List<ProcedureDescriptor>();
        foreach (var pd in procedureDataProviders)
        {
            var procs = await pd.GetProcedureList();

            list.AddRange(procs);

            foreach (var proc in procs)
            {
                procedureLookup.Add(proc.name, pd);
            }
        }

        return list;
    }
    
    public IObservable<ProcedureDefinition> GetOrCreateProcedureDefinition(string procedureName)
    {
        IProcedureDataProvider pv;
        if (procedureLookup.TryGetValue(procedureName, out pv))
        {
            return pv.GetOrCreateProcedureDefinition(procedureName);
        }
        return null;
    }

    //public ProcedureDefinition GetOrCreateProcedureDefinition(string procedureName)
    //{
    //    IProcedureDataProvider pv;
    //    if(procedureLookup.TryGetValue(procedureName, out pv))
    //    {
    //        return pv.GetOrCreateProcedureDefinition(procedureName);
    //    }
    //    return null;
    //}

    public void SaveProcedureDefinition(string procedureName, ProcedureDefinition procedure)
    {
        throw new NotImplementedException();
    }
}