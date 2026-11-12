using Model;
using System.Collections.ObjectModel;

namespace IDao
{
    public interface IDAOParada
    {
        void deleteParada(string ordre_filtre, string trajecte_filtre);
        Parada getParada(string ordre_filtre, string trajecte_filtre);
        ObservableCollection<Parada> getParades(string ordre_filtre, string trajecte_filtre);
        void insertParada(Parada parada);
    }
}