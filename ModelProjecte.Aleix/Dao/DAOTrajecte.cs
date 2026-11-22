
using DAO;
using IDao;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;

namespace Dao
{
    internal class DAOTrajecte : IDAOTrajecte
    {
        private readonly MyDBContext _context;

        public DAOTrajecte(MyDBContext c)
        {
            _context = c;
        }

        public Trajecte getTrajecte(String id_filtre)
        {
            ObservableCollection<Trajecte> oc = getTrajectes(id_filtre, "-1","-1");

            if (oc.Count == 0)
                return null;

            return oc[0];
        }

        public ObservableCollection<Trajecte> getTrajectes(String id_filtre, String id_usuari_filtre, String validat_filtre)
        {
            ObservableCollection<Trajecte> oc = new ObservableCollection<Trajecte>();

            var conexio = _context.Database.GetDbConnection();

            if (conexio.State == ConnectionState.Closed)
                conexio.Open();

            var dades = new List<TrajecteDades>();

            using (var comanda = conexio.CreateCommand())
            {
                comanda.CommandText = @"
                    SELECT * FROM TRAJECTE
                    WHERE (:ID_TEXT = -1 OR ID = :ID_TEXT) AND
                            (:ID_USUARI_TEXT = -1 OR :ID_USUARI_TEXT = USUARI_ID) AND
                            (:VALIDAT_TEXT = -1 OR VALIDAT = :VALIDAT_TEXT)";

                if (comanda is Oracle.ManagedDataAccess.Client.OracleCommand oracleCommand)
                    oracleCommand.BindByName = true;

                int id = -1, id_usuari = -1, validat = -1;

                if (!Int32.TryParse(id_filtre, out id))
                    id = -1;

                if (!Int32.TryParse(id_usuari_filtre, out id_usuari))
                    id_usuari = -1;

                if (!Int32.TryParse(validat_filtre, out validat))
                    validat = -1;

                var param = comanda.CreateParameter();
                param.ParameterName = "ID_TEXT";
                param.Value = id;
                param.DbType = DbType.Int32;
                comanda.Parameters.Add(param);

                var param2 = comanda.CreateParameter();
                param2.ParameterName = "ID_USUARI_TEXT";
                param2.Value = id_usuari;
                param2.DbType = DbType.Int32;
                comanda.Parameters.Add(param2);

                var param3 = comanda.CreateParameter();
                param3.ParameterName = "VALIDAT_TEXT";
                param3.Value = validat;
                param3.DbType = DbType.Int32;
                comanda.Parameters.Add(param3);

                using (var reader = comanda.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dades.Add(new TrajecteDades
                        {
                            Id = Convert.ToInt32(reader["ID"]),
                            Nom = Convert.ToString(reader["NOM"]),
                            DataCreacio = Convert.ToDateTime(reader["DATACREACIO"]),
                            Validat = Convert.ToInt32(reader["VALIDAT"]),
                            MitjaId = Convert.ToInt32(reader["MITJA_ID"]),
                            UsuariId = Convert.ToInt32(reader["USUARI_ID"])
                        });
                    }
                }
            }

            foreach (var d in dades)
            {
                MitjaTrans mitja = null;
                Usuari usuari = null;

                MySQLFactory.getUOW(uow =>
                {
                    mitja = uow.DAOMitjaTrans.getMitjaTrans(
                        d.MitjaId.ToString());

                    usuari = uow.DAOUsuari.getUsuari(
                        d.UsuariId.ToString());
                });

                oc.Add(new Trajecte(
                    d.Id,
                    d.Nom,
                    d.DataCreacio,
                    d.Validat,
                    mitja,
                    usuari
                ));
            }

            return oc;
        }

        private class TrajecteDades
        {
            public int Id;
            public String Nom;
            public DateTime DataCreacio;
            public int Validat;
            public int MitjaId;
            public int UsuariId;
        }


        public void insertTrajecte(Trajecte trajecte)
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
            INSERT INTO TRAJECTE
            (NOM, DATACREACIO, VALIDAT, MITJA_ID, USUARI_ID)
            VALUES (:NOM_TEXT, :DATA_TEXT,
                    :VALIDAT_TEXT, :MITJA_TEXT, :USUARI_TEXT)";

                comanda.Transaction = transaction.GetDbTransaction();

                if (comanda is Oracle.ManagedDataAccess.Client.OracleCommand oracleCommand)
                    oracleCommand.BindByName = true;

                var param2 = comanda.CreateParameter();
                param2.ParameterName = "NOM_TEXT";
                param2.Value = trajecte.Nom;
                param2.DbType = DbType.String;
                comanda.Parameters.Add(param2);

                var param3 = comanda.CreateParameter();
                param3.ParameterName = "DATA_TEXT";
                param3.Value = trajecte.DataCreacio;
                param3.DbType = DbType.Date;
                comanda.Parameters.Add(param3);

                var param4 = comanda.CreateParameter();
                param4.ParameterName = "VALIDAT_TEXT";
                param4.Value = trajecte.Validat;
                param4.DbType = DbType.Int32;
                comanda.Parameters.Add(param4);

                var param5 = comanda.CreateParameter();
                param5.ParameterName = "MITJA_TEXT";
                param5.Value = trajecte.Mitja.Id;
                param5.DbType = DbType.Int32;
                comanda.Parameters.Add(param5);

                var param6 = comanda.CreateParameter();
                param6.ParameterName = "USUARI_TEXT";
                param6.Value = trajecte.Usuari.Id;
                param6.DbType = DbType.Int32;
                comanda.Parameters.Add(param6);

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
                            "Error en inserir el trajecte i fer rollback.",
                            ex, rollbackEx);
                    }

                    throw new AggregateException(
                        "Error en inserir el trajecte.", ex);
                }
            }
        }


        public void deleteTrajecte(String id_filtre)
        {
            var conexio = _context.Database.GetDbConnection();

            if (conexio.State == ConnectionState.Closed)
                conexio.Open();

            var transaction = _context.Database.CurrentTransaction;

            if (transaction == null)
                throw new InvalidOperationException("No hi ha cap transacció activa.");

            int id;

            if (!Int32.TryParse(id_filtre, out id))
                throw new InvalidOperationException("Id del trajecte no valid.");

            using (var comanda = conexio.CreateCommand())
            {
                comanda.CommandText = "DELETE FROM TRAJECTE WHERE ID = :ID_TEXT";
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
                            "Error en eliminar el trajecte i fer rollback.",
                            ex, rollbackEx);
                    }

                    throw new AggregateException(
                        "Error en eliminar el trajecte " + id + ".", ex);
                }
            }
        }
        public void updateTrajecte(Trajecte trajecte)
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
            UPDATE TRAJECTE
            SET NOM = :NOM_TEXT,
                DATACREACIO = :DATA_TEXT,
                VALIDAT = :VALIDAT_TEXT,
                MITJA_ID = :MITJA_TEXT,
                USUARI_ID = :USUARI_TEXT
            WHERE ID = :ID_TEXT";

                comanda.Transaction = transaction.GetDbTransaction();

                if (comanda is Oracle.ManagedDataAccess.Client.OracleCommand oracleCommand)
                    oracleCommand.BindByName = true;

                var param = comanda.CreateParameter();
                param.ParameterName = "NOM_TEXT";
                param.Value = trajecte.Nom;
                param.DbType = DbType.String;
                comanda.Parameters.Add(param);

                var param2 = comanda.CreateParameter();
                param2.ParameterName = "DATA_TEXT";
                param2.Value = trajecte.DataCreacio;
                param2.DbType = DbType.Date;
                comanda.Parameters.Add(param2);

                var param3 = comanda.CreateParameter();
                param3.ParameterName = "VALIDAT_TEXT";
                param3.Value = trajecte.Validat;
                param3.DbType = DbType.Int32;
                comanda.Parameters.Add(param3);

                var param4 = comanda.CreateParameter();
                param4.ParameterName = "MITJA_TEXT";
                param4.Value = trajecte.Mitja.Id;
                param4.DbType = DbType.Int32;
                comanda.Parameters.Add(param4);

                var param5 = comanda.CreateParameter();
                param5.ParameterName = "USUARI_TEXT";
                param5.Value = trajecte.Usuari.Id;
                param5.DbType = DbType.Int32;
                comanda.Parameters.Add(param5);

                var param6 = comanda.CreateParameter();
                param6.ParameterName = "ID_TEXT";
                param6.Value = trajecte.Id;
                param6.DbType = DbType.Int32;
                comanda.Parameters.Add(param6);

                transaction.CreateSavepoint("sp1");

                try
                {
                    if (comanda.ExecuteNonQuery() != 1)
                    {
                        transaction.RollbackToSavepoint("sp1");
                        throw new AggregateException("Trajecte no actualitzat.");
                    }
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
                            "Error en actualitzar el trajecte i fer rollback.",
                            ex, rollbackEx);
                    }

                    throw new AggregateException(
                        "Error en actualitzar el trajecte " + trajecte.Id + ".", ex);
                }
            }
        }
    }
}
