using Model;
using System.Collections.ObjectModel;

namespace IDao
{
    public interface IDAOCiutat
    {
        void deleteCiutat(string id_filtre);
        Ciutat getCiutat(string id_filtre);
        ObservableCollection<Ciutat> getCiutats(string id_filtre);
        void insertCiutat(Ciutat ciutat);
    }
}