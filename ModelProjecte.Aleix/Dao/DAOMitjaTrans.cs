
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
    internal class DAOMitjaTrans : IDAOMitjaTrans
    {
        private readonly MyDBContext _context;

        public DAOMitjaTrans(MyDBContext c)
        {
            _context = c;
        }

        public MitjaTrans getMitjaTrans(String id_filtre)
        {
            ObservableCollection<MitjaTrans> oc = getMitjasTrans(id_filtre);

            if (oc.Count == 0)
                return null;

            return oc[0];
        }

        public ObservableCollection<MitjaTrans> getMitjasTrans(String id_filtre)
        {
            ObservableCollection<MitjaTrans> oc = new ObservableCollection<MitjaTrans>();

            var conexio = _context.Database.GetDbConnection();

            if (conexio.State == ConnectionState.Closed)
                conexio.Open();

            using (var comanda = conexio.CreateCommand())
            {
                comanda.CommandText = @"
                    SELECT * FROM MITJA_TRANS
                    WHERE (:ID_TEXT = -1 OR ID = :ID_TEXT)";

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

                using (var reader = comanda.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int idMitja = Convert.ToInt32(reader["ID"]);
                        String nom = Convert.ToString(reader["NOM"]);

                        oc.Add(new MitjaTrans(idMitja, nom));
                    }
                }
            }

            return oc;
        }

        public void insertMitjaTrans(MitjaTrans mitja)
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
                    INSERT INTO MITJA_TRANS (ID, NOM)
                    VALUES (:ID_TEXT, :NOM_TEXT)";

                comanda.Transaction = transaction.GetDbTransaction();

                if (comanda is Oracle.ManagedDataAccess.Client.OracleCommand oracleCommand)
                    oracleCommand.BindByName = true;

                var param = comanda.CreateParameter();
                param.ParameterName = "ID_TEXT";
                param.Value = mitja.Id;
                param.DbType = DbType.Int32;
                comanda.Parameters.Add(param);

                var param2 = comanda.CreateParameter();
                param2.ParameterName = "NOM_TEXT";
                param2.Value = mitja.Nom;
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
                            "Error en inserir el mitja i fer rollback.",
                            ex, rollbackEx);
                    }

                    throw new AggregateException(
                        "Error en inserir el mitja " + mitja.Id + ".", ex);
                }
            }
        }

        public void deleteMitjaTrans(String id_filtre)
        {
            var conexio = _context.Database.GetDbConnection();

            if (conexio.State == ConnectionState.Closed)
                conexio.Open();

            var transaction = _context.Database.CurrentTransaction;

            if (transaction == null)
                throw new InvalidOperationException("No hi ha cap transacció activa.");

            int id;

            if (!Int32.TryParse(id_filtre, out id))
                throw new InvalidOperationException("Id del mitja no valid.");

            using (var comanda = conexio.CreateCommand())
            {
                comanda.CommandText = "DELETE FROM MITJA_TRANS WHERE ID = :ID_TEXT";
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
                            "Error en eliminar el mitja i fer rollback.",
                            ex, rollbackEx);
                    }

                    throw new AggregateException(
                        "Error en eliminar el mitja " + id + ".", ex);
                }
            }
        }
    }
}
