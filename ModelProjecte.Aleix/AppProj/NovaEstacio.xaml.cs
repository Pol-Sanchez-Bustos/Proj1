using DAO;
using Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AppProj
{
    public partial class NovaEstacio : Window
    {
        public Estacio EstacioCreada { get; private set; }

        public NovaEstacio()
        {
            InitializeComponent();
        }

        private void btGuardarCanvis_Click(object sender, RoutedEventArgs e)
        {
            double latitud;
            double longitud;

            bool correcte = true;

            if (txtCiutat.Text.Trim() == "")
            {
                txtCiutat.BorderBrush = Brushes.IndianRed;
                correcte = false;
            }

            if (txtNom.Text.Trim() == "")
            {
                txtNom.BorderBrush = Brushes.IndianRed;
                correcte = false;
            }


            if (!double.TryParse(txtLatitud.Text, out latitud))
            {
                txtLatitud.BorderBrush = Brushes.IndianRed;
                correcte = false;
            }

            if (!double.TryParse(txtLongitud.Text, out longitud))
            {
                txtLongitud.BorderBrush = Brushes.IndianRed;
                correcte = false;
            }


            if (!correcte)
            {
                MessageBox.Show("Hi ha camps buits o dades incorrectes.");
                return;
            }

            try
            {
                bool creada = false;

                MySQLFactory.getUOW(uow =>
                {
                    Ciutat ciutat = uow.DAOCiutat.getCiutat("-1", txtCiutat.Text.Trim());

                    if (ciutat == null)
                    {
                        MessageBox.Show("La ciutat '" + txtCiutat.Text.Trim() + "' no existeix a la base de dades.");
                        return;
                    }

                    Estacio est = new Estacio(
                        0,
                        txtNom.Text.Trim(),
                        latitud,
                        longitud,
                        ciutat
                    );

                    uow.DAOEstacio.insertEstacio(est);

                    uow.Commit();

                    EstacioCreada = est;
                    creada = true;
                });

                if (creada)
                {
                    MessageBox.Show("L'estació s'ha creat correctament.");
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No s'ha pogut crear l'estació.\n\nError: " + ex.Message,
                    "Error en crear l'estació",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void btCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = sender as TextBox;

            
            txt.BorderBrush = Brushes.Gray;
            
        }
    }
}