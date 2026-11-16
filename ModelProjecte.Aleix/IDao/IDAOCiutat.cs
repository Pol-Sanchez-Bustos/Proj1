using Model;
using System.Collections.ObjectModel;

namespace IDao
{
    public interface IDAOCiutat
    {
        void deleteCiutat(string id_filtre);
        Ciutat getCiutat(String id_filtre, String nom_filtre = "");
        ObservableCollection<Ciutat> getCiutats(String id_filtre, String nom_filtre = "");
        void insertCiutat(Ciutat ciutat);
    }
}