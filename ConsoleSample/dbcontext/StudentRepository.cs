using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSample.dbcontext
{
    internal class StudentRepository : SimpleClient<Student>
    {
        public StudentRepository(ISqlSugarClient client)
        {
            this.Context = client;
        }
    }
}
