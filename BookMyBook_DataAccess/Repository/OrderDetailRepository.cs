using BookMyBook_DataAccess.Repository.IRepository;
using BookMyBook_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookMyBook_DataAccess.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private ApplicationDBContext _db;
        public OrderDetailRepository(ApplicationDBContext db):base(db)
        {
            _db = db;
        }       
        public void Update(OrderDetail orderDetail)
        {
            _db.OrderDetails.Update(orderDetail);
        }
    }
}
