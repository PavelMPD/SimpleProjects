using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator;

namespace DbMigration
{
    [Migration(201403201511)]
    public class CreateTablesAndRelations : Migration
    {
        public override void Up()
        {
            Execute.Script(@"SqlScripts\201403201511\CreateTables.sql");
        }

        public override void Down()
        {
            Execute.Script(@"SqlScripts\201403201511\DropTables.sql");
        }
    }
}
