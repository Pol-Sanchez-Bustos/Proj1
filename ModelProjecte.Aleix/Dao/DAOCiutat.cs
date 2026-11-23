using DAO;
using IDao;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Model;
using System;
using System.Collections.ObjectModel;
using System.Data;

namespace Dao
{
    internal class DAOCiutat : IDAOCiutat
    {
        private readonly MyDBContext _context;

        public DAOCiutat(MyDBContext c)
        {
            _context = c;
        }

        public Ciutat getCiutat(String id_filtre, String nom_filtre = "")
        {
            ObservableCollection<Ciutat> oc = getCiutats(id_filtre, nom_filtre);

            if (oc.Count == 0)
                return null;

            return oc[0];
        }

        public ObservableCollection<Ciutat> getCiutats(String id_filtre, String nom_filtre = "")
        {
            ObservableCollection<Ciutat> oc = new ObservableCollection<Ciutat>();

            var conexio = _context.Database.GetDbConnection();

            if (conexio.State == ConnectionState.Closed)
                conexio.Open();

            using (var comanda = conexio.CreateCommand())
            {
                comanda.CommandText = @"
                    SELECT * FROM CIUTAT
                    WHERE (:ID_TEXT = -1 OR ID = :ID_TEXT)
                    AND (:NOM_TEXT IS NULL OR UPPER(NOM) LIKE UPPER(:NOM_TEXT))";

                if (comanda is Oracle.ManagedDataAccess.Client.OracleCommand oracleCommand)
                    oracleCommand.BindByName = true;

                int id = -1;

                if (!Int32.TryParse(id_filtre, out id))
                    id = -1;

                var param = comanda.CreateParameter();
                param.ParameterName = "ID_TEXT";
                param.Value = id;
                param.DbType = DbType.Int32;
                comanda.Parameters.Add(param);

                var param2 = comanda.CreateParameter();
                param2.ParameterName = "NOM_TEXT";

                param2.Value = String.IsNullOrWhiteSpace(nom_filtre)
                    ? DBNull.Value
                    : "%" + nom_filtre + "%";

                param2.DbType = DbType.String;
                comanda.Parameters.Add(param2);

                using (var reader = comanda.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int idCiutat = Convert.ToInt32(reader["ID"]);
                        String nom = Convert.ToString(reader["NOM"]);

                        oc.Add(new Ciutat(idCiutat, nom));
                    }
                }
            }

            return oc;
        }

        public void insertCiutat(Ciutat ciutat)
        {
            var conexio = _context.Database.GetDbConnection();

            if (conexio.State == ConnectionState.Closed)
                conexio.Open();

            var transaction = _context.Database.CurrentTransaction;

            if (transaction == null)
                throw new InvalidOperationException("No hi ha cap transacció activa.");

            using (var comanda = conexio.CreateCommand())
            {
                comanda.CommandText = @"
                    INSERT INTO CIUTAT (ID, NOM)
                    VALUES (:ID_TEXT, :NOM_TEXT)";

                comanda.Transaction = transaction.GetDbTransaction();

                if (comanda is Oracle.ManagedDataAccess.Client.OracleCommand oracleCommand)
                    oracleCommand.BindByName = true;

                var param = comanda.CreateParameter();
                param.ParameterName = "ID_TEXT";
                param.Value = ciutat.Id;
                param.DbType = DbType.Int32;
                comanda.Parameters.Add(param);

                var param2 = comanda.CreateParameter();
                param2.ParameterName = "NOM_TEXT";
                param2.Value = ciutat.Nom;
                param2.DbType = DbType.String;
                comanda.Parameters.Add(param2);

                transaction.CreateSavepoint("sp1");

                try
                {
                    comanda.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    try
                    {
                        transaction.RollbackToSavepoint("sp1");
                    }
                    catch (Exception rollbackEx)
                    {
                        throw new AggregateException(
                            "Error en inserir la ciutat i fer rollback.",
                            ex, rollbackEx);
                    }

                    throw new AggregateException(
                        "Error en inserir la ciutat " + ciutat.Id + ".", ex);
                }
            }
        }

        public void deleteCiutat(String id_filtre)
        {
            var conexio = _context.Database.GetDbConnection();

            if (conexio.State == ConnectionState.Closed)
                conexio.Open();

            var transaction = _context.Database.CurrentTransaction;

            if (transaction == null)
                throw new InvalidOperationException("No hi ha cap transacció activa.");

            int id;

            if (!Int32.TryParse(id_filtre, out id))
                throw new InvalidOperationException("Id de la ciutat no valida.");

            using (var comanda = conexio.CreateCommand())
            {
                comanda.CommandText = "DELETE FROM CIUTAT WHERE ID = :ID_TEXT";
                comanda.Transaction = transaction.GetDbTransaction();

                var param = comanda.CreateParameter();
                param.ParameterName = "ID_TEXT";
                param.Value = id;
                param.DbType = DbType.Int32;
                comanda.Parameters.Add(param);

                transaction.CreateSavepoint("sp1");

                try
                {
                    comanda.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    try
                    {
                        transaction.RollbackToSavepoint("sp1");
                    }
                    catch (Exception rollbackEx)
                    {
                        throw new AggregateException(
                            "Error en eliminar la ciutat i fer rollback.",
                            ex, rollbackEx);
                    }

                    throw new AggregateException(
                        "Error en eliminar la ciutat " + id + ".", ex);
                }
            }
        }
    }
}