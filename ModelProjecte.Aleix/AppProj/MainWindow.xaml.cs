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
            
        }

        private void btBuscarTrajecte_Click(object sender, RoutedEventArgs e)
        {
            if(txtSortida.Text == "")
            {
                txtSortida.Text = "0:00";
            }

            if (txtSortida.Text.Count(':') == 1)
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

                int origenId = -1, destiId = -1;

                try
                {
                    MySQLFactory.getUOW(uow =>
                    {
                        origenId = uow.DAOEstacio.getEstacio("-1",
                            uow.DAOCiutat.getCiutat("-1", origen[0]).Id+"",
                            origen[1]
                            ).Id;

                        destiId = uow.DAOEstacio.getEstacio("-1",
                            uow.DAOCiutat.getCiutat("-1", desti[0]).Id + "",
                            desti[1]
                            ).Id;
                    });

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Dades invalides", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                ObservableCollection<Parada> lisOrigen;
                ObservableCollection<Trajecte> lisTrajecte = new ObservableCollection<Trajecte>();


                try
                {
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

                                    if (trajecte != null & trajecte.Validat == 1)
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
            else
            {
                txtSortida.BorderBrush = Brushes.IndianRed;
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

        
    }
}