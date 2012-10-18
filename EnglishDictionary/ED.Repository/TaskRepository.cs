using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ED.Domen.Entities;
using ED.Repository.Interfaces;

namespace ED.Repository
{
    public class TaskRepository : IRepository<Task>
    {

        public Task GetEntity(int id)
        {
            return new Task();
        }

        public void Save(Task entity)
        {
            
        }


        public IEnumerable<Task> GetList()
        {
            return new Task[] {
                new Task() { Id = 1, Caption = "Набор 1" },
                new Task() { Id = 2, Caption = "Набор 2" }
            };
        }
    }
}
