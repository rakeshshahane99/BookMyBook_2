using BookMyBook_DataAccess.Repository.IRepository;
using BookMyBook_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookMyBook_DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private ApplicationDBContext _db;
        public CategoryRepository(ApplicationDBContext db):base(db)
        {
            _db = db;
        }       
        public void Update(Category category)
        {
            _db.Categories.Update(category);
        }
    }
}
