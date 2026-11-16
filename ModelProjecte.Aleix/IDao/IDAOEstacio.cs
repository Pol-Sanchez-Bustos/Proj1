using Model;
using System.Collections.ObjectModel;

namespace IDao
{
    public interface IDAOEstacio
    {
        void deleteEstacio(string id_filtre, string id_ciutat_filtre);
        public Estacio getEstacio(
        String id_filtre, String id_ciutat_filtre, String nom_filtre = "");
        public ObservableCollection<Estacio> getEstacions(
        String id_filtre, String id_ciutat_filtre, String nom_filtre = "");
        void insertEstacio(Estacio est);
    }
}