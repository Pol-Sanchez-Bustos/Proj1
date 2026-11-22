
using DAO;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AppProj
{
    /// <summary>
    /// Lógica de interacción para Editor.xaml
    /// </summary>
    public partial class Editor : Window
    {
        Usuari user;

        ObservableCollection<Parada> paradaOc = new ObservableCollection<Parada>();
        ObservableCollection<Trajecte> trajecteOc = new ObservableCollection<Trajecte>();

        enum Estat
        {
            EDICIO,
            CREACIO,
            IDLE
        }

        Estat estatTrajecte = Estat.IDLE;
        Estat estatParada = Estat.IDLE;
        public Editor(Usuari usr)
        {
            InitializeComponent();
            user = usr;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MySQLFactory.getUOW(uow =>
            {
                trajecteOc = uow.DAOTrajecte.getTrajectes("-1", ""+user.Id, "-1");
                dtgTrajectes.ItemsSource = trajecteOc;

                cbMitjaTransport.ItemsSource = uow.DAOMitjaTrans.getMitjasTrans("-1");
                cbMitjaTransport.SelectedIndex = 0;

            });
        }

        private void estatTrajecteUpdate()
        {
            switch (estatTrajecte)
            {
                case Estat.EDICIO:
                    btEliminarTrajecte.IsEnabled = true;
                    btCancelaTraj.IsEnabled = true;
                    btGuardarCanvisTraj.IsEnabled = true;
                    txtNom.IsEnabled = true;
                    cbMitjaTransport.IsEnabled = true;
                    btNovaParada.IsEnabled = true;
                    break;

                case Estat.CREACIO:
                    btEliminarTrajecte.IsEnabled = false;
                    btCancelaTraj.IsEnabled = true;
                    btGuardarCanvisTraj.IsEnabled = true;
                    txtNom.IsEnabled = true;
                    cbMitjaTransport.IsEnabled = true;
                    btNovaParada.IsEnabled = false;
                    break;

                case Estat.IDLE:
                    
                    btEliminarTrajecte.IsEnabled = false;
                    btCancelaTraj.IsEnabled = false;
                    btGuardarCanvisTraj.IsEnabled = false;
                    txtNom.IsEnabled = false;
                    cbMitjaTransport.IsEnabled = false;
                    btNovaParada.IsEnabled = false;
                    break;
            }
        }

        private void estatParadaUpdate()
        {
            switch (estatParada)
            {
                case Estat.EDICIO:
                    btCancelaPar.IsEnabled = true;
                    btGuardarCanvisPar.IsEnabled = true;
                    btEliminarParada.IsEnabled = true;
                    txtEstacio.IsEnabled = true;
                    txtHora.IsEnabled = true;
                    btPuja.IsEnabled = true;
                    btBaixa.IsEnabled = true;
                    btNovaParada.Content = "Nova Parada";
                    break;

                case Estat.CREACIO:
                    btCancelaPar.IsEnabled = true;
                    btGuardarCanvisPar.IsEnabled = true;
                    btEliminarParada.IsEnabled = false;
                    txtEstacio.IsEnabled = true;
                    txtHora.IsEnabled = true;
                    btPuja.IsEnabled = false;
                    btBaixa.IsEnabled = false;
                    btNovaParada.Content = "Afegir al trajecte";
                    break;

                case Estat.IDLE:
                    btCancelaPar.IsEnabled = false;
                    btGuardarCanvisPar.IsEnabled = false;
                    btEliminarParada.IsEnabled = false;
                    txtEstacio.IsEnabled = false;
                    txtHora.IsEnabled = false;
                    btPuja.IsEnabled = false;
                    btBaixa.IsEnabled = false;
                    btNovaParada.Content = "Nova Parada";
                    break;
            }
        }

        private void mnLogOut_Click(object sender, RoutedEventArgs e)
        {
            MainWindow ventana = new MainWindow();
            ventana.Show();

            this.Close();
        }

        private void mnConsultar_Click(object sender, RoutedEventArgs e)
        {
            MainWindow ventana = new MainWindow(user);
            ventana.Show();

            this.Close();
        }

        private void btNouTrajecte_Click(object sender, RoutedEventArgs e)
        {
            txtNom.Text = "";
            dtgTrajectes.SelectedItem = null;
            estatTrajecte = Estat.CREACIO; estatTrajecteUpdate();
        }

        private void btEliminarTrajecte_Click(object sender, RoutedEventArgs e)
        {
            if (dtgTrajectes.HasItems && dtgTrajectes.IsLoaded && dtgTrajectes.SelectedItem != null)
            {
                MySQLFactory.getUOW(uow =>
                {
                    Trajecte tj = dtgTrajectes.SelectedItem as Trajecte;

                    uow.DAOTrajecte.deleteTrajecte(""+tj.Id);
                    
                    uow.Commit();

                    trajecteOc.Remove(tj);

                    txtNom.Text = "";
                    estatTrajecte = Estat.IDLE; estatTrajecteUpdate();
                    dtgTrajectes.Items.Refresh();
                });
            }
        }

        private void btCancelaTraj_Click(object sender, RoutedEventArgs e)
        {
            txtNom.Text = "";
            dtgTrajectes.SelectedItem = null;
            
            estatTrajecte = Estat.IDLE; estatTrajecteUpdate();
        }

        private void btGuardarCanvisTraj_Click(object sender, RoutedEventArgs e)
        {
            if(txtNom.Text.Length < 2)
            {
                txtNom.Foreground = Brushes.IndianRed;
                
            }else if(estatTrajecte == Estat.EDICIO)
            {
                MySQLFactory.getUOW(uow =>
                {
                    Trajecte tj = dtgTrajectes.SelectedItem as Trajecte;

                    tj.Validat = 0;
                    tj.Nom = txtNom.Text;
                    tj.Mitja = cbMitjaTransport.SelectedItem as MitjaTrans;

                    uow.DAOTrajecte.updateTrajecte(tj);

                    uow.Commit();
                    dtgTrajectes.Items.Refresh();



                });
            }
            else
            {
                MySQLFactory.getUOW(uow =>
                {
                    Trajecte tj = new Trajecte(0, txtNom.Text, DateTime.Now, 0, cbMitjaTransport.SelectedItem as MitjaTrans, user);

                    uow.DAOTrajecte.insertTrajecte(tj);

                    uow.Commit();

                    MySQLFactory.getUOW(uow =>
                    {
                        trajecteOc = uow.DAOTrajecte.getTrajectes("-1", "" + user.Id, "-1");
                        dtgTrajectes.ItemsSource = trajecteOc;
                    });
                });
            }
        }

        private void dtgTrajectes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtgTrajectes.HasItems && dtgTrajectes.IsLoaded && dtgTrajectes.SelectedItem != null) {
                estatTrajecte = Estat.EDICIO;
                estatTrajecteUpdate();

                Trajecte tj = dtgTrajectes.SelectedItem as Trajecte;

                txtNom.Text = tj.Nom;
                cbMitjaTransport.SelectedItem = tj.Mitja;

                MySQLFactory.getUOW(uow =>
                {
                    paradaOc = uow.DAOParada.getParades("-1",""+tj.Id, "-1") ;
                    dtgParades.ItemsSource = paradaOc;
                });
            }

        }

        private void dtgParades_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtgParades.HasItems && dtgParades.IsLoaded && dtgParades.SelectedItem != null)
            {
                estatParada = Estat.EDICIO;
                estatParadaUpdate();

                Parada pd = dtgParades.SelectedItem as Parada;

                txtEstacio.Text = pd.Estacio.Nom;
                txtHora.Text = pd.HoraPasFormat;

                if(pd.Ordre == 1)
                {
                    btPuja.IsEnabled = false;
                }

                if (pd.Ordre == paradaOc.Count)
                {
                    btBaixa.IsEnabled = false;
                }


            }
        }

        private void btNovaParada_Click(object sender, RoutedEventArgs e)
        {
            if (estatParada != Estat.CREACIO) {
                txtEstacio.Text = "";
                txtHora.Text = "";
                dtgParades.SelectedItem = null;
                estatParada = Estat.CREACIO;
                estatParadaUpdate();
            }
            else
            {
                Trajecte tj = dtgTrajectes.SelectedItem as Trajecte;

                int horaSortida;

                string[] parts = txtHora.Text.Split(':');

                int hores = int.Parse(parts[0]);
                int minuts = int.Parse(parts[1]);

                horaSortida = (hores * 60) + minuts;

                Parada pd = new Parada(paradaOc.Count + 1, horaSortida, null, tj);

                paradaOc.Add(pd);

            }
        }
        

        private void btEliminarParada_Click(object sender, RoutedEventArgs e)
        {

            Parada pd = dtgTrajectes.SelectedItem as Parada;
            paradaOc.Remove(pd);
            txtEstacio.Text = "";
            txtHora.Text = "";
            dtgParades.Items.Refresh();
            dtgParades.SelectedItem = null;

        }

        private void btPuja_Click(object sender, RoutedEventArgs e)
        {
            int maxO = paradaOc.Count;
            Parada pd = dtgTrajectes.SelectedItem as Parada;

            

            foreach (Parada par in paradaOc) { 
                if(par.Ordre > pd.Ordre)
                {
                    par.Ordre--;
                }
            }

            pd.Ordre++;
            if (pd.Ordre == maxO) { 
                btPuja.IsEnabled = false;
            }




        }

        private void btBaixa_Click(object sender, RoutedEventArgs e)
        {
            Parada pd = dtgTrajectes.SelectedItem as Parada;



            foreach (Parada par in paradaOc)
            {
                if (par.Ordre > pd.Ordre)
                {
                    par.Ordre++;
                }
            }

            pd.Ordre--;
            if (pd.Ordre == 0)
            {
                btBaixa.IsEnabled = false;
            }
        }

        private void btNovaEstacio_Click(object sender, RoutedEventArgs e)
        {


          
        }

        

        private void btCancelaPar_Click(object sender, RoutedEventArgs e)
        {
            txtEstacio.Text = "";
            txtHora.Text = "";


            Trajecte tj = dtgTrajectes.SelectedItem as Trajecte;
            MySQLFactory.getUOW(uow =>
            {
                paradaOc = uow.DAOParada.getParades("-1", "" + tj.Id, "-1");
                dtgParades.ItemsSource = paradaOc;
            });

            estatParada = Estat.IDLE;
            estatParadaUpdate();

            dtgParades.Items.Refresh();
            dtgParades.SelectedItem = null;
        }

        private void btGuardarCanvisPar_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
