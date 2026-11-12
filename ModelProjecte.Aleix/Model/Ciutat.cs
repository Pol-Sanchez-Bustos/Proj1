using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Ciutat
    {
        int id;
        String nom;

        public Ciutat(int id, string nom)
        {
            Id = id;
            Nom = nom;
        }

        public int Id { get => id; set => id = value; }
        public string Nom { get => nom; set => nom = value; }

        public override string ToString()
        {
            return ""+nom;
        }
    
    }
}
