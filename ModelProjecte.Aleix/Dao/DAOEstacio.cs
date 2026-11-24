
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
    internal class DAOEstacio : IDAOEstacio
    {
        private readonly MyDBContext _context;

        public DAOEstacio(MyDBContext c)
        {
            _context = c;
        }

        public Estacio getEstacio(
        String id_filtre, String id_ciutat_filtre, String nom_filtre = "")
        {
            ObservableCollection<Estacio> oc = getEstacions(id_filtre, id_ciutat_filtre, nom_filtre);

            if (oc.Count == 0)
                return null;

            return oc[0];
        }


        public ObservableCollection<Estacio> getEstacions(
            String id_filtre, String id_ciutat_filtre, String nom_filtre = "")
        {
            ObservableCollection<Estacio> oc = new ObservableCollection<Estacio>();

            var conexio = _context.Database.GetDbConnection();

            if (conexio.State == ConnectionState.Closed)
                conexio.Open();

            using (var comanda = conexio.CreateCommand())
            {
                comanda.CommandText = @"
            SELECT * FROM ESTACIO
            WHERE (:ID_TEXT = -1 OR ID = :ID_TEXT)
            AND (:ID_CIUTAT_TEXT = -1 OR ID_CIUTAT = :ID_CIUTAT_TEXT)
            AND (:NOM_TEXT IS NULL OR UPPER(NOM) LIKE UPPER(:NOM_TEXT))
            ORDER BY ID";

                if (comanda is Oracle.ManagedDataAccess.Client.OracleCommand oracleCommand)
                    oracleCommand.BindByName = true;

                int id = -1;
                int idCiutat = -1;

                if (!Int32.TryParse(id_filtre, out id))
                    id = -1;

                if (!Int32.TryParse(id_ciutat_filtre, out idCiutat))
                    idCiutat = -1;

                var param = comanda.CreateParameter();
                param.ParameterName = "ID_TEXT";
                param.Value = id;
                param.DbType = DbType.Int32;
                comanda.Parameters.Add(param);

                var param2 = comanda.CreateParameter();
                param2.ParameterName = "ID_CIUTAT_TEXT";
                param2.Value = idCiutat;
                param2.DbType = DbType.Int32;
                comanda.Parameters.Add(param2);

                var param3 = comanda.CreateParameter();
                param3.ParameterName = "NOM_TEXT";
                param3.Value = String.IsNullOrEmpty(nom_filtre)
                    ? DBNull.Value
                    : "%" + nom_filtre + "%";
                param3.DbType = DbType.String;
                comanda.Parameters.Add(param3);

                using (var reader = comanda.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int idEstacio = Convert.ToInt32(reader["ID"]);
                        int idCity = Convert.ToInt32(reader["ID_CIUTAT"]);

                        String nom = Convert.ToString(reader["NOM"]);
                        Double lat = Convert.ToDouble(reader["LATITUD"]);
                        Double lon = Convert.ToDouble(reader["LONGITUD"]);

                        Ciutat ciutat = null;

                        DAOCiutat daoCiutat = new DAOCiutat(_context);
                        ciutat = daoCiutat.getCiutat(""+idCity,"");

                        Estacio estacio = new Estacio(
                            idEstacio,
                            nom,
                            lat,
                            lon,
                            ciutat
                        );

                        oc.Add(estacio);
                    }
                }
            }

            return oc;
        }


        public void insertEstacio(Estacio est)
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
            INSERT INTO ESTACIO
            (ID_CIUTAT, NOM, LATITUD, LONGITUD)
            VALUES (:ID_CIUTAT_TEXT, :NOM_TEXT, :LAT_TEXT, :LON_TEXT)";

                comanda.Transaction = transaction.GetDbTransaction();

                if (comanda is Oracle.ManagedDataAccess.Client.OracleCommand oracleCommand)
                    oracleCommand.BindByName = true;

                var param2 = comanda.CreateParameter();
                param2.ParameterName = "ID_CIUTAT_TEXT";
                param2.Value = est.Ciutat.Id;
                param2.DbType = System.Data.DbType.Int32;
                comanda.Parameters.Add(param2);

                var param3 = comanda.CreateParameter();
                param3.ParameterName = "NOM_TEXT";
                param3.Value = est.Nom;
                param3.DbType = System.Data.DbType.String;
                comanda.Parameters.Add(param3);

                var param4 = comanda.CreateParameter();
                param4.ParameterName = "LAT_TEXT";
                param4.Value = est.Latitud;
                param4.DbType = System.Data.DbType.Double;
                comanda.Parameters.Add(param4);

                var param5 = comanda.CreateParameter();
                param5.ParameterName = "LON_TEXT";
                param5.Value = est.Longitud;
                param5.DbType = System.Data.DbType.Double;
                comanda.Parameters.Add(param5);

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
                            "Error en inserir l'estacio i fer rollback.",
                            ex, rollbackEx);
                    }

                    throw new AggregateException(
                        "Error en inserir l'estacio.", ex);
                }
            }
        }

        public void deleteEstacio(String id_filtre, String id_ciutat_filtre)
        {
            var conexio = _context.Database.GetDbConnection();

            if (conexio.State == ConnectionState.Closed)
                conexio.Open();

            var transaction = _context.Database.CurrentTransaction;

            if (transaction == null)
                throw new InvalidOperationException("No hi ha cap transacció activa.");

            int id;
            int idCiutat;

            if (!Int32.TryParse(id_filtre, out id))
                throw new InvalidOperationException("Id de l'estacio no valida.");

            if (!Int32.TryParse(id_ciutat_filtre, out idCiutat))
                throw new InvalidOperationException("Id de la ciutat no valida.");

            using (var comanda = conexio.CreateCommand())
            {
                comanda.CommandText = @"
                    DELETE FROM ESTACIO
                    WHERE ID = :ID_TEXT AND ID_CIUTAT = :ID_CIUTAT_TEXT";

                comanda.Transaction = transaction.GetDbTransaction();

                var param = comanda.CreateParameter();
                param.ParameterName = "ID_TEXT";
                param.Value = id;
                param.DbType = System.Data.DbType.Int32;
                comanda.Parameters.Add(param);

                var param2 = comanda.CreateParameter();
                param2.ParameterName = "ID_CIUTAT_TEXT";
                param2.Value = idCiutat;
                param2.DbType = System.Data.DbType.Int32;
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
                            "Error en eliminar l'estacio i fer rollback.",
                            ex, rollbackEx);
                    }

                    throw new AggregateException(
                        "Error en eliminar l'estacio " + id + ".", ex);
                }
            }
        }
    }
}
