
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
    internal class DAOParada : IDAOParada
    {
        private readonly MyDBContext _context;

        public DAOParada(MyDBContext c)
        {
            _context = c;
        }

        public Parada getParada(String ordre_filtre, String trajecte_filtre, String origen_id = "")
        {
            ObservableCollection<Parada> oc =
                getParades(ordre_filtre, trajecte_filtre, origen_id);

            if (oc.Count == 0)
                return null;

            return oc[0];
        }


        public ObservableCollection<Parada> getParades(
            String ordre_filtre, String trajecte_filtre, String origen_id = "")
        {
            ObservableCollection<Parada> oc = new ObservableCollection<Parada>();

            var conexio = _context.Database.GetDbConnection();

            if (conexio.State == ConnectionState.Closed)
                conexio.Open();

            using (var comanda = conexio.CreateCommand())
            {
                comanda.CommandText = @"
            SELECT * FROM PARADA
            WHERE (:ORDRE_TEXT = -1 OR ORDRE = :ORDRE_TEXT)
            AND (:TRAJECTE_TEXT = -1 OR TRAJECTE = :TRAJECTE_TEXT)
            AND (:ORIGEN_ID = -1 OR ESTACIO_ID = :ORIGEN_ID) 
            ORDER BY ORDRE";

                if (comanda is Oracle.ManagedDataAccess.Client.OracleCommand oracleCommand)
                    oracleCommand.BindByName = true;

                int ordre = -1;
                int trajecte = -1;
                int origenId = -1;

                if (!Int32.TryParse(ordre_filtre, out ordre))
                    ordre = -1;

                if (!Int32.TryParse(trajecte_filtre, out trajecte))
                    trajecte = -1;

                if (!Int32.TryParse(origen_id, out origenId))
                    origenId = -1;

                var param = comanda.CreateParameter();
                param.ParameterName = "ORDRE_TEXT";
                param.Value = ordre;
                param.DbType = DbType.Int32;
                comanda.Parameters.Add(param);

                var param2 = comanda.CreateParameter();
                param2.ParameterName = "TRAJECTE_TEXT";
                param2.Value = trajecte;
                param2.DbType = DbType.Int32;
                comanda.Parameters.Add(param2);

                var param3 = comanda.CreateParameter();
                param3.ParameterName = "ORIGEN_ID";
                param3.Value = origenId;
                param3.DbType = DbType.Int32;
                comanda.Parameters.Add(param3);

                using (var reader = comanda.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int or = reader.GetInt32(reader.GetOrdinal("estacio_id"));
                        int tr = reader.GetInt32(reader.GetOrdinal("TRAJECTE"));
                        int ord = reader.GetInt32(reader.GetOrdinal("ordre"));
                        int hp = reader.GetInt32(reader.GetOrdinal("horapas"));

                        MySQLFactory.getUOW(uow =>
                        {
                            Estacio estacio = uow.DAOEstacio.getEstacio(
                                or.ToString(), "-1", "");

                            Trajecte trajecte = uow.DAOTrajecte.getTrajecte(
                                ""+tr);
                            
                            oc.Add(new Parada(
                                ord,
                                hp,
                                estacio,
                                trajecte
                            ));
                        });


                    }
                }
            }

            

            return oc;
        }

        public ObservableCollection<int> getTrajectesEntreEstacions(
    int origenId, int destiId, int horaSortida)
        {
            ObservableCollection<int> resultats = new ObservableCollection<int>();

            var conexio = _context.Database.GetDbConnection();

            if (conexio.State == ConnectionState.Closed)
                conexio.Open();

            using (var comanda = conexio.CreateCommand())
            {
                comanda.CommandText = @"
            SELECT DISTINCT T.ID
            FROM TRAJECTE T
            INNER JOIN PARADA PO
                ON PO.TRAJECTE = T.ID
            INNER JOIN PARADA PD
                ON PD.TRAJECTE = T.ID
            WHERE PO.ESTACIO_ID = :ORIGEN_ID
            AND PD.ESTACIO_ID = :DESTI_ID
            AND PD.ORDRE > PO.ORDRE
            AND PO.HORAPAS >= :HORA_SORTIDA
            AND PD.HORAPAS > PO.HORAPAS
            ORDER BY T.ID";

                if (comanda is Oracle.ManagedDataAccess.Client.OracleCommand oracleCommand)
                    oracleCommand.BindByName = true;

                var param1 = comanda.CreateParameter();
                param1.ParameterName = "ORIGEN_ID";
                param1.Value = origenId;
                param1.DbType = DbType.Int32;
                comanda.Parameters.Add(param1);

                var param2 = comanda.CreateParameter();
                param2.ParameterName = "DESTI_ID";
                param2.Value = destiId;
                param2.DbType = DbType.Int32;
                comanda.Parameters.Add(param2);

                var param3 = comanda.CreateParameter();
                param3.ParameterName = "HORA_SORTIDA";
                param3.Value = horaSortida;
                param3.DbType = DbType.Int32;
                comanda.Parameters.Add(param3);

                using (var reader = comanda.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        resultats.Add(Convert.ToInt32(reader["ID"]));
                    }
                }
            }

            return resultats;
        }

        private class ParadaDades
        {
            public int Ordre;
            public int HoraPas;
            public int EstacioId;
            public int TrajecteId;
        }

        public void insertParada(Parada parada)
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
                    INSERT INTO PARADA
                    (ORDRE, ESTACIO_ID, HORAPAS, TRAJECTE)
                    VALUES (:ORDRE_TEXT, :ESTACIO_TEXT,
                            :HORA_TEXT, :TRAJECTE_TEXT)";

                comanda.Transaction = transaction.GetDbTransaction();

                if (comanda is Oracle.ManagedDataAccess.Client.OracleCommand oracleCommand)
                    oracleCommand.BindByName = true;

                var param = comanda.CreateParameter();
                param.ParameterName = "ORDRE_TEXT";
                param.Value = parada.Ordre;
                param.DbType = DbType.Int32;
                comanda.Parameters.Add(param);

                var param2 = comanda.CreateParameter();
                param2.ParameterName = "ESTACIO_TEXT";
                param2.Value = parada.Estacio.Id;
                param2.DbType = DbType.Int32;
                comanda.Parameters.Add(param2);

                var param3 = comanda.CreateParameter();
                param3.ParameterName = "HORA_TEXT";
                param3.Value = parada.HoraPas;
                param3.DbType = DbType.Int32;
                comanda.Parameters.Add(param3);

                var param4 = comanda.CreateParameter();
                param4.ParameterName = "TRAJECTE_TEXT";
                param4.Value = parada.Trajecte.Id;
                param4.DbType = DbType.Int32;
                comanda.Parameters.Add(param4);

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
                            "Error en inserir la parada i fer rollback.",
                            ex, rollbackEx);
                    }

                    throw new AggregateException(
                        "Error en inserir la parada " + parada.Ordre + ".", ex);
                }
            }
        }

        public void deleteParada(String ordre_filtre, String trajecte_filtre)
        {
            var conexio = _context.Database.GetDbConnection();

            if (conexio.State == ConnectionState.Closed)
                conexio.Open();

            var transaction = _context.Database.CurrentTransaction;

            if (transaction == null)
                throw new InvalidOperationException("No hi ha cap transacció activa.");

            int ordre;
            int trajecte;

            if (!Int32.TryParse(ordre_filtre, out ordre))
                throw new InvalidOperationException("Ordre de la parada no valid.");

            if (!Int32.TryParse(trajecte_filtre, out trajecte))
                throw new InvalidOperationException("Id del trajecte no valid.");

            using (var comanda = conexio.CreateCommand())
            {
                comanda.CommandText = @"
                    DELETE FROM PARADA
                    WHERE ORDRE > :ORDRE_TEXT
                    AND TRAJECTE = :TRAJECTE_TEXT";

                comanda.Transaction = transaction.GetDbTransaction();

                var param = comanda.CreateParameter();
                param.ParameterName = "ORDRE_TEXT";
                param.Value = ordre;
                param.DbType = DbType.Int32;
                comanda.Parameters.Add(param);

                var param2 = comanda.CreateParameter();
                param2.ParameterName = "TRAJECTE_TEXT";
                param2.Value = trajecte;
                param2.DbType = DbType.Int32;
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
                            "Error en eliminar la parada i fer rollback.",
                            ex, rollbackEx);
                    }

                    throw new AggregateException(
                        "Error en eliminar la parada " + ordre + ".", ex);
                }
            }
        }

        public void updateParada(Parada parada)
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
            UPDATE PARADA
            SET ESTACIO_ID = :ESTACIO_TEXT,
                HORAPAS = :HORA_TEXT
            WHERE ORDRE = :ORDRE_TEXT
            AND TRAJECTE = :TRAJECTE_TEXT";

                comanda.Transaction = transaction.GetDbTransaction();

                if (comanda is Oracle.ManagedDataAccess.Client.OracleCommand oracleCommand)
                    oracleCommand.BindByName = true;

                var param = comanda.CreateParameter();
                param.ParameterName = "ESTACIO_TEXT";
                param.Value = parada.Estacio.Id;
                param.DbType = DbType.Int32;
                comanda.Parameters.Add(param);

                var param2 = comanda.CreateParameter();
                param2.ParameterName = "HORA_TEXT";
                param2.Value = parada.HoraPas;
                param2.DbType = DbType.Int32;
                comanda.Parameters.Add(param2);

                var param3 = comanda.CreateParameter();
                param3.ParameterName = "ORDRE_TEXT";
                param3.Value = parada.Ordre;
                param3.DbType = DbType.Int32;
                comanda.Parameters.Add(param3);

                var param4 = comanda.CreateParameter();
                param4.ParameterName = "TRAJECTE_TEXT";
                param4.Value = parada.Trajecte.Id;
                param4.DbType = DbType.Int32;
                comanda.Parameters.Add(param4);

                transaction.CreateSavepoint("sp1");

                try
                {
                    int filesAfectades = comanda.ExecuteNonQuery();

                    if (filesAfectades == 0)
                        throw new InvalidOperationException("No s'ha trobat la parada a modificar.");
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
                            "Error en modificar la parada i fer rollback.",
                            ex, rollbackEx);
                    }

                    throw new AggregateException(
                        "Error en modificar la parada " + parada.Ordre + ".", ex);
                }
            }
        }
    }
}
