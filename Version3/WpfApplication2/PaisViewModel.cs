
namespace WpfApplication2
{
    using com.cairone.odataexample;
    using Microsoft.OData.Client;
    using Service;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Telerik.Windows.Controls;
    public class PaisViewModel : ViewModelBase
    {
        private const string PAIS_ENTITY_NAME = "Paises";

        private readonly ODataExampleService serviceContext = null;

        private ObservableCollection<Pais> gridDataSource = null;

        private TrackableEntities entityList = null;

        //private IList<Pais> newItems = null;
        //private IList<Pais> updateItems = null;
        //private IList<Pais> deleteItems = null;

        public PaisViewModel()
        {
            // it should be a Singleton
            // or injected to this constructor
            this.serviceContext = new ODataExampleService();

            this.gridDataSource = new ObservableCollection<Pais>();
            this.LoadPaises();

            //this.newItems = new List<Pais>();
            //this.updateItems = new List<Pais>();
            //this.deleteItems = new List<Pais>();

            this.entityList = new TrackableEntities(PAIS_ENTITY_NAME);
        }

        private void LoadPaises()
        {
            int i;

            if (this.gridDataSource == null)
            {
                return;
            }

            /*
            for (i = 0; i < 20; i++)
            {
                this.gridDataSource.Add(new Pais() { Id = (i + 1), Name = "Pais " + (i + 1) });
            }*/
            
            this.gridDataSource = this.serviceContext.LoadPaises();
        }

        public ObservableCollection<Pais> ItemSource
        {
            get
            {
                //var list = this.gridDataSource.OrderBy(x => x.nombre).ToList();

                //return new ObservableCollection<Pais>(list);
                //return this.serviceContext.LoadPaises();
                return this.gridDataSource;
            }
        }

        public int TotalChanges
        {
            get
            {
                int totalCount = 0;

                //totalCount += this.newItems.Count;
                //totalCount += this.updateItems.Count;
                //totalCount += this.deleteItems.Count;

                totalCount = this.entityList.Count;

                return totalCount;
            }
        }

        public DataServiceContext Context
        {
            get
            {
                return this.serviceContext.Context;
            }
        }
        public IList<string> Errors { get; private set; }

        public void Insert(Pais pais)
        {
            if (pais == null)
            {
                return;
            }

            //this.gridDataSource.Add(pais);

            this.entityList.ProcessEntity(pais, TrackableEntity.Operation.Insert);

            // not needed any longer
            //this.newItems.Add(pais);
        }
        public void Update(Pais pais)
        {
            if (pais == null)
            {
                return;
            }

            /*
            Pais thisPais = this.gridDataSource.Where(p => p.id == pais.id).SingleOrDefault();
            if (thisPais != null)
            {
                thisPais.nombre = pais.nombre;
            }*/

            this.entityList.ProcessEntity(pais, TrackableEntity.Operation.Update);


            // not needed any longer
            //this.updateItems.Add(pais);

        }

        public void RemoveAll(IList<Pais> paises)
        {
            if (paises == null)
            {
                return;
            }

            // No need for search
            // param is coming off the grid view
            /*
            Pais thisPais = this.gridDataSource.Where(p => p.Id == pais.Id).SingleOrDefault();
            if (thisPais != null)
            {
                this.gridDataSource.Remove(thisPais);
            }*/

            foreach (Pais item in paises)
            {
                //this.gridDataSource.Remove(item);

                this.entityList.ProcessEntity(item, TrackableEntity.Operation.Delete);

                // not needed any longer
                //this.deleteItems.Add(item);
            }
        }

        // OData Batch Processing
        // Send items to OData Service
        // http://docs.oasis-open.org/odata/odata/v4.0/errata03/os/complete/part1-protocol/odata-v4.0-errata03-os-part1-protocol-complete.html#_Toc453752313
        public void SubmitChanges()
        {
            // This needs refactoring, but for now ....
            //this.serviceContext.SavePaises(this.newItems);
            //this.serviceContext.UpdatePaises(this.updateItems);
            //this.serviceContext.DeletePaises(this.deleteItems);

            this.serviceContext.ProcessOperation(this.entityList);

            this.Errors = this.serviceContext.SubmitChanges();

            this.entityList.Clear();

            //this.newItems.Clear();
            //this.updateItems.Clear();
            //this.deleteItems.Clear();
        }

        public void RejectChanges()
        {
            //this.newItems.Clear();
            //this.updateItems.Clear();
            //this.deleteItems.Clear();

            this.entityList.Clear();
        }
    }
}