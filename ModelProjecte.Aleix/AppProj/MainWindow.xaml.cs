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
                    int horaSortida;

                    string[] parts = txtSortida.Text.Split(':');

                    int hores = int.Parse(parts[0]);
                    int minuts = int.Parse(parts[1]);

                    if (hores < 0 || hores > 23 || minuts < 0 || minuts > 59)
                    {
                        txtSortida.BorderBrush = Brushes.IndianRed;
                        return;
                    }

                    horaSortida = (hores * 60) + minuts;

                    ObservableCollection<ResultatTrajecte> lisTrajecte =
                        new ObservableCollection<ResultatTrajecte>();

                    ObservableCollection<ResultatTrajecte> lisTransbords =
                        new ObservableCollection<ResultatTrajecte>();

                    MySQLFactory.getUOW(uow =>
                    {
                        ObservableCollection<int> ids =
                            uow.DAOParada.getTrajectesEntreEstacions(
                                origenId, destiId, horaSortida);

                        foreach (int id in ids)
                        {
                            ResultatTrajecte trajecte =
                                uow.DAOTrajecte.getResultatTrajecte(""+id,origenId,destiId);

                            if (trajecte != null && trajecte.Trajecte1.Validat == 1)
                            {
                                lisTrajecte.Add(trajecte);
                            }
                        }

                        ObservableCollection<ResultatTrajecte> transbords =
                            uow.DAOParada.buscarTransbordos(origenId, destiId);

                        foreach (ResultatTrajecte resultat in transbords)
                        {
                            if (resultat.HoraSortida >= horaSortida)
                            {
                                lisTransbords.Add(resultat);
                            }
                        }
                    });

                    dtgTrajectes.ItemsSource = lisTrajecte;
                    dtgTransbords.ItemsSource = lisTransbords;

                    txtTrajecte.Text = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (dtgTrajectes.HasItems && dtgTrajectes.IsLoaded && dtgTrajectes.SelectedItem != null)
            {
                ResultatTrajecte resultat = dtgTrajectes.SelectedItem as ResultatTrajecte;

                if (resultat == null)
                    return;

                String[] origen = txtOrigen.Text.Split(',');
                String[] desti = txtDesti.Text.Split(',');

                string nomOrigen = origen[1].Trim();
                string nomDesti = desti[1].Trim();

                string str = "";

                str += "Dades del trajecte seleccionat\n\n";
                str += "TRAJECTE: " + resultat.Trajecte1.Nom + "\n\n";

                bool iniciat = false;

                foreach (Parada par in resultat.ParadesTrajecte1)
                {
                    if (par.Estacio.Nom.CompareTo(nomOrigen) == 0)
                        iniciat = true;

                    if (iniciat)
                        str += par.Estacio.Nom + " - " + par.HoraPasFormat + "\n";

                    if (par.Estacio.Nom.CompareTo(nomDesti) == 0 && iniciat)
                        break;
                }

                str += "\n";
                str += "Sortida: " + resultat.HoraSortidaFormat + "\n";
                str += "Arribada: " + resultat.HoraArribadaFormat + "\n";

                txtTrajecte.Text = str;
                txtTrajecte.Visibility = Visibility.Visible;
            }
        }


        private void dtgTransbords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtgTransbords.HasItems && dtgTransbords.IsLoaded &&
                dtgTransbords.SelectedItem != null)
            {
                ResultatTrajecte resultat =
                    dtgTransbords.SelectedItem as ResultatTrajecte;

                if (resultat == null)
                    return;

                string str = "";

                str += "Dades del trajecte seleccionat\n\n";

                str += "PRIMER TRAJECTE: " + resultat.Trajecte1.Nom + "\n\n";

                String[] origen = txtOrigen.Text.Split(',');
                String[] desti = txtDesti.Text.Split(',');

                string nomOrigen = origen[1].Trim();
                string nomDesti = desti[1].Trim();

                bool iniciat = false;

                foreach (Parada par in resultat.ParadesTrajecte1)
                {
                    if (par.Estacio.Nom.CompareTo(nomOrigen) == 0)
                        iniciat = true;

                    if (iniciat)
                        str += par.Estacio.Nom + " - " + par.HoraPasFormat + "\n";

                    if (par.Estacio.Id == resultat.EstacioTransbordo.Id && iniciat)
                        break;
                }

                str += "\n";
                str += "SEGON TRAJECTE: " + resultat.Trajecte2.Nom + "\n\n";

                iniciat = false;

                foreach (Parada par in resultat.ParadesTrajecte2)
                {
                    if (par.Estacio.Id == resultat.EstacioTransbordo.Id)
                        iniciat = true;

                    if (iniciat)
                        str += par.Estacio.Nom + " - " + par.HoraPasFormat + "\n";

                    if (par.Estacio.Nom.CompareTo(nomDesti) == 0 && iniciat)
                        break;
                }

                str += "\n";
                str += "Sortida: " + resultat.HoraSortidaFormat + "\n";
                str += "Arribada: " + resultat.HoraArribadaFormat + "\n";

                txtTrajecte.Text = str;
                txtTrajecte.Visibility = Visibility.Visible;
            }
        }
    }
}