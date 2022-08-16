using BookMyBook_DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookMyBook_DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        readonly ApplicationDBContext _db;
        internal DbSet<T> dbSet;
        public Repository(ApplicationDBContext db)
        {
            _db = db;
            //_db.Products.Include(u => u.Category).Include(u => u.CoverType);
            //_db.ShoppingCarts.Include(u=>u.Product)
            this.dbSet = _db.Set<T>();
        }
        public void Add(T entity)
        {
            dbSet.Add(entity);
        }
        //Inclue properties="Category, CoverType"
        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter=null, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if (filter!=null)
            {
                query = query.Where(filter);
            }
           
            if (includeProperties != null)
            {
                foreach (var includeprop in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeprop);
                }
            }
            return query.ToList();
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked=true)
        {
            IQueryable<T> query;
            if (tracked)
            {
                query = dbSet;
            }
            else 
            {
            query=dbSet.AsNoTracking();
            }
            if (includeProperties != null)
            {
                foreach (var includeprop in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeprop);
                }
            }
            query =query.Where(filter);
            return query.FirstOrDefault();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity);
        }
    }
}
