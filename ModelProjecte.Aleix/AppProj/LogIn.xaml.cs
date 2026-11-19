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
    /// <summary>
    /// Lógica de interacción para LogIn.xaml
    /// </summary>
    public partial class LogIn : Window
    {
        public LogIn()
        {
            InitializeComponent();
        }

        private void btReturn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow ventana = new MainWindow();
            ventana.Show();
            this.Close();
        }

        private void btIniciSessio_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text;
            string password = txtPassword.Password;

            if (email == "" || password == "")
            {
                MessageBox.Show("Has d'omplir tots els camps");
            }
            else
            {
                int error_id = -1;
                Usuari user = null;
                try
                {
                    MySQLFactory.getUOW(uow =>
                    {
                        user = uow.DAOUsuari.getUsuariLogin(email, "-1");
                        if (user != null)
                        {
                            error_id = -2;
                            user = uow.DAOUsuari.getUsuariLogin(email, password);
                            if(user.Tipus == 'B')error_id = -3;
                        }
                    });
                }
                catch (Exception ex)
                {

                }

                if (user != null)
                {
                    MainWindow ventana = new MainWindow(user);
                    ventana.Show();

                    this.Close();
                }
                else
                {
                    if (error_id == -1) MessageBox.Show("Usuari no trobat:");
                    if (error_id == -2) MessageBox.Show("Contrasenya erronia:");
                    if (error_id == -3) MessageBox.Show("Usuari bloquejat:");


                }
            }
        }
    }
}
