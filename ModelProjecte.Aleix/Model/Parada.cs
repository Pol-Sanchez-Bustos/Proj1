using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Parada
    {
        int ordre;
        Estacio estacio;
        int horaPas;
        Trajecte trajecte;

        public Parada(int ordre, int horaPas, Estacio estacio, Trajecte trajecte)
        {
            Ordre = ordre;
            HoraPas = horaPas;
            Estacio = estacio;
            Trajecte = trajecte;
        }

        public int Ordre { get => ordre; set => ordre = value; }
        public int HoraPas { get => horaPas; set => horaPas = value; }
        public string HoraPasFormat
        {
            get
            {
                int hores = HoraPas / 60;
                int minuts = HoraPas % 60;

                return hores + ":" + minuts.ToString("D2");

            }
            set
            {
                string[] parts = value.Split(':');

                int hores = int.Parse(parts[0]);
                int minuts = int.Parse(parts[1]);

                HoraPas = (hores * 60) + minuts;
            }
        }
        public Estacio Estacio { get => estacio; set => estacio = value; }
        public Trajecte Trajecte { get => trajecte; set => trajecte = value; }

        
    }
}
