
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
    public partial class Administrador : Window
    {

        Usuari user;

        ObservableCollection<Usuari> users = new ObservableCollection<Usuari>();

        ObservableCollection<Trajecte> validats = new ObservableCollection<Trajecte>();
        ObservableCollection<Trajecte> noValidats = new ObservableCollection<Trajecte>();
        public Administrador(Usuari newUser)
        {
            user = newUser;
            InitializeComponent();
        }



        private void btValidar_Click(object sender, RoutedEventArgs e)
        {
            if (cbTrajectesNoValidats.SelectedItem != null && cbTrajectesNoValidats.HasItems && cbTrajectesNoValidats.IsLoaded)
            {
                Trajecte traj = cbTrajectesNoValidats.SelectedItem as Trajecte;

                traj.Validat = 1;

                MySQLFactory.getUOW(uow =>
                {
                    uow.DAOTrajecte.updateTrajecte(traj);
                    noValidats.Remove(traj);
                    validats.Add(traj);
                    uow.Commit();

                    cbTrajectesNoValidats.Items.Refresh();
                    cbTrajectesNoValidats.SelectedIndex  = 0;
                    cbTrajectesValidats.Items.Refresh();
                    cbTrajectesValidats.SelectedIndex = 0;

                });

                if (cbTrajectesNoValidats.HasItems)
                {
                    btValidar.IsEnabled = true;
                }
                else
                {
                    btValidar.IsEnabled = false;
                }

                if (cbTrajectesValidats.HasItems)
                {
                    btDesvalidar.IsEnabled = true;
                }
                else
                {
                    btDesvalidar.IsEnabled = false;
                }

            }
        }

        private void btDesvalidar_Click(object sender, RoutedEventArgs e)
        {
            if (cbTrajectesValidats.SelectedItem != null && cbTrajectesValidats.HasItems && cbTrajectesValidats.IsLoaded)
            {
                Trajecte traj = cbTrajectesValidats.SelectedItem as Trajecte;

                traj.Validat = 0;

                MySQLFactory.getUOW(uow =>
                {
                    uow.DAOTrajecte.updateTrajecte(traj);
                    validats.Remove(traj);
                    noValidats.Add(traj);
                    uow.Commit();

                    cbTrajectesNoValidats.Items.Refresh();
                    cbTrajectesNoValidats.SelectedIndex = 0;
                    cbTrajectesValidats.Items.Refresh();
                    cbTrajectesValidats.SelectedIndex = 0;
                });

                if (cbTrajectesNoValidats.HasItems)
                {
                    btValidar.IsEnabled = true;
                }
                else
                {
                    btValidar.IsEnabled = false;
                }

                if (cbTrajectesValidats.HasItems)
                {
                    btDesvalidar.IsEnabled = true;
                }
                else
                {
                    btDesvalidar.IsEnabled = false;
                }

            }
        }

        private void cbTrajectesNoValidats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTrajectesNoValidats.SelectedItem != null && cbTrajectesNoValidats.HasItems && cbTrajectesNoValidats.IsLoaded)
            {

                Usuari us = dtgAdministradors.SelectedItem as Usuari;
                Trajecte traj = cbTrajectesNoValidats.SelectedItem as Trajecte;
                List<Parada> pars = new List<Parada>();

                MySQLFactory.getUOW(uow =>
                {
                    pars = uow.DAOParada.getParades("-1",""+(cbTrajectesNoValidats.SelectedItem as Trajecte).Id,"-1").ToList<Parada>();




                });


                String str = "Dades de trajecte seleccionat\n" +
                    "Editor: "+us.Nom
                    +"\n\nValidat: No"
                    +"\n\n";

                foreach(Parada par in pars)
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

                str += "\nConsulta:\n"+traj.Nom;

                txtTrajectesNoValidats.Text = str;
                



            }
            else
            {
                txtTrajectesNoValidats.Text = "";
            }
        }

        private void cbTrajectesValitats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTrajectesValidats.SelectedItem != null && cbTrajectesValidats.HasItems && cbTrajectesValidats.IsLoaded)
            {

                Usuari us = dtgAdministradors.SelectedItem as Usuari;
                Trajecte traj = cbTrajectesValidats.SelectedItem as Trajecte;
                List<Parada> pars = new List<Parada>();

                MySQLFactory.getUOW(uow =>
                {
                    pars = uow.DAOParada.getParades("-1", "" + (cbTrajectesValidats.SelectedItem as Trajecte).Id, "-1").ToList<Parada>();

                });

                String str = "Dades de trajecte seleccionat\n" +
                    "Editor: " + us.Nom
                    + "\n\nValidat: Si"
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

                txtTrajectesValidats.Text = str;




            }
            else
            {
                txtTrajectesValidats.Text = "";
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MySQLFactory.getUOW(uow =>
            {
                users = uow.DAOUsuari.getUsuaris("-1", "-1", "-1");
                users.Remove(user);
                dtgAdministradors.ItemsSource = users;

            });
        }

        private void btHabilitar_Click(object sender, RoutedEventArgs e)
        {
            if (dtgAdministradors.HasItems && dtgAdministradors.IsLoaded && dtgAdministradors.SelectedItem != null)
            {
                Usuari us = dtgAdministradors.SelectedItem as Usuari;
                if (us.Tipus != 'A')
                {
                
                    if (us.Tipus == 'B')
                    {
                        us.Tipus = 'E';
                    }
                    else
                    {
                        us.Tipus = 'B';
                    }

                    MySQLFactory.getUOW(uow =>
                    {
                        uow.DAOUsuari.updateUsuari(us);
                    
                        uow.Commit();
                    });

                    dtgAdministradors.Items.Refresh();
                }
        
            }
        }

        private void dtgAdministradors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtgAdministradors.HasItems && dtgAdministradors.IsLoaded && dtgAdministradors.SelectedItem != null)
            {
                Usuari us = dtgAdministradors.SelectedItem as Usuari;
                if (us.Tipus == 'B')
                {
                    btHabilitar.IsEnabled = true;
                    btHabilitar.Content = "Habilitar";
                } else if (us.Tipus == 'E')
                {
                    btHabilitar.IsEnabled = true;
                    btHabilitar.Content = "Deshabilitar";
                }
                else
                {
                    btHabilitar.IsEnabled = false;
                }

                MySQLFactory.getUOW(uow =>
                {
                    noValidats = uow.DAOTrajecte.getTrajectes("-1", "" + us.Id, "0");
                    cbTrajectesNoValidats.ItemsSource = noValidats;

                    if (cbTrajectesNoValidats.HasItems){
                        btValidar.IsEnabled = true;
                        cbTrajectesNoValidats.IsEnabled = true;
                        cbTrajectesNoValidats.SelectedIndex = 0;
                        
                    }
                    else
                    {
                        btValidar.IsEnabled = false;
                        cbTrajectesNoValidats.IsEnabled = false;
                    }

                    validats = uow.DAOTrajecte.getTrajectes("-1", "" + us.Id, "1");
                    cbTrajectesValidats.ItemsSource = validats;

                    if (cbTrajectesValidats.HasItems)
                    {
                        btDesvalidar.IsEnabled = true;
                        cbTrajectesValidats.IsEnabled = true;
                        cbTrajectesValidats.SelectedIndex = 0;
                    }
                    else
                    {
                        btDesvalidar.IsEnabled = false;
                        cbTrajectesValidats.IsEnabled = false;
                    }
                });


            }
        }
    }
}
