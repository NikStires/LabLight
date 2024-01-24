//using System;
//using UniRx;
//using System.Collections.Generic;
//using System.Threading.Tasks;

///// <summary>
///// Originally known as Api 
///// Implements IDataProvider interface for accessing available procedures and updating runtime state
///// </summary>
//public class HttpDataProvider : IWorkspaceProvider, IProcedureDataProvider
//{
//    [Serializable]
//    class RequestSetStep
//    {
//        public string id;
//        public int step;
//    }

//    private IHttp http;
//    public HttpDataProvider(IHttp http)
//    {
//        this.http = http;
//    }

//    public void SetStep(string deviceId, int step)
//    {
//        http.PostJson(Config.GetResourcePath("/state/step"), new RequestSetStep()
//        {
//            id = deviceId,
//            step = step         
//        }).Subscribe();
//    }

//    [Serializable]
//    class RequestSetMaster
//    {
//        public string master;
//    }
//    public void SetMaster(string master)
//    {
//        // Do not assume self as master until server confirmation
//        http.PostJson(Config.GetResourcePath("/state/master"), new RequestSetMaster()
//        {
//            master = master
//        }).Subscribe();
//    }

//    [Serializable]
//    class RequestSetProcedure
//    {
//        public string id;
//        public string name;
//    }
//    public void SetProcedure(string deviceId, string procedureName)
//    {
//        http.PostJson(Config.GetResourcePath("/state/procedure"), new RequestSetProcedure()
//        {
//            id = deviceId,
//            name = procedureName
//        }).Subscribe();
//    }

//    public IObservable<WorkspaceFrame> GetWorkspace()
//    {
//        return http.Get(Config.GetResourcePath("/state/workspace")).Select(jsonString =>
//        {
//            try
//            {
//                return Parsers.ParseWorkspace(jsonString);
//            }
//            catch (Exception e)
//            {
//                //ServiceRegistry.Logger.LogError("Parsing workspace definition " + e.ToString());
//                throw;
//            }
//        });
//    }

//    public Task<List<ProcedureDescriptor>> GetProcedureList()
//    {
//        return http.Get(Config.GetResourcePath("/procedure/index.json")).Select(jsonString =>
//        {
//            try
//            {
//                return Parsers.ParseProcedures(jsonString);
//            }
//            catch (Exception e)
//            {
//                //ServiceRegistry.Logger.LogError("Could not create procedures " + e.ToString());
//                throw;
//            }
//        }).ToTask();
//    }

//    public IObservable<ProcedureDefinition> GetOrCreateProcedureDefinition(string procedureName)
//    {
//        // RS TODO If we are going to use this again the creation part should be implemented

//        var basePath = "/procedure/" + procedureName;
//        return http.Get(Config.GetResourcePath(basePath + "/index.json")).Select(jsonString =>
//        {
//            try
//            {
//                var procedure = Parsers.ParseProcedure(jsonString);
//                procedure.mediaBasePath = basePath;
//                return procedure;
//            }
//            catch (Exception e)
//            {
//                //ServiceRegistry.Logger.LogError("Parsing protocol definition " + e.ToString());
//                throw;
//            }
//        });
//    }

//    public void SaveProcedureDefinition(string procedureName, ProcedureDefinition procedure)
//    {
//        throw new NotImplementedException();
//    }
//}