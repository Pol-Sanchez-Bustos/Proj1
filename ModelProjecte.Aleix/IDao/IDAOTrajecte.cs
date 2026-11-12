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
        ObservableCollection<Trajecte> getTrajectes(string id_filtre);
        void insertTrajecte(Trajecte trajecte);
    }
}
