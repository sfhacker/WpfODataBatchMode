
namespace WpfApplication2
{
    using com.cairone.odataexample;
    using Filters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    using Telerik.Windows;
    using Telerik.Windows.Controls;
    using Telerik.Windows.Controls.GridView;

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

            // Data Service Context properties
            this.paisesDataSource.DataServiceContext = paisViewModel.Context;
            this.paisesDataSource.QueryName = "Paises";

            // if 'false', then the DataPager throws an exception
            this.paisesDataSource.AutoLoad = true;

            //this.paisesDataSource.Load();

            // Grid View
            //this.gridView.IsReadOnly = true;
            this.gridView.AutoGenerateColumns = false;
            //this.gridView.CanUserInsertRows = false;

            this.gridView.RowEditEnded += GridView_RowEditEnded;
            this.gridView.Deleting += GridView_Deleting;
            this.gridView.SelectionChanged += GridView_SelectionChanged;

            // Works ok
            this.gridView.ItemsSource = paisViewModel.ItemSource;

            // Works fine
            //this.gridView.SetBinding(RadGridView.ItemsSourceProperty, new Binding("DataView") { ElementName = "paisesDataSource" });
            

            // Works ok
            this.dataPager.SetBinding(RadDataPager.SourceProperty, new Binding("Items") { Source = this.gridView });

            // Works ok
            //this.dataPager.SetBinding(RadDataPager.SourceProperty, new Binding("DataView") { ElementName = "paisesDataSource" });

            // Put this in the view
            this.dataPager.PageSize = 5;

            //this.gridView.IsBusy = false;

            this.gridView.KeyboardCommandProvider = new CustomKeyboardCommandProvider(this.gridView);
        }

        private void GridView_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            //var newItem = e.AddedItems;
            //var removedItems = e.RemovedItems;
        }

        private void GridView_Deleting(object sender, GridViewDeletingEventArgs e)
        {
            PaisViewModel paisViewModel = null;

            paisViewModel = (PaisViewModel)this.DataContext;

            if (e.Items != null)
            {
                List<Pais> paises = e.Items.Cast<Pais>().ToList();

                paisViewModel.RemoveAll(paises);
            }
            
        }

        private void GridView_RowEditEnded(object sender, GridViewRowEditEndedEventArgs e)
        {
            PaisViewModel paisViewModel = null;

            Pais newPais = null;

            paisViewModel = (PaisViewModel)this.DataContext;

            if (e.EditAction == GridViewEditAction.Cancel)
            {
                e.UserDefinedErrors.Clear();

                return;
            }

            if (e.EditOperationType == GridViewEditOperationType.Insert)
            {
                //Add the new entry to the data base.
                //this.radGridView1.SelectedRows[0].Cells["Picture Name"].Value;

                GridViewRow row = e.Row;
                if (e.Row is GridViewNewRow)
                {
                    newPais = e.NewData as Pais;
                    if (newPais == null)
                    {
                        return;
                    }

                    paisViewModel.Insert(newPais);

                    //System.Windows.MessageBox.Show(string.Format("New Pais: {0} {1}", newPais.id, newPais.nombre));
                }

                //this.paisModel.AddNewCountry(newPais);

                // This operation may be costly
                // it should only happen when operation 'success'
                //this.gridView.Rebind();

                return;
            }

            if (e.EditOperationType == GridViewEditOperationType.Edit)
            {
                GridViewRow row = e.Row;

                newPais = e.NewData as Pais;
                if (newPais == null)
                {
                    return;
                }

                //System.Windows.MessageBox.Show(string.Format("Update Pais: {0} {1}", newPais.id, newPais.nombre));

                paisViewModel.Update(newPais);
            }
        }

        private void gridView_NewItem(object sender, Telerik.Windows.Controls.GridView.GridViewAddingNewEventArgs e)
        {
            //e.Cancel = true;
            // http://docs.telerik.com/devtools/wpf/controls/radgridview/how-to/insert-new-row-into-paged-gridview
            Dispatcher.BeginInvoke(new Action(() => {
                ((RadGridView)sender).Items.MoveToLastPage();
            }));

            Pais newPais = new Pais()
            {
                id = -1
            };

            e.NewObject = newPais;

            //this.gridView.ScrollIntoView(newPais);
            //this.dataPager.MoveToLastPage();
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
        }
    }
}