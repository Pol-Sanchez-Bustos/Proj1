using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace IDao
{
    public interface IDAOTrajecte
    {
        void deleteTrajecte(string id_filtre);
        Trajecte getTrajecte(string id_filtre);
        public ObservableCollection<Trajecte> getTrajectes(String id_filtre, String id_usuari_filtre, String validat_filtre);
        ResultatTrajecte getResultatTrajecte(string idTrajecte, int origenId, int destiId);
        void insertTrajecte(Trajecte trajecte);
        void updateTrajecte(Trajecte trajecte);
    }
}
