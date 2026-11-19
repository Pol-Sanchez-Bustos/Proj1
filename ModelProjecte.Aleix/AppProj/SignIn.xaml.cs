
using DAO;
using Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
    /// Lógica de interacción para SignIn.xaml
    /// </summary>
    public partial class SignIn : Window
    {
        public SignIn()
        {
            InitializeComponent();
        }

        private void btReturn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow ventana = new MainWindow();
            ventana.Show();
            this.Close();
        }

        private void btRegistrar_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text;
            string nom = txtNom.Text;
            string password = txtPassword.Password;

            if (email == "" || nom == "" || password == "")
            {
                MessageBox.Show("Has d'omplir tots els camps");
            }
            else
            {
                int id = -1;
                try { 
                    MySQLFactory.getUOW(uow =>
                    {
                        if(uow.DAOUsuari.getUsuariLogin(email,"-1") != null)
                        {
                            id = 1;
                        }
                    });
                }catch (Exception ex){

                }

            if (id == -1)
                {
                    Usuari user = new Usuari(email, nom, password, 'E', DateTime.Now);
                    try
                    {
                        MySQLFactory.getUOW(uow =>
                        {
                            uow.DAOUsuari.insertUsuari(user);

                            uow.Commit();
                        });

                        MainWindow ventana = new MainWindow(user);
                        ventana.Show();

                        this.Close();

                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show("Error al inserir l'usuari:\n" + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Correu ja registrat:");
                    
                }
                
            }
        }
    }
}
