using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace IDao
{
    public interface IDAOMitjaTrans
    {
        void deleteMitjaTrans(string id_filtre);
        ObservableCollection<MitjaTrans> getMitjasTrans(string id_filtre);
        MitjaTrans getMitjaTrans(string id_filtre);
        void insertMitjaTrans(MitjaTrans mitja);
    }
}
