

namespace WpfApplication2.Service
{
    using com.cairone.odataexample;
    using Filters;
    using Microsoft.OData.Client;
    using Microsoft.OData.Core;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Text;

    public sealed class ODataExampleService
    {
        private const string PAIS_ENTITY_NAME = "Paises";
        private const string PROVINCIA_ENTITY_NAME = "Provincias";
        private const string LOCALIDAD_ENTITY_NAME = "Localidades";

        private string serviceUri = string.Empty;

        private string errmsg = string.Empty;

        private readonly ODataExample container = null;

        // This class could be a singleton
        // Or should be called from App.xaml.cs and use constructor injection
        public ODataExampleService()
        {
            try
            {
                this.serviceUri = ConfigurationManager.AppSettings["webServiceURL"].ToString();
                if (string.IsNullOrEmpty(serviceUri))
                {
                    throw new ArgumentNullException("webServiceURL", "Invalid value");
                }

                // http://odata.github.io/odata.net/04-06-use-client-hooks-in-odata-client/
                this.container = new ODataExample(new Uri(serviceUri));

                this.SetDefaults();
            }
            catch (Exception ex)
            {
                this.errmsg = ex.ToString();
            }
        }

        // This could be an extension method
        public ODataExampleService(string uri)
        {
            throw new NotImplementedException();
        }

        public DataServiceContext Context
        {
            get
            {
                return this.container;
            }
        }
        public ObservableCollection<Pais> LoadPaises()
        {
            try
            {
                var query = this.container.Paises;

                return new ObservableCollection<Pais>(query);
            }
            catch (Exception ex)
            {
                this.errmsg = ex.Message;
            }

            // null or an empty collection ?
            return null;
        }

        public ObservableCollection<Provincia> LoadStatesByCountry(Pais pais)
        {
            ObservableCollection<Provincia> rst = new ObservableCollection<Provincia>();

            try
            {
                // I do not understand why we have to do it this way.
                //var query = (from states in this.container.Provincias.Expand(w => w.Pais)
                //                select states
                //                ).ToList();
                //var rst = query.Where(p => p.Pais != null && p.Pais.Id == pais.Id);

                // If we do not use 'Expand', state.Pais is always null
                foreach (var state in this.container.Provincias)
                {
                    //if ( (state.Pais != null) && (state.Pais.Id == pais.Id) )
                    if (state.paisId == pais.id)
                    {
                        rst.Add(state);
                    }
                }

                return rst;
            }
            catch (Exception ex)
            {
                this.errmsg = ex.Message;
            }

            // null or an empty collection ?
            return rst;
        }

        public ObservableCollection<Localidad> LoadCitiesByCountryAndState(Pais pais, Provincia provincia)
        {
            try
            {
                var query = this.container.Localidades.ToList();
                var qlist = query.Where(c => c.paisId == pais.id && c.provinciaId == provincia.id);

                return new ObservableCollection<Localidad>(qlist);
            }
            catch (Exception ex)
            {
                this.errmsg = ex.Message;
            }

            // null or an empty collection ?
            return new ObservableCollection<Localidad>();
        }

        // OData Batch Processing
        private void SavePaises(IList<Pais> paises)
        {
            if (paises == null)
            {
                return;
            }
            if (paises.Count == 0)
            {
                return;
            }
            foreach (Pais item in paises)
            {
                this.container.AddToPaises(item);
                //this.container.AddObject(PAIS_ENTITY_NAME, item);
            }
        }

        // OData Batch Processing
        private void UpdatePaises(IList<Pais> paises)
        {
            if (paises == null)
            {
                return;
            }
            if (paises.Count == 0)
            {
                return;
            }
            foreach (Pais item in paises)
            {
                this.container.UpdateObject(item);
            }
        }

        // OData Batch Processing
        private void DeletePaises(IList<Pais> paises)
        {
            if (paises == null)
            {
                return;
            }
            if (paises.Count == 0)
            {
                return;
            }
            foreach (Pais item in paises)
            {
                this.container.DeleteObject(item);
            }
        }

        public void ProcessOperation(TrackableEntities list)
        {
            if (list == null)
            {
                return;
            }

            foreach (TrackableEntity item in list.Items)
            {
                object entity = item.Entity;

                Type entityType = entity.GetType();

                switch (item.OperationType)
                {
                    case TrackableEntity.Operation.Insert:
                        // I need to make it generic!
                        this.container.AddObject(list.Name, entity);
                    break;

                    case TrackableEntity.Operation.Update:
                        this.container.UpdateObject(entity);
                    break;

                    case TrackableEntity.Operation.Delete:
                        this.container.DeleteObject(entity);
                    break;
                }
            }
        }
        // Needs refactoring
        public IList<string> SubmitChanges()
        {
            IList<string> rst = new List<string>();
            string msg = string.Empty;

            this.container.Format.UseJson();

            // Throws a funny exception!
            // SaveChangesOptions.BatchWithSingleChangeset

            try
            {
                DataServiceResponse response = this.container.SaveChanges(SaveChangesOptions.ReplaceOnUpdate);

                // Makes no sense if not 'SaveChangesOptions.BatchWithSingleChangeset'
                if (!response.IsBatchResponse)
                {
                    // Some error here
                }

                int i = 0;
                Exception ex = null;
                
                foreach (OperationResponse individualResponse in response)
                {
                    ex = individualResponse.Error;

                    //Console.WriteLine("Operation {0} status code = {1}", i++, individualResponse.StatusCode);

                    msg = string.Format("Operation {0} status code = {1}", i++, individualResponse.StatusCode);
                    if (ex != null)
                    {
                        msg += ". " + ex.Message;
                    }

                    rst.Add(msg);
                }

                return rst;
            }
            catch (DataServiceRequestException ex) // if Batch mode as well
            {
                // always null
                //var new_Ex = ex.GetBaseException() as Microsoft.OData.Client.DataServiceRequestException;

                if (ex.InnerException != null)
                {
                    string jsonResponse = ex.InnerException.Message;

                    // Some messages are not json formatted
                    try
                    {
                        ODataServiceSdlResponse response = JsonConvert.DeserializeObject<ODataServiceSdlResponse>(jsonResponse);
                        msg = response.Error.Message;
                    }
                    catch (Exception jsonEx)
                    {
                        msg = jsonResponse;
                    }

                    //msg = string.Format("Request Exception: {0}", ex.InnerException.Message);
                    
                }
                else
                {
                    msg = string.Format("Request Exception: {0}", ex.Message);
                }
            }
            catch (DataServiceClientException dsce)
            {
                //msg = string.Format("Client Exception, Status Code - {0}", dsce.StatusCode.ToString());
                var new_Ex = dsce.GetBaseException() as Microsoft.OData.Client.DataServiceClientException;

                string jsonResponse = new_Ex.Message;

                switch (new_Ex.StatusCode)
                {
                    case 401:
                        ODataServiceSpringResponse response401 = JsonConvert.DeserializeObject<ODataServiceSpringResponse>(jsonResponse);
                        msg = response401.Message;
                        break;
                    default:
                        ODataServiceSdlResponse response = JsonConvert.DeserializeObject<ODataServiceSdlResponse>(jsonResponse);
                        msg = response.Error.Message;

                        if (msg.ToLower().Contains(value: "access is denied"))
                        {
                            msg = "ACCESO DENEGADO";
                        }

                        break;
                }
            }
            catch (DataServiceQueryException dsqe)
            {
                msg = string.Format("Query Exception, Status code - {0}", dsqe.Response.StatusCode.ToString());
            }
            catch (InvalidOperationException ex)
            {
                var new_Ex = ex.GetBaseException();
                var ex_Data = ex.Data;

                if (ex.InnerException != null)
                {
                    msg = string.Format("Transaction Exception: {0}", ex.InnerException.Message);
                }
                else
                {
                    msg = string.Format("Transaction Exception: {0}", ex.Message);
                }
            }

            rst.Add(msg);

            return rst;
            // it works fine!
            // if not 'SaveChangesOptions.BatchWithSingleChangeset'
            //this.container.BeginSaveChanges(SaveChangesOptions.ReplaceOnUpdate, asyncCallback, this.container);

            //DataServiceResponse response = await this.container.SaveChangesAsync(SaveChangesOptions.BatchWithSingleChangeset | SaveChangesOptions.ReplaceOnUpdate);
            /*
            if (!response.IsBatchResponse)
            {
                // Some error here
            }

            int i = 0;
            foreach (OperationResponse individualResponse in response)
            {
                Console.WriteLine("Operation {0} status code = {1}", i++, individualResponse.StatusCode);
            }*/

            //return true;
        }

        // not used
        private bool InsertCountry(Pais pais)
        {
            this.container.AddObject(PAIS_ENTITY_NAME, pais);
            this.container.AddObject(PAIS_ENTITY_NAME, pais);
            this.container.AddObject(PAIS_ENTITY_NAME, pais);

            this.container.BeginSaveChanges(asyncCallback, this.container);

            return true;
        }

        // not used
        private bool UpdateCountry(Pais pais)
        {
            // Validations done in the View Model
            this.container.UpdateObject(pais);

            this.container.BeginSaveChanges(asyncCallback, this.container);

            return true;
        }

        private bool DeleteCountry(Pais pais)
        {
            this.container.DeleteObject(pais);

            this.container.BeginSaveChanges(asyncCallback, this.container);

            return true;
        }

        // Add here all the default (or initial) options
        private void SetDefaults()
        {
            // Only PUT
            this.container.SaveChangesDefaultOptions = Microsoft.OData.Client.SaveChangesOptions.ReplaceOnUpdate;

            // Can we use this one instead of 'SendingRequest2'
            //this.container.Credentials = System.Net.CredentialCache.DefaultCredentials;

            // Sending username and password to OData service
            // Uncomment when needed
            this.container.SendingRequest2 += Container_SendingRequest2;

            //this.container.Configurations.RequestPipeline.OnMessageCreating += Container_OnMessageCreating;
        }

        private DataServiceClientRequestMessage Container_OnMessageCreating(DataServiceClientRequestMessageArgs arg)
        {
            // it does not work!
            return new CustomizedRequestMessage(arg, null, new Dictionary<string, string>()
            {
            {"Content-Type", "application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false;charset=utf-8"},
            {"Preference-Applied", "odata.include-annotations=\"*\""}
            });
        }

        private void Container_SendingRequest2(object sender, SendingRequest2EventArgs e)
        {
            string authBasic;

            //authBasic = string.Format("Basic {0}", this.GenerateAuthHeader("hacker", "123456"));

            //e.RequestMessage.SetHeader(headerName: "Authorization", headerValue: authBasic);
            // Already included
            var headers = e.RequestMessage.Headers;

            //e.RequestMessage.SetHeader(headerName: "Content-Type", headerValue: "application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false;charset=utf-8");
        }

        private string GenerateAuthHeader(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            return Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", username, password)));
        }
        // Using this method for all entities (Pais, Provincia, etc) and all operations (e.g. Add, Delete, etc)
        private void asyncCallback(IAsyncResult result)
        {
            ODataExample svcContext = null;
            string msg = string.Empty;

            svcContext = result.AsyncState as ODataExample;

            try
            {
                // Complete the save changes operation and display the response.
                svcContext.EndSaveChanges(result);
                msg = string.Format("Transaction Completed: {0}", result.IsCompleted);
            }
            catch (DataServiceRequestException ex)
            {
                if (ex.InnerException != null)
                {
                    msg = string.Format("Request Exception: {0}", ex.InnerException.Message);
                }
                else
                {
                    msg = string.Format("Request Exception: {0}", ex.Message);
                }
            }
            catch (DataServiceClientException dsce)
            {
                msg = string.Format("Client Exception, Status Code - {0}", dsce.StatusCode.ToString());
            }
            catch (DataServiceQueryException dsqe)
            {
                msg = string.Format("Query Exception, Status code - {0}", dsqe.Response.StatusCode.ToString());
            }
            catch (InvalidOperationException ex)
            {
                if (ex.InnerException != null)
                {
                    msg = string.Format("Transaction Exception: {0}", ex.InnerException.Message);
                }
                else
                {
                    msg = string.Format("Transaction Exception: {0}", ex.Message);
                }

            }

            // this should not be here
            System.Windows.MessageBox.Show(msg);
        }

        private System.Net.ICredentials GenerateUserCredentials(string username, string password)
        {
            System.Net.ICredentials userCredentials = null;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            userCredentials = new System.Net.NetworkCredential(username, password);

            return userCredentials;
        }
    }

    internal class CustomizedRequestMessage : HttpWebRequestMessage
    {
        public string Response { get; set; }
        public Dictionary<string, string> CustomizedHeaders { get; set; }

        public CustomizedRequestMessage(DataServiceClientRequestMessageArgs args)
            : base(args)
        {
        }

        public CustomizedRequestMessage(DataServiceClientRequestMessageArgs args, string response, Dictionary<string, string> headers)
            : base(args)
        {
            this.Response = response;
            this.CustomizedHeaders = headers;
        }

        public override IODataResponseMessage GetResponse()
        {
            return new HttpWebResponseMessage(
                this.CustomizedHeaders,
                200,
                () =>
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes(this.Response);
                    return new MemoryStream(byteArray);
                });
        }
    }
}