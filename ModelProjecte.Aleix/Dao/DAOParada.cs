
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

        public Parada getParada(String ordre_filtre, String trajecte_filtre)
        {
            ObservableCollection<Parada> oc =
                getParades(ordre_filtre, trajecte_filtre);

            if (oc.Count == 0)
                return null;

            return oc[0];
        }

        public ObservableCollection<Parada> getParades(
            String ordre_filtre, String trajecte_filtre)
        {
            ObservableCollection<Parada> oc = new ObservableCollection<Parada>();

            var conexio = _context.Database.GetDbConnection();

            if (conexio.State == ConnectionState.Closed)
                conexio.Open();

            var dades = new List<ParadaDades>();

            using (var comanda = conexio.CreateCommand())
            {
                comanda.CommandText = @"
                    SELECT * FROM PARADA
                    WHERE (:ORDRE_TEXT = -1 OR ORDRE = :ORDRE_TEXT)
                    AND (:TRAJECTE_TEXT = -1 OR TRAJECTE = :TRAJECTE_TEXT)";

                if (comanda is Oracle.ManagedDataAccess.Client.OracleCommand oracleCommand)
                    oracleCommand.BindByName = true;

                int ordre = -1;
                int trajecte = -1;

                if (!Int32.TryParse(ordre_filtre, out ordre))
                    ordre = -1;

                if (!Int32.TryParse(trajecte_filtre, out trajecte))
                    trajecte = -1;

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

                using (var reader = comanda.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dades.Add(new ParadaDades
                        {
                            Ordre = Convert.ToInt32(reader["ORDRE"]),
                            HoraPas = Convert.ToInt32(reader["HORAPAS"]),
                            EstacioId = Convert.ToInt32(reader["ESTACIO_ID"]),
                            TrajecteId = Convert.ToInt32(reader["TRAJECTE"])
                        });
                    }
                }
            }

            foreach (var d in dades)
            {
                Estacio estacio = null;
                Trajecte trajecte = null;

                MySQLFactory.getUOW(uow =>
                {
                    estacio = uow.DAOEstacio.getEstacio(
                        d.EstacioId.ToString(), "-1");

                    trajecte = uow.DAOTrajecte.getTrajecte(
                        d.TrajecteId.ToString());
                });

                oc.Add(new Parada(
                    d.Ordre,
                    d.HoraPas,
                    estacio,
                    trajecte
                ));
            }

            return oc;
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
                    WHERE ORDRE = :ORDRE_TEXT
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
    }
}
