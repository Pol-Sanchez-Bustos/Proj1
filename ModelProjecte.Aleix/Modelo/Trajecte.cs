using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    internal class Trajecte
    {
        int id;
        String nom;

        DateTime dataCreacio;
        int validat;
        MitjaTrans mitja;
        Usuari usuari;

        public Trajecte(int id, string nom, DateTime dataCreacio, int validat, MitjaTrans mitja, Usuari usuari)
        {
            Id = id;
            Nom = nom;
            DataCreacio = dataCreacio;
            Validat = validat;
            Mitja = mitja;
            Usuari = usuari;
        }

        public int Id { get => id; set => id = value; }
        public string Nom { get => nom; set => nom = value; }
        public DateTime DataCreacio { get => dataCreacio; set => dataCreacio = value; }
        public int Validat { get => validat; set => validat = value; }
        internal MitjaTrans Mitja { get => mitja; set => mitja = value; }
        internal Usuari Usuari { get => usuari; set => usuari = value; }
    }
}
