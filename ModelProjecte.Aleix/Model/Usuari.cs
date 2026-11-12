using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Usuari
    {
        int id;
        String email, nom, password;
        char tipus;
        DateTime dataRegistre;

        public Usuari(int id, string email, string nom, string password, char tipus, DateTime dataRegistre)
        {
            Id = id;
            Email = email;
            Nom = nom;
            Password = password;
            Tipus = tipus;
            DataRegistre = dataRegistre;
        }

        public int Id { get => id; set => id = value; }
        public string Email { get => email; set => email = value; }
        public string Nom { get => nom; set => nom = value; }
        public string Password { get => password; set => password = value; }
        public char Tipus { get => tipus; set => tipus = value; }
        public DateTime DataRegistre { get => dataRegistre; set => dataRegistre = value; }
    }
}
