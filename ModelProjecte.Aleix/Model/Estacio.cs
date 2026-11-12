using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Estacio
    {
        int id;
        Ciutat ciutat;
        String nom;
        Double latitud, longitud;

        public Estacio(int id, string nom, double latitud, double longitud, Ciutat ciutat)
        {
            Id = id;
            Nom = nom;
            Latitud = latitud;
            Longitud = longitud;
            Ciutat = ciutat;
        }

        public int Id { get => id; set => id = value; }
        public string Nom { get => nom; set => nom = value; }
        public double Latitud { get => latitud; set => latitud = value; }
        public double Longitud { get => longitud; set => longitud = value; }
        public Ciutat Ciutat { get => ciutat; set => ciutat = value; }

        


    }
}
