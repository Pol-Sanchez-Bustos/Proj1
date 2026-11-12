using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    internal class Parada
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
        internal Estacio Estacio { get => estacio; set => estacio = value; }
        internal Trajecte Trajecte { get => trajecte; set => trajecte = value; }
    }
}
