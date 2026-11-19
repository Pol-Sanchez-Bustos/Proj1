
using DAO;
using Model;
using System;
using System.Collections.Generic;
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
        public Administrador(Usuari newUser)
        {
            user = newUser;
            InitializeComponent();
        }

        

        private void btValidar_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btDesvalidar_Click(object sender, RoutedEventArgs e)
        {
        }

        private void cmbTrajectesNoVisibles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void cmbTrajectesValitats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
                dtgAdministradors.ItemsSource = uow.DAOUsuari.getUsuaris("-1", "-1", "-1");


            });
        }
    }
}
