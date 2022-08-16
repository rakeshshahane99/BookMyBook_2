using BookMyBook_DataAccess.Repository.IRepository;
using BookMyBook_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookMyBook_DataAccess.Repository
{
    public class CoverTypeRepository : Repository<CoverType>, ICoverTypeRepository
    {
        private ApplicationDBContext _db;
        public CoverTypeRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }

        public void Update(CoverType coverType)
        {
            _db.CoverTypes.Update(coverType);
        }   

    }
}
