using BookMyBook_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookMyBook_DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader orderHeader);

        void UpdateStatus(int id, string orderStatus, string payementStatus=null);
        void UpdateStripePayementId(int id, string sessionId, string payementIntentId);

    }
}
