using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Trajecte
    {
        int id;
        String nom;

        DateTime dataCreacio;
        int validat;
        MitjaTrans mitja;
        Usuari usuari;

        String horaSortida, horaArribada;

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
        public MitjaTrans Mitja { get => mitja; set => mitja = value; }
        public Usuari Usuari { get => usuari; set => usuari = value; }
        public string HoraSortida { get => horaSortida; set => horaSortida = value; }
        public string HoraArribada { get => horaArribada; set => horaArribada = value; }
    }
}
