using DAO;
using IDao;
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
        public MainWindow()
        {
            
            
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                MySQLFactory.getUOW(uow =>
                {
                    /*IDAOCiutat dao = uow.DAOCiutat;
                    Nom.Content = dao.getCiutat("2").Nom;
                    dtgEx.ItemsSource = dao.getCiutats("-1");*/

                    IDAOEstacio dao = uow.DAOEstacio;
                    Nom.Content = dao.getEstacio("2", "1").Nom;
                    dtgEx.ItemsSource = dao.getEstacions("","");
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}