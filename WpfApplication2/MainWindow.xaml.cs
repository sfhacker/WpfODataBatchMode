
namespace WpfApplication2
{
    using com.cairone.odataexample;
    using Microsoft.OData.Client;
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Data;
    using Telerik.Windows;
    using Telerik.Windows.Controls;

    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new PaisViewModel();
        }

        private void gridView_Loaded(object sender, RoutedEventArgs e)
        {
            PaisViewModel paisViewModel = null;

            paisViewModel = (PaisViewModel)this.DataContext;

            this.gridView.IsReadOnly = true;
            this.gridView.AutoGenerateColumns = false;
            this.gridView.CanUserInsertRows = false;

            this.gridView.ItemsSource = paisViewModel.ItemSource;

            //MyNorthwindEntities ordersContext = new MyNorthwindEntities();
            //DataServiceQuery<Pais> paisQuery = paisViewModel.ItemSource;
            //QueryableDataServiceCollectionView<Pais> ordersView = new QueryableDataServiceCollectionView<Pais>(this.ordersContext, paisQuery);

            this.dataPager.SetBinding(RadDataPager.SourceProperty, new Binding("Items") { Source = this.gridView });

            //this.gridView.IsBusy = false;
        }

        private void gridView_NewItem(object sender, Telerik.Windows.Controls.GridView.GridViewAddingNewEventArgs e)
        {
            Pais newPais = new Pais()
            {
                id = 1,
                nombre = "Argentina"
            };

            e.NewObject = newPais;
        }

        private void btnSaveAll_Click(object sender, RoutedEventArgs e)
        {
            PaisViewModel paisViewModel = null;

            MessageBoxResult rst;

            paisViewModel = (PaisViewModel)this.DataContext;

            int totalChanges = paisViewModel.TotalChanges;

            if (totalChanges < 1)
            {
                System.Windows.MessageBox.Show("No se ha realizado ningun cambio.", "Pais::Guardar cambios", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }
            // it is read-only, remember?
            //this.gridView.BeginInsert();
            rst = System.Windows.MessageBox.Show(string.Format("Se guardaran todos los cambios realizados hasta el momento: {0}. Desea proceder?", paisViewModel.TotalChanges), "Pais::Guardar cambios", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (rst == MessageBoxResult.Yes)
            {
                paisViewModel.SubmitChanges();

                foreach (string item in paisViewModel.Errors)
                {
                    this.statusInfo.Text += item + " * ";
                }
            }
        }

        private void btnCancelAll_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult rst;

            PaisViewModel viewModel = (PaisViewModel)this.DataContext;

            int totalChanges = viewModel.TotalChanges;

            if (totalChanges < 1)
            {
                System.Windows.MessageBox.Show("No se ha realizado ningun cambio.", "Pais::Cancelar cambios", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }

            rst = System.Windows.MessageBox.Show("Todos los cambios realizados seran perdidos. Desea proceder?", "Pais::Cancelar cambios", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (rst == MessageBoxResult.Yes)
            {
                viewModel.RejectChanges();

                //this.gridView.Rebind();
                //this.gridView.ItemsSource = null;

                // I want to force a Reload!!!!!
                this.gridView.ItemsSource = null;
                this.gridView.Items.Clear();

                // we need a reload method somewhere
                // it does not work otherwise!
                var alas = new PaisViewModel();

                this.gridView.ItemsSource = alas.ItemSource;         // viewModel.ItemSource;

                //this.gridView.IsBusy = false;
                //this.gridView.CancelEdit();

                this.gridView.Items.Refresh();
            }
        }

        private void gridView_DataLoading(object sender, Telerik.Windows.Controls.GridView.GridViewDataLoadingEventArgs e)
        {
            this.gridView.IsBusy = true;
        }

        private void gridView_DataLoaded(object sender, System.EventArgs e)
        {
            this.gridView.IsBusy = false;
        }

        private void radContextMenu_ItemClick(object sender, RadRoutedEventArgs e)
        {
            PaisViewModel viewModel = null;

            string itemName = string.Empty;

            RadMenuItem item = e.OriginalSource as RadMenuItem;

            if (item.Header == null)
            {
                return;
            }

            viewModel = (PaisViewModel)this.DataContext;
            if (viewModel == null)
            {
                return;
            }

            itemName = item.Header.ToString();
            if (itemName.Contains("Nuevo"))
            {
                //viewModel.Insert(newPais);

                //this.gridView.AddNewRowPosition = SystemRowPosition.Top;

                Pais testPais = (Pais)this.gridView.Items.AddNew();
                //this.gridView.Items.AddNewItem(newPais);

                Random rnd = new Random();
                int paisId = rnd.Next(200, 300);  // 1 <= month < 13

                testPais.id = paisId;
                testPais.nombre = "Norway - The Northen Lights (" + paisId.ToString() + ")";

                this.gridView.Items.CommitNew();

                this.gridView.ScrollIntoView(testPais);
                this.dataPager.MoveToLastPage();

                viewModel.Insert(testPais);

                //this.gridView.Items.Refresh();

                return;
            }

            // http://docs.telerik.com/devtools/wpf/controls/radgridview/managing-data/how-to/edit-item-outside-gridview
            if (itemName.Contains("Modificar"))
            {
                if (this.gridView.SelectedItems.Count == 0)
                {
                    return;
                }

                if (this.gridView.SelectedItems.Count > 1)
                {
                    System.Windows.MessageBox.Show("Debe seleccionar un solo pais.", "Paises", MessageBoxButton.OK, MessageBoxImage.Stop);

                    return;
                }

                //var thisItem = this.gridView.SelectedItem;

                Pais thisPais = this.gridView.SelectedItem as Pais;
                if (thisPais == null)
                {
                    return;
                }

                //thisPais.Name += " * Modified!";
                //viewModel.Update(thisPais);

                /*Pais copyPais = new Pais();
                copyPais.id = thisPais.id;
                copyPais.nombre = thisPais.nombre;
                copyPais.prefijo = thisPais.prefijo;

                copyPais.nombre += " * Modified!";
                */

                //thisPais.nombre += " * Modified!";

                this.gridView.Items.EditItem(thisPais);
                // change the value
                thisPais.nombre += " * Modified!";
                this.gridView.Items.CommitEdit();

                viewModel.Update(thisPais);

                //this.gridView.Items.c
                //this.gridView.Items.Refresh();

                return;
            }

            // we could delete more than one, of course!
            if (itemName.Contains("Eliminar"))
            {
                if (this.gridView.SelectedItems.Count == 0)
                {
                    return;
                }

                IList<Pais> paisList = new List<Pais>();

                Pais thisPais = null;
                foreach (var row in this.gridView.SelectedItems)
                {
                    thisPais = (Pais)row;
                    if (thisPais != null)
                    {
                        paisList.Add(thisPais);
                    }
                }

                foreach (var row in paisList)
                {
                    this.gridView.Items.Remove(row);
                }

                viewModel.RemoveAll(paisList);

                //this.gridView.Rebind();

                return;
            }

        }
    }
}