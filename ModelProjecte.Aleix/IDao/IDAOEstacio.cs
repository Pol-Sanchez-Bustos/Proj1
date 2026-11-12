using Model;
using System.Collections.ObjectModel;

namespace IDao
{
    public interface IDAOEstacio
    {
        void deleteEstacio(string id_filtre, string id_ciutat_filtre);
        Estacio getEstacio(string id_filtre, string id_ciutat_filtre);
        ObservableCollection<Estacio> getEstacions(string id_filtre, string id_ciutat_filtre);
        void insertEstacio(Estacio est);
    }
}