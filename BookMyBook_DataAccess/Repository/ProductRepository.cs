using BookMyBook_DataAccess.Repository.IRepository;
using BookMyBook_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookMyBook_DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDBContext _db;
        public ProductRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product product)
        {
            var objFromDb = _db.Products.FirstOrDefault(x => x.Id == product.Id);
            if (objFromDb == null) 
            {
                objFromDb.Title = product.Title;
                objFromDb.Description = product.Description;   
                objFromDb.ISBN = product.ISBN;
                objFromDb.Author = product.Author;
                objFromDb.Price = product.Price;
                objFromDb.ListPrice = product.ListPrice;
                objFromDb.Price50= product.Price50;
                objFromDb.Price100 = product.Price100;
                objFromDb.CategoryId= product.CategoryId;
                objFromDb.CoverTypeId= product.CoverTypeId; 
                if (objFromDb.ImageUrl!=null)
                {
                    objFromDb.ImageUrl = product.ImageUrl;
                }

            }


            _db.Products.Update(product);
        }   

    }
}
