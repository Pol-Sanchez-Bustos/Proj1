
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
                    btGuardarCanvisPar.IsEnabled = true;
                    btEliminarParada.IsEnabled = true;
                    txtEstacio.IsEnabled = true;
                    txtHora.IsEnabled = true;
                    btPuja.IsEnabled = true;
                    btBaixa.IsEnabled = true;
                    btNovaParada.Content = "Guardar cambis parada";
                    btCancelaPar.Content = "Para d'editar";
                    break;

                case Estat.CREACIO:
                    btGuardarCanvisPar.IsEnabled = true;
                    btEliminarParada.IsEnabled = false;
                    txtEstacio.IsEnabled = true;
                    txtHora.IsEnabled = true;
                    btPuja.IsEnabled = false;
                    btBaixa.IsEnabled = false;
                    btNovaParada.Content = "Afegir al trajecte";
                    btCancelaPar.Content = "Para d'editar";
                    break;

                case Estat.IDLE:
                    btGuardarCanvisPar.IsEnabled = false;
                    btEliminarParada.IsEnabled = false;
                    txtEstacio.IsEnabled = false;
                    txtHora.IsEnabled = false;
                    btPuja.IsEnabled = false;
                    btBaixa.IsEnabled = false;
                    btNovaParada.Content = "Nova Parada";
                    btCancelaPar.Content = "Reestableix Parades";
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

                    trajecteOc = uow.DAOTrajecte.getTrajectes("-1", "" + user.Id, "-1");
                    dtgTrajectes.ItemsSource = trajecteOc;
                });
            }
        }

        private void dtgTrajectes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtgTrajectes.HasItems && dtgTrajectes.IsLoaded && dtgTrajectes.SelectedItem == null) {
                txtNom.Text = "";
                dtgTrajectes.SelectedItem = null;

                estatTrajecte = Estat.IDLE; estatTrajecteUpdate();

                paradaOc = new ObservableCollection<Parada>();

                txtEstacio.Text = "";
                txtHora.Text = "";

                estatParada = Estat.IDLE;
                estatParadaUpdate();

                dtgParades.Items.Refresh();
                btCancelaPar.IsEnabled = false;

            } 
           
            else if(dtgTrajectes.HasItems && dtgTrajectes.IsLoaded && dtgTrajectes.SelectedItem != null) {
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
                btCancelaPar.IsEnabled = true;
            }

        }

        private void dtgParades_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtgParades.HasItems && dtgParades.IsLoaded && dtgParades.SelectedItem != null)
            {
                estatParada = Estat.EDICIO;
                estatParadaUpdate();

                

                Parada pd = dtgParades.SelectedItem as Parada;

                if (pd == null)
                {
                    MessageBox.Show("La parada es null");
                    return;
                }

                if (pd.Estacio == null)
                {
                    MessageBox.Show("La estación es null");
                    return;
                }

                if (pd.Estacio.Ciutat == null)
                {
                    MessageBox.Show("La ciudad de la estación es null");
                    return;
                }

                if (pd.Estacio.Ciutat.Nom == null)
                {
                    MessageBox.Show("El nombre de la ciudad es null");
                    return;
                }


                txtEstacio.Text = pd.Estacio.Ciutat.Nom+","+pd.Estacio.Nom;
                txtEstacio.BorderBrush = Brushes.Black;

                txtHora.Text = pd.HoraPasFormat;
                txtHora.BorderBrush = Brushes.Black;

                if (pd.Ordre == 0)
                {
                    btPuja.IsEnabled = false;
                }

                if (pd.Ordre == paradaOc.Count-1)
                {
                    btBaixa.IsEnabled = false;
                }


            }
        }

        private void btNovaParada_Click(object sender, RoutedEventArgs e)
        {
            if (estatParada == Estat.IDLE) {
                txtEstacio.Text = "";
                txtHora.Text = "";
                dtgParades.SelectedItem = null;
                estatParada = Estat.CREACIO;
                estatParadaUpdate();

            }
            else if (estatParada == Estat.CREACIO)
            {
                Trajecte tj = dtgTrajectes.SelectedItem as Trajecte;

                bool valid = true;

                Estacio  est = null;

                MySQLFactory.getUOW(uow =>
                {
                    String[] origen = txtEstacio.Text.Split(',');


                    if (origen.Length != 2)
                    {
                        txtEstacio.BorderBrush = Brushes.IndianRed;
                        valid = false;
                    }
                    else
                    {

                        origen[0] = origen[0].Trim();
                        origen[1] = origen[1].Trim();

                        try
                        {
                            MySQLFactory.getUOW(uow =>
                            {
                                int ciutatId = uow.DAOCiutat.getCiutat("-1", origen[0]).Id;


                                est = uow.DAOEstacio.getEstacio("-1",
                                    ciutatId + "",
                                    origen[1]
                                    );

                            });

                        }
                        catch (Exception ex)
                        {
                            txtEstacio.BorderBrush = Brushes.IndianRed;
                            valid = false;
                        }
                    }


                });

                Parada pd = new Parada(paradaOc.Count, 1, est, tj);

                pd.HoraPasFormat = txtHora.Text;
                if (pd.HoraPas < 0 || pd.HoraPas > 1439)
                {
                    txtHora.BorderBrush = Brushes.IndianRed;
                    valid = false;
                }
                

                if (valid)
                {
                    pd.Editat = true;
                    paradaOc.Add(pd);
                    dtgParades.Items.Refresh();

                    estatParada = Estat.IDLE;
                    estatParadaUpdate();
                }


                

            }
            else
            {
                Parada pd = dtgParades.SelectedItem as Parada;
                Parada pd_aux = pd;
                bool valid = true;

                MySQLFactory.getUOW(uow =>
                {
                    String[] origen = txtEstacio.Text.Split(',');


                    if (origen.Length != 2)
                    {
                        txtEstacio.BorderBrush = Brushes.IndianRed;
                        valid = false;
                    }
                    else {

                        origen[0] = origen[0].Trim();
                        origen[1] = origen[1].Trim();

                        try
                        {
                            MySQLFactory.getUOW(uow =>
                            {
                                int ciutatId = uow.DAOCiutat.getCiutat("-1", origen[0]).Id;


                                pd_aux.Estacio = uow.DAOEstacio.getEstacio("-1",
                                    ciutatId+"",
                                    origen[1]
                                    );

                                if(pd_aux.Estacio == null)
                                {
                                    txtEstacio.BorderBrush = Brushes.IndianRed;
                                    valid = false;
                                }


                            });

                        }
                        catch (Exception ex)
                        {
                            txtEstacio.BorderBrush = Brushes.IndianRed;
                            valid = false;
                        }
                    }


                });

                if(txtHora.Text.Split(":").Length == 2)
                {
                    pd_aux.HoraPasFormat = txtHora.Text;
                    if (pd_aux.HoraPas < 0 || pd_aux.HoraPas > 1439)
                    {
                        txtHora.BorderBrush = Brushes.IndianRed;
                        valid = false;
                    }
                }
                else
                {
                    txtHora.BorderBrush = Brushes.IndianRed;
                    valid = false;
                }
                


                if (valid)
                {
                    pd = pd_aux;
                    pd.Editat = true;
                    txtEstacio.Text = "";
                    txtHora.Text = "";
                    dtgParades.SelectedItem = null;
                    estatParada = Estat.IDLE;
                    estatParadaUpdate();
                    dtgParades.Items.Refresh();
                }
                
            }
        }
        

        private void btEliminarParada_Click(object sender, RoutedEventArgs e)
        {

            Parada pd = dtgParades.SelectedItem as Parada;
            paradaOc.Remove(pd);

            foreach (Parada par in paradaOc)
            {
                if (par.Ordre > pd.Ordre)
                {
                    par.Ordre--;
                }
            }


            txtEstacio.Text = "";
            txtHora.Text = "";
            dtgParades.Items.Refresh();
            dtgParades.SelectedItem = null;

        }

        private void btPuja_Click(object sender, RoutedEventArgs e)
        {
            
            Parada pd = dtgParades.SelectedItem as Parada;

            

            foreach (Parada par in paradaOc) { 
                if(par.Ordre-1 == pd.Ordre)
                {
                    par.Ordre--;
                    par.Editat = true;
                }
            }

            pd.Ordre++;
            pd.Editat = true;
            if (pd.Ordre == 0) { 
                btPuja.IsEnabled = false;
            }

            dtgParades.Items.Refresh();


        }

        private void btBaixa_Click(object sender, RoutedEventArgs e)
        {

            int maxO = paradaOc.Count;
            Parada pd = dtgParades.SelectedItem as Parada;



            foreach (Parada par in paradaOc)
            {
                if (par.Ordre+1 == pd.Ordre)
                {
                    par.Ordre++;
                    par.Editat = true;
                }
            }

            pd.Ordre--;
            pd.Editat = true;
            if (pd.Ordre == maxO)
            {
                btBaixa.IsEnabled = false;
            }

            dtgParades.Items.Refresh();
            
        }

        private void btNovaEstacio_Click(object sender, RoutedEventArgs e)
        {


          
        }

        

        private void btCancelaPar_Click(object sender, RoutedEventArgs e)
        {

            txtEstacio.Text = "";
            txtHora.Text = "";
            

            if (estatParada == Estat.IDLE)
            {
                Trajecte tj = dtgTrajectes.SelectedItem as Trajecte;
                MySQLFactory.getUOW(uow =>
                {
                    paradaOc = uow.DAOParada.getParades("-1", "" + tj.Id, "-1");
                    dtgParades.ItemsSource = paradaOc;
                });

                
                dtgParades.Items.Refresh();
                dtgParades.SelectedItem = null;
            }

            dtgParades.SelectedItem = null;
            estatParada = Estat.IDLE;
            estatParadaUpdate();

        }

        private void btGuardarCanvisPar_Click(object sender, RoutedEventArgs e)
        {
            MySQLFactory.getUOW(uow =>
            {
                Trajecte tj = dtgTrajectes.SelectedItem as Trajecte;
                uow.DAOParada.deleteParada(""+(paradaOc.Count - 1),""+ tj.Id);

                foreach (Parada par in paradaOc)
                {
                    if (par.Editat)
                    {
                        uow.DAOParada.updateParada(par);
                    }
                }

                uow.Commit();

            });
        }
    }
}
