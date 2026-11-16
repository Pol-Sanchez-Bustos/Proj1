using Model;
using System.Collections.ObjectModel;

namespace IDao
{
    public interface IDAOParada
    {
        void deleteParada(string ordre_filtre, string trajecte_filtre);
        Parada getParada(String ordre_filtre, String trajecte_filtre, String origen_id = "");
        public ObservableCollection<Parada> getParades(
            String ordre_filtre, String trajecte_filtre, String origen_id = "");
        ObservableCollection<int> getTrajectesEntreEstacions(
    int origenId, int destiId, int horaSortida);

        void insertParada(Parada parada);
    }
}