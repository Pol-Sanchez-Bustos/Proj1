
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
    internal class DAOUsuari : IDAOUsuari
    {
        private readonly MyDBContext _context;

        public DAOUsuari(MyDBContext c)
        {
            _context = c;
        }

        public Usuari getUsuari(String id_filtre)
        {
            ObservableCollection<Usuari> oc = getUsuaris(id_filtre);

            if (oc.Count == 0)
                return null;

            return oc[0];
        }

        public Usuari getUsuariLogin(String email_filtre, String password_filtre)
        {
            ObservableCollection<Usuari> oc =
                getUsuaris("-1", email_filtre, password_filtre);

            if (oc.Count == 0)
                return null;

            return oc[0];
        }

        public ObservableCollection<Usuari> getUsuaris(
            String id_filtre,
            String email_filtre = "-1",
            String password_filtre = "-1")
        {
            ObservableCollection<Usuari> oc = new ObservableCollection<Usuari>();

            var conexio = _context.Database.GetDbConnection();

            if (conexio.State == ConnectionState.Closed)
                conexio.Open();

            using (var comanda = conexio.CreateCommand())
            {
                comanda.CommandText = @"
                    SELECT * FROM USUARI
                    WHERE (:ID_TEXT = -1 OR ID = :ID_TEXT)
                    AND (:EMAIL_TEXT = '-1' OR EMAIL = :EMAIL_TEXT)
                    AND (:PASSWORD_TEXT = '-1' OR PASSWORD = :PASSWORD_TEXT)";

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
                param2.ParameterName = "EMAIL_TEXT";
                param2.Value = email_filtre;
                param2.DbType = DbType.String;
                comanda.Parameters.Add(param2);

                var param3 = comanda.CreateParameter();
                param3.ParameterName = "PASSWORD_TEXT";
                param3.Value = password_filtre;
                param3.DbType = DbType.String;
                comanda.Parameters.Add(param3);

                using (var reader = comanda.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int idUsuari = Convert.ToInt32(reader["ID"]);
                        String email = Convert.ToString(reader["EMAIL"]);
                        String nom = Convert.ToString(reader["NOM"]);
                        String password = Convert.ToString(reader["PASSWORD"]);
                        Char tipus = Convert.ToChar(reader["TIPUS"]);
                        DateTime data = Convert.ToDateTime(reader["DATAREGISTRE"]);

                        oc.Add(new Usuari(
                            idUsuari,
                            email,
                            nom,
                            password,
                            tipus,
                            data
                        ));
                    }
                }
            }

            return oc;
        }



        public void insertUsuari(Usuari usuari)
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
                    INSERT INTO USUARI
                    (EMAIL, NOM, PASSWORD, TIPUS, DATAREGISTRE)
                    VALUES ( :EMAIL_TEXT, :NOM_TEXT,
                            :PASSWORD_TEXT, :TIPUS_TEXT, :DATA_TEXT)";

                comanda.Transaction = transaction.GetDbTransaction();

                if (comanda is Oracle.ManagedDataAccess.Client.OracleCommand oracleCommand)
                    oracleCommand.BindByName = true;

                
                var param2 = comanda.CreateParameter();
                param2.ParameterName = "EMAIL_TEXT";
                param2.Value = usuari.Email;
                param2.DbType = DbType.String;
                comanda.Parameters.Add(param2);

                var param3 = comanda.CreateParameter();
                param3.ParameterName = "NOM_TEXT";
                param3.Value = usuari.Nom;
                param3.DbType = DbType.String;
                comanda.Parameters.Add(param3);

                var param4 = comanda.CreateParameter();
                param4.ParameterName = "PASSWORD_TEXT";
                param4.Value = usuari.Password;
                param4.DbType = DbType.String;
                comanda.Parameters.Add(param4);

                var param5 = comanda.CreateParameter();
                param5.ParameterName = "TIPUS_TEXT";
                param5.Value = usuari.Tipus.ToString();
                param5.DbType = DbType.String;
                comanda.Parameters.Add(param5);

                var param6 = comanda.CreateParameter();
                param6.ParameterName = "DATA_TEXT";
                param6.Value = usuari.DataRegistre;
                param6.DbType = DbType.Date;
                comanda.Parameters.Add(param6);

                transaction.CreateSavepoint("sp1");

                try
                {
                    if(comanda.ExecuteNonQuery() != 1)
                    {
                        throw new AggregateException(
                            "Usuari no inserit.");
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
                            "Error en inserir l'usuari i fer rollback.",
                            ex, rollbackEx);
                    }

                    throw new AggregateException(
                        "Error en inserir l'usuari " + usuari.Id + ".", ex);
                }
            }
        }

        public void deleteUsuari(String id_filtre)
        {
            var conexio = _context.Database.GetDbConnection();

            if (conexio.State == ConnectionState.Closed)
                conexio.Open();

            var transaction = _context.Database.CurrentTransaction;

            if (transaction == null)
                throw new InvalidOperationException("No hi ha cap transacció activa.");

            int id;

            if (!Int32.TryParse(id_filtre, out id))
                throw new InvalidOperationException("Id de l'usuari no valid.");

            using (var comanda = conexio.CreateCommand())
            {
                comanda.CommandText = "DELETE FROM USUARI WHERE ID = :ID_TEXT";
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
                            "Error en eliminar l'usuari i fer rollback.",
                            ex, rollbackEx);
                    }

                    throw new AggregateException(
                        "Error en eliminar l'usuari " + id + ".", ex);
                }
            }
        }
    }
}
