using Model;
using System.Collections.ObjectModel;

namespace IDao
{
    public interface IDAOUsuari
    {
        void deleteUsuari(string id_filtre);
        Usuari getUsuari(string id_filtre);
        Usuari getUsuariLogin(string email_filtre, string password_filtre);
        ObservableCollection<Usuari> getUsuaris(string id_filtre, string email_filtre = "-1", string password_filtre = "-1");
        void insertUsuari(Usuari usuari);

        void updateUsuari(Usuari usuari);
    }
}