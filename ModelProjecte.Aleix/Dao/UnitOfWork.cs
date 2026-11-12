using Dao;
using IDao;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAO
{
    public class UnitOfWork : IDisposable
    {
        private readonly MyDBContext _context;


        public IDAOCiutat DAOCiutat { get; }
        public IDAOEstacio DAOEstacio { get; }
        public IDAOUsuari DAOUsuari { get; }
        public IDAOMitjaTrans DAOMitjaTrans { get; }
        public IDAOTrajecte DAOTrajecte { get; }
        public IDAOParada DAOParada { get; }

        public UnitOfWork(MyDBContext context)
        {
            _context = context;

            DAOCiutat = new DAOCiutat(_context);
            DAOEstacio = new DAOEstacio(_context);
            DAOUsuari = new DAOUsuari(_context);
            DAOMitjaTrans = new DAOMitjaTrans(_context);
            DAOTrajecte = new DAOTrajecte(_context);
            DAOParada = new DAOParada(_context);
        }
        }

        public void BeginTransaction() => _context.Database.BeginTransaction();
        public void Commit() => _context.Database.CurrentTransaction?.Commit();

        public void Rollback() => _context.Database.CurrentTransaction?.Rollback();

        public void Dispose()
        {
         }
    }



}
