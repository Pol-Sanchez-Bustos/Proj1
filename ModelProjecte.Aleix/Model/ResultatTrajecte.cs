using System.Collections.ObjectModel;

namespace Model
{
    public class ResultatTrajecte
    {
        public Trajecte Trajecte1 { get; set; }
        public Trajecte Trajecte2 { get; set; }

        public Estacio EstacioTransbordo { get; set; }

        public int HoraSortida { get; set; }
        public int HoraArribada { get; set; }
        public int? HoraTransbordo { get; set; }

        public ObservableCollection<Parada> ParadesTrajecte1 { get; set; }
        public ObservableCollection<Parada> ParadesTrajecte2 { get; set; }

        public ResultatTrajecte()
        {
            ParadesTrajecte1 = new ObservableCollection<Parada>();
            ParadesTrajecte2 = new ObservableCollection<Parada>();
        }

        public string HoraSortidaFormat
        {
            get
            {
                int hores = HoraSortida / 60;
                int minuts = HoraSortida % 60;

                return hores + ":" + minuts.ToString("D2");
            }
        }

        public string HoraArribadaFormat
        {
            get
            {
                int hores = HoraArribada / 60;
                int minuts = HoraArribada % 60;

                return hores + ":" + minuts.ToString("D2");
            }
        }

        public string HoraTransbordoFormat
        {
            get
            {
                if (HoraTransbordo == null)
                    return "";

                int hores = HoraTransbordo.Value / 60;
                int minuts = HoraTransbordo.Value % 60;

                return hores + ":" + minuts.ToString("D2");
            }
        }
    }
}