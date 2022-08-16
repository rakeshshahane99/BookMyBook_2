using BookMyBook_DataAccess.Repository.IRepository;
using BookMyBook_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookMyBook_DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDBContext _db;
        public OrderHeaderRepository(ApplicationDBContext db):base(db)
        {
            _db = db;
        }       
        public void Update(OrderHeader orderHeader)
        {
            _db.OrderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderStatus, string? payementStatus = null)
        {
           var orderFromDb=_db.OrderHeaders.FirstOrDefault(u=>u.Id == id);
            if (orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if (payementStatus!=null)
                {
                    orderFromDb.PayementStatus = payementStatus;
                }
            }
        }
        public void UpdateStripePayementId(int id, string sessionId, string payementIntentId )
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            orderFromDb.SessionId = sessionId;
            orderFromDb.PayementIntentId = payementIntentId;
        }
    }
}
