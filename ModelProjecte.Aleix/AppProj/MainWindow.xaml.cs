using DAO;
using IDao;
using Model;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace AppProj
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Usuari user = null;
        public MainWindow()
        {

            
            InitializeComponent();
        }

        public MainWindow(Usuari usuari)
        {

            user = usuari;
            
            InitializeComponent();
            updateMenus();
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MySQLFactory.getUOW(uow =>
            {
                ObservableCollection<Estacio> est = uow.DAOEstacio.getEstacions("-1", "-1", "");
                lstSugerenciasOrigen.ItemsSource = est;
                lstSugerenciasDesti.ItemsSource = est;

            });
        }

        private void btBuscarTrajecte_Click(object sender, RoutedEventArgs e)
        {
            bool sort = true, dest = true, org = true;
            if(txtSortida.Text == "")
            {
                txtSortida.Text = "0:00";
            }

            if (txtSortida.Text.Count(':') != 1)
            {
                sort = false;
                txtSortida.BorderBrush = Brushes.IndianRed;
            }
            
            if (txtDesti.Text.Count(',') != 1)
            {
                dest = false;
                txtDesti.BorderBrush = Brushes.IndianRed;
            }

            if (txtOrigen.Text.Count(',') != 1)
            {
                dest = false;
                txtOrigen.BorderBrush = Brushes.IndianRed;
            }


            int origenId = -1, destiId = -1;

            if (sort && dest && org)
            {
                String[] origen = txtOrigen.Text.Split(',');
                String[] desti = txtDesti.Text.Split(',');


                if (origen.Length != 2 || desti.Length != 2)
                {
                    MessageBox.Show("Format incorrecte. Utilitza: Ciutat, Estació");
                    return;
                }

                origen[0] = origen[0].Trim();
                origen[1] = origen[1].Trim();

                desti[0] = desti[0].Trim();
                desti[1] = desti[1].Trim();

                

                try
                {
                    MySQLFactory.getUOW(uow =>
                    {
                        Estacio est = uow.DAOEstacio.getEstacio("-1",
                            uow.DAOCiutat.getCiutat("-1", origen[0]).Id + "",
                            origen[1]
                            );

                        if(est == null)
                        {
                            txtOrigen.BorderBrush = Brushes.IndianRed;
                            org = false;
                        }
                        else
                        {
                            origenId = est.Id;
                        }

                    });

                }
                catch (Exception ex)
                {
                    txtOrigen.BorderBrush = Brushes.IndianRed;
                    org = false;
                    MessageBox.Show("Origen invalid", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    MySQLFactory.getUOW(uow =>
                    {
                        Estacio est = uow.DAOEstacio.getEstacio("-1",
                            uow.DAOCiutat.getCiutat("-1", desti[0]).Id + "",
                            desti[1]
                            );

                        if (est == null)
                        {
                            txtDesti.BorderBrush = Brushes.IndianRed;
                            dest = false;
                        }
                        else
                        {
                            destiId = est.Id;
                        }

                        
                    });

                }
                catch (Exception ex)
                {
                    txtDesti.BorderBrush = Brushes.IndianRed;
                    dest = false;
                    MessageBox.Show("Desti invalid", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }



            }
            if (sort && dest && org)
            {
                try
                {
                    ObservableCollection<Parada> lisOrigen;
                    ObservableCollection<Trajecte> lisTrajecte = new ObservableCollection<Trajecte>();

                    MySQLFactory.getUOW(uow =>
                    {
                        lisOrigen = uow.DAOParada.getParades("-1", "-1", "" +origenId);

                        ObservableCollection<Trajecte> lisTrajecte = new ObservableCollection<Trajecte>();

                        try
                        {

                            int horaSortida;

                            string[] parts = txtSortida.Text.Split(':');

                            int hores = int.Parse(parts[0]);
                            int minuts = int.Parse(parts[1]);

                            horaSortida = (hores * 60) + minuts;

                            MySQLFactory.getUOW(uow =>
                            {
                                ObservableCollection<int> ids =
                                    uow.DAOParada.getTrajectesEntreEstacions(
                                        origenId, destiId, horaSortida);

                                foreach (int id in ids)
                                {
                                    Trajecte trajecte =
                                        uow.DAOTrajecte.getTrajecte(id.ToString());

                                    if (trajecte != null && trajecte.Validat == 1)
                                        lisTrajecte.Add(trajecte);
                                }
                            });

                            dtgTrajectes.ItemsSource = lisTrajecte;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString(), "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    
 
                    });

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Dades invalides", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            
            

        }

        

        private void mnSignIn_Click(object sender, RoutedEventArgs e)
        {
            SignIn ventana = new SignIn();
            ventana.Show();

            this.Close();
        }

        private void mnLogIn_Click(object sender, RoutedEventArgs e)
        {
            LogIn ventana = new LogIn();
            ventana.Show();

            this.Close();
        }

        private void mnEdit_Click(object sender, RoutedEventArgs e)
        {
            
            Editor ventana = new Editor(user);
            ventana.Show();

            this.Close();

        }

        private void mnAdmin_Click(object sender, RoutedEventArgs e)
        {
            Administrador ventana = new Administrador(user);
            ventana.Show();

            this.Close();
        }

        private void updateMenus()
        {
            if(user == null )
            {
                mnSignIn.Visibility = Visibility.Visible;
                mnLogIn.Visibility = Visibility.Visible;

                mnEdit.Visibility = Visibility.Collapsed;
                mnAdmin.Visibility = Visibility.Collapsed;
                mnLogOut.Visibility = Visibility.Collapsed;
            }
            else
            {
                mnSignIn.Visibility = Visibility.Collapsed;
                mnLogIn.Visibility = Visibility.Collapsed;

                
                mnEdit.Visibility = Visibility.Visible;
                if(user.Tipus == 'A')mnAdmin.Visibility = Visibility.Visible;
                mnLogOut.Visibility = Visibility.Visible;
            }
        }

        private void mnLogOut_Click(object sender, RoutedEventArgs e)
        {
            user = null;
            updateMenus();
        }

        private void txtSortida_KeyDown(object sender, KeyEventArgs e)
        {
            if(txtSortida.BorderBrush == Brushes.IndianRed)
            {
                txtSortida.BorderBrush = Brushes.Black;
            }

        }

        private void txtDesti_KeyDown(object sender, KeyEventArgs e)
        {
            if (txtDesti.BorderBrush == Brushes.IndianRed)
            {
                txtDesti.BorderBrush = Brushes.Black;
            }
        }

        private void txtOrigen_KeyDown(object sender, KeyEventArgs e)
        {
            if (txtOrigen.BorderBrush == Brushes.IndianRed)
            {
                txtOrigen.BorderBrush = Brushes.Black;
            }
        }

        private void lstSugerenciasOrigen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (lstSugerenciasOrigen.SelectedItem is Estacio estacionSeleccionada)
            {


                txtOrigen.Text = ""+estacionSeleccionada.ToString();



                popupSugerenciasOrigen.IsOpen = false;

            }
        }

        private void lstSugerenciasDesti_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstSugerenciasDesti.SelectedItem is Estacio estacionSeleccionada)
            {


                txtDesti.Text = ""+estacionSeleccionada.ToString();



                popupSugerenciasDesti.IsOpen = false;

            }

        }

        private void txtOrigen_GotFocus(object sender, RoutedEventArgs e)
        {
            popupSugerenciasOrigen.IsOpen = true;
        }

        private void txtDesti_GotFocus(object sender, RoutedEventArgs e)
        {
            popupSugerenciasDesti.IsOpen = true;
        }

        private void txtOrigen_TextChanged(object sender, TextChangedEventArgs e)
        {
            popupSugerenciasOrigen.IsOpen = txtOrigen.IsFocused;
        }

        private void txtDesti_TextChanged(object sender, TextChangedEventArgs e)
        {
            popupSugerenciasDesti.IsOpen = txtDesti.IsFocused;
        }

        private void dtgTrajectes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(dtgTrajectes.HasItems && dtgTrajectes.IsLoaded && dtgTrajectes.SelectedItem != null)
            {
                Trajecte traj = dtgTrajectes.SelectedItem as Trajecte;
                List<Parada> pars = new List<Parada>();

                MySQLFactory.getUOW(uow =>
                {
                    pars = uow.DAOParada.getParades("-1", "" + traj.Id, "-1").ToList<Parada>();




                });


                String str = "Dades de trajecte seleccionat\n" 
                    + "\n\n";

                foreach (Parada par in pars)
                {

                    if (par.Ordre == 0)
                    {
                        str += "Origen: ";
                    }
                    else
                    {
                        str += "-> ";
                    }

                    str += par.Estacio.Nom + " - " + par.HoraPasFormat + "\n";
                }

                str += "\nConsulta:\n" + traj.Nom;

                txtTrajecte.Text = str;
            }
            
        }
    }
}