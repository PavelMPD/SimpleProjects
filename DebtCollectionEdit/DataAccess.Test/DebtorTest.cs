using System;
using System.Data.Objects;
using System.Diagnostics;
using DebtCollection.Model.Enums;
using NUnit.Framework;
using System.Linq;
using DebtCollection.Model;
using DebtCollection.Common;

namespace DataAccess.Test
{
    [TestFixture]
    public class DebtorTest
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [TearDown]
        public void TearDown()
        {
            
        }

        [Test]
        public void GetDebtor()
        {
            var dbContext = new ApplicationDbContext();
            var debtor = dbContext.Debtors.Where(d => d.Id == 1);
            Trace.Write(debtor.ToTraceString());
        }

        [Test]
        public void GetEndorsment()
        {
            var dbContext = new ApplicationDbContext();
            var endorsment = dbContext.Endorsements.Where(d => d.Id == 1);
            Trace.Write(endorsment.ToTraceString());
        }
        
        [Test]
        public void DebtorsJoinEndorsements()
        {
            var dbContext = new ApplicationDbContext();
            var query = dbContext.Debtors.Join(dbContext.Endorsements, d => d.Id, e => e.Id, (d, e) => new { Debtor = d, Endorsement = e }).Count();
            Trace.Write(query);
            //Trace.Write(query.ToTraceString());
        }
    }
}
