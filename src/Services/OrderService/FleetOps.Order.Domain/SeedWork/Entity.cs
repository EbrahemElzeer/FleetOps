using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Domain.SeedWork
{
    public abstract class Entity
    {
        public Guid Id { get;protected set; }

        protected Entity()
        {
            
        }


        protected Entity(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(
                    "Entity ID cannot be empty.",
                    nameof(id));

            Id = id;
        }


    }
}
